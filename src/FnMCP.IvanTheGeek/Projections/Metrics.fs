module FnMCP.IvanTheGeek.Projections.Metrics

open System
open System.IO
open System.Text
open FnMCP.IvanTheGeek.Domain
open FnMCP.IvanTheGeek.Domain.Projections.FrontMatterParser

// ============================================================================
// PHASE 2: Metrics Projection - Statistics from system events
// ============================================================================

type MetricsData = {
    GeneratedAt: DateTime
    TotalEventsCreated: int
    TotalProjectionsRegenerated: int
    TotalToolInvocations: int
    ToolSuccesses: int
    ToolFailures: int
    MostRecentProjectionRegen: (ProjectionType * DateTime * int) option  // (type, timestamp, event_count)
}

module MetricsReader =
    // Parse a system event YAML file
    let private parseSystemEvent (path: string) : (SystemEventType * Map<string, string>) option =
        try
            let content = File.ReadAllText(path)
            let (fm, _) = parseFrontMatter content

            match fm.TryFind("type") with
            | Some typeStr ->
                let eventType =
                    match typeStr with
                    | "EventCreated" -> Some EventCreated
                    | "ProjectionRegenerated" -> Some ProjectionRegenerated
                    | "ProjectionQueried" -> Some ProjectionQueried
                    | "ToolInvoked" -> Some ToolInvoked
                    | _ -> None

                match eventType with
                | Some et -> Some (et, fm)
                | None -> None
            | None -> None
        with _ -> None

    // Scan all system events and compute metrics
    let computeMetrics (basePath: string) : MetricsData =
        let systemEventsDir = Path.Combine(basePath, "nexus", "events", "system", "active")

        if not (Directory.Exists(systemEventsDir)) then
            {
                GeneratedAt = DateTime.Now
                TotalEventsCreated = 0
                TotalProjectionsRegenerated = 0
                TotalToolInvocations = 0
                ToolSuccesses = 0
                ToolFailures = 0
                MostRecentProjectionRegen = None
            }
        else
            let files = Directory.GetFiles(systemEventsDir, "*.yaml", SearchOption.AllDirectories)

            let mutable eventsCreated = 0
            let mutable projectionsRegenerated = 0
            let mutable toolInvocations = 0
            let mutable toolSuccesses = 0
            let mutable toolFailures = 0
            let mutable mostRecentRegen : (ProjectionType * DateTime * int) option = None

            for file in files do
                match parseSystemEvent file with
                | Some (EventCreated, _) ->
                    eventsCreated <- eventsCreated + 1

                | Some (ProjectionRegenerated, fm) ->
                    projectionsRegenerated <- projectionsRegenerated + 1

                    // Track most recent
                    let occurred = fm.TryFind("occurred_at") |> Option.bind (fun s ->
                        match DateTime.TryParse(s) with
                        | true, dt -> Some dt
                        | _ -> None)
                    let projType = fm.TryFind("projection_type") |> Option.bind (fun s ->
                        try Some (ProjectionType.Parse(s)) with _ -> None)
                    let eventCount = fm.TryFind("event_count") |> Option.bind (fun s ->
                        match Int32.TryParse(s) with
                        | true, n -> Some n
                        | _ -> None)

                    match occurred, projType, eventCount, mostRecentRegen with
                    | Some dt, Some pt, Some ec, None ->
                        mostRecentRegen <- Some (pt, dt, ec)
                    | Some dt, Some pt, Some ec, Some (_, prevDt, _) when dt > prevDt ->
                        mostRecentRegen <- Some (pt, dt, ec)
                    | _ -> ()

                | Some (ToolInvoked, fm) ->
                    toolInvocations <- toolInvocations + 1
                    match fm.TryFind("success") with
                    | Some "True" -> toolSuccesses <- toolSuccesses + 1
                    | Some "False" -> toolFailures <- toolFailures + 1
                    | _ -> ()

                | Some (ProjectionQueried, _) ->
                    ()  // Not counted in current metrics

                | None -> ()

            {
                GeneratedAt = DateTime.Now
                TotalEventsCreated = eventsCreated
                TotalProjectionsRegenerated = projectionsRegenerated
                TotalToolInvocations = toolInvocations
                ToolSuccesses = toolSuccesses
                ToolFailures = toolFailures
                MostRecentProjectionRegen = mostRecentRegen
            }

module MetricsWriter =
    // Write metrics to statistics.yaml
    let writeMetrics (basePath: string) (metrics: MetricsData) : string =
        let outputDir = Path.Combine(basePath, "nexus", "projections", "metrics")
        if not (Directory.Exists(outputDir)) then
            Directory.CreateDirectory(outputDir) |> ignore

        let outputPath = Path.Combine(outputDir, "statistics.yaml")

        let timestamp = metrics.GeneratedAt.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK")
        let sb = StringBuilder()

        sb.AppendLine("---") |> ignore
        sb.AppendLine($"generated_at: {timestamp}") |> ignore
        sb.AppendLine($"total_events_created: {metrics.TotalEventsCreated}") |> ignore
        sb.AppendLine($"total_projections_regenerated: {metrics.TotalProjectionsRegenerated}") |> ignore
        sb.AppendLine($"total_tool_invocations: {metrics.TotalToolInvocations}") |> ignore
        sb.AppendLine($"tool_successes: {metrics.ToolSuccesses}") |> ignore
        sb.AppendLine($"tool_failures: {metrics.ToolFailures}") |> ignore

        match metrics.MostRecentProjectionRegen with
        | Some (projType, dt, eventCount) ->
            let recentTimestamp = dt.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK")
            sb.AppendLine("most_recent_projection_regen:") |> ignore
            sb.AppendLine($"  projection_type: {projType.AsString}") |> ignore
            sb.AppendLine($"  timestamp: {recentTimestamp}") |> ignore
            sb.AppendLine($"  event_count: {eventCount}") |> ignore
        | None ->
            sb.AppendLine("most_recent_projection_regen: null") |> ignore

        File.WriteAllText(outputPath, sb.ToString(), Text.Encoding.UTF8)
        outputPath

    // Regenerate the metrics projection
    let regenerateMetrics (basePath: string) : string =
        let startTime = DateTime.Now
        let metrics = MetricsReader.computeMetrics basePath
        let outputPath = writeMetrics basePath metrics
        let duration = DateTime.Now - startTime

        // Emit ProjectionRegenerated system event
        let systemEvent = {
            Id = Guid.NewGuid()
            Type = ProjectionRegenerated
            OccurredAt = DateTime.Now
            EventId = None
            EventType = None
            ProjectionType = Some Metrics
            Duration = Some duration
            EventCount = None  // Not applicable for metrics
            Staleness = None
            ToolName = None
            Success = None
        }
        EventWriter.writeSystemEvent basePath None systemEvent |> ignore

        // Update projection registry
        let registryEntry = {
            Registry.Name = "metrics"
            Registry.Path = Path.GetDirectoryName(outputPath)
            Registry.Type = Metrics
            Registry.LastRegenerated = DateTime.Now
            Registry.Staleness = Fresh  // Always fresh since we just regenerated
        }
        Registry.RegistryIO.updateProjection basePath registryEntry

        // Also write .meta.yaml
        let meta : Projections.ProjectionMeta = {
            ProjectionType = Metrics
            LastRegenerated = DateTime.Now
            SourceEventCount = metrics.TotalEventsCreated + metrics.TotalProjectionsRegenerated + metrics.TotalToolInvocations
            Staleness = Fresh
        }
        Projections.ProjectionMetadata.writeMetaFile (Path.GetDirectoryName(outputPath)) meta

        outputPath
