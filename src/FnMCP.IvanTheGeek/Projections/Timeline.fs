module FnMCP.IvanTheGeek.Projections.Timeline

open System
open System.IO
open System.Text
open FnMCP.IvanTheGeek.Domain
open FnMCP.IvanTheGeek.Domain.Projections

// Timeline projection: chronologically sorted events

let readTimeline (basePath: string) : TimelineItem list =
    // Read from both old and new project-scoped paths
    let allDirs = [
        Path.Combine(basePath, "nexus", "events", "domain", "active")  // Old path (backward compat)
        Path.Combine(basePath, "nexus", "events", "core", "domain", "active")
        Path.Combine(basePath, "nexus", "events", "laundrylog", "domain", "active")
        Path.Combine(basePath, "nexus", "events", "perdiem", "domain", "active")
        Path.Combine(basePath, "nexus", "events", "fnmcp-nexus", "domain", "active")
    ]

    allDirs
    |> List.collect (fun dir ->
        if Directory.Exists(dir) then
            Directory.GetFiles(dir, "*.md", SearchOption.AllDirectories) |> Array.toList
        else [])
    |> List.choose tryParseEvent
    |> List.sortBy (fun i -> i.OccurredAt)

let generateEvolutionMarkdown (basePath: string) : string =
    let timeline = readTimeline basePath
    let sb = StringBuilder()

    sb.AppendLine("# Nexus Evolution Timeline") |> ignore
    sb.AppendLine() |> ignore
    sb.AppendLine("**Generated:** " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")) |> ignore
    sb.AppendLine() |> ignore
    sb.AppendLine("This timeline shows the chronological evolution of the Nexus knowledge system through captured events.") |> ignore
    sb.AppendLine() |> ignore

    if List.isEmpty timeline then
        sb.AppendLine("_No events recorded yet._") |> ignore
    else
        sb.AppendLine($"**Total Events:** {timeline.Length}") |> ignore
        sb.AppendLine() |> ignore

        // Group by month
        let grouped = timeline |> List.groupBy (fun e -> e.OccurredAt.ToString("yyyy-MM"))

        for (month, events) in grouped do
            sb.AppendLine($"## {month}") |> ignore
            sb.AppendLine() |> ignore

            for event in events do
                let date = event.OccurredAt.ToString("yyyy-MM-dd")
                let time = event.OccurredAt.ToString("HH:mm")
                sb.AppendLine($"### {date} {time} - {event.Title}") |> ignore
                sb.AppendLine() |> ignore
                sb.AppendLine($"**Type:** {event.Type}") |> ignore
                sb.AppendLine() |> ignore
                sb.AppendLine($"**File:** `{Path.GetFileName(event.Path)}`") |> ignore
                sb.AppendLine() |> ignore

            sb.AppendLine() |> ignore

    sb.ToString()

let writeEvolutionFile (basePath: string) : string =
    let projectionDir = Path.Combine(basePath, "nexus", "projections", "timeline")
    if not (Directory.Exists(projectionDir)) then
        Directory.CreateDirectory(projectionDir) |> ignore

    let content = generateEvolutionMarkdown basePath
    let filePath = Path.Combine(projectionDir, "evolution.md")
    File.WriteAllText(filePath, content, Encoding.UTF8)
    filePath

// ============================================================================
// PHASE 2: Regenerate with system events and metadata
// ============================================================================

let regenerateTimeline (basePath: string) : string =
    let startTime = DateTime.Now
    let timeline = readTimeline basePath
    let outputPath = writeEvolutionFile basePath
    let duration = DateTime.Now - startTime

    // Emit ProjectionRegenerated system event
    let systemEvent : SystemEventMeta = {
        Id = Guid.NewGuid()
        Type = ProjectionRegenerated
        OccurredAt = DateTime.Now
        EventId = None
        EventType = None
        ProjectionType = Some ProjectionType.Timeline
        Duration = Some duration
        EventCount = Some timeline.Length
        Staleness = None
        ToolName = None
        Success = None
    }
    EventWriter.writeSystemEvent basePath None systemEvent |> ignore

    // Update projection registry
    let registryEntry = {
        Registry.Name = "timeline"
        Registry.Path = Path.GetDirectoryName(outputPath)
        Registry.Type = ProjectionType.Timeline
        Registry.LastRegenerated = DateTime.Now
        Registry.Staleness = Fresh
    }
    Registry.RegistryIO.updateProjection basePath registryEntry

    // Write .meta.yaml
    let meta : ProjectionMeta = {
        ProjectionType = ProjectionType.Timeline
        LastRegenerated = DateTime.Now
        SourceEventCount = timeline.Length
        Staleness = Fresh
    }
    ProjectionMetadata.writeMetaFile (Path.GetDirectoryName(outputPath)) meta

    outputPath
