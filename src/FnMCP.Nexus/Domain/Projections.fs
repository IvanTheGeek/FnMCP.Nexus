module FnMCP.Nexus.Domain.Projections

open System
open System.IO
open FnMCP.Nexus.Domain

// Projection interfaces and common utilities

// Helper to parse YAML frontmatter
module FrontMatterParser =
    let parseFrontMatter (content: string) =
        // Lightweight parser to extract key-value pairs from YAML frontmatter
        // Returns (map, body)
        let lines = content.Replace("\r\n", "\n").Split('\n')
        if lines.Length = 0 || lines[0].Trim() <> "---" then Map.empty, content
        else
            let mutable i = 1
            let mutable map = Map.empty<string, string>
            let mutable inFront = true
            while i < lines.Length && inFront do
                let line = lines[i]
                if line.Trim() = "---" then inFront <- false
                elif line.StartsWith("  - ") then
                    // ignore list values for simple projection
                    ()
                else
                    let idx = line.IndexOf(':')
                    if idx > 0 then
                        let key = line.Substring(0, idx).Trim()
                        let value = line.Substring(idx + 1).Trim().Trim('"')
                        map <- map.Add(key, value)
                i <- i + 1
            // body
            let body = String.Join("\n", lines[i..])
            map, body

// Common projection utilities
let tryParseEvent (path: string) : TimelineItem option =
    try
        let content = File.ReadAllText(path)
        let (fm, _) = FrontMatterParser.parseFrontMatter content
        if fm.IsEmpty then None else
        let title = fm.TryFind("title") |> Option.defaultValue (Path.GetFileNameWithoutExtension(path))
        let etype = fm.TryFind("type") |> Option.defaultValue "Unknown"
        let occurredStr = fm.TryFind("occurred_at") |> Option.defaultValue ""
        let ok, dt = DateTime.TryParse(occurredStr)
        let dt' = if ok then dt else File.GetCreationTime(path)
        Some { Path = path; Title = title; Type = etype; OccurredAt = dt' }
    with _ -> None

// ============================================================================
// PHASE 2: Projection Metadata - Track staleness and regeneration
// ============================================================================

// Projection metadata stored in .meta.yaml
type ProjectionMeta = {
    ProjectionType: ProjectionType
    LastRegenerated: DateTime
    SourceEventCount: int       // How many events were used
    Staleness: Staleness        // Computed from system events
}

module ProjectionMetadata =
    open System.Text

    // Write projection metadata to .meta.yaml
    let writeMetaFile (projectionPath: string) (meta: ProjectionMeta) : unit =
        let metaPath = Path.Combine(projectionPath, ".meta.yaml")
        let dir = Path.GetDirectoryName(metaPath)
        if not (Directory.Exists(dir)) then
            Directory.CreateDirectory(dir) |> ignore

        let timestamp = meta.LastRegenerated.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK")
        let content = StringBuilder()
        content.AppendLine("---") |> ignore
        content.AppendLine($"projection_type: {meta.ProjectionType.AsString}") |> ignore
        content.AppendLine($"last_regenerated: {timestamp}") |> ignore
        content.AppendLine($"source_event_count: {meta.SourceEventCount}") |> ignore
        content.AppendLine($"staleness: {meta.Staleness.AsString}") |> ignore

        File.WriteAllText(metaPath, content.ToString(), Text.Encoding.UTF8)

    // Read projection metadata from .meta.yaml
    let readMetaFile (projectionPath: string) : ProjectionMeta option =
        let metaPath = Path.Combine(projectionPath, ".meta.yaml")
        if not (File.Exists(metaPath)) then None
        else
            try
                let content = File.ReadAllText(metaPath)
                let (fm, _) = FrontMatterParser.parseFrontMatter content

                let projType =
                    fm.TryFind("projection_type")
                    |> Option.bind (fun s -> try Some (ProjectionType.Parse(s)) with _ -> None)

                let lastRegen =
                    fm.TryFind("last_regenerated")
                    |> Option.bind (fun s ->
                        match DateTime.TryParse(s) with
                        | true, dt -> Some dt
                        | _ -> None)

                let eventCount =
                    fm.TryFind("source_event_count")
                    |> Option.bind (fun s ->
                        match Int32.TryParse(s) with
                        | true, n -> Some n
                        | _ -> None)

                let staleness =
                    match fm.TryFind("staleness") with
                    | Some s when s = "Fresh" -> Fresh
                    | Some s ->
                        let trimmed = s.Trim()
                        if trimmed.StartsWith("Stale(") && trimmed.EndsWith(")") then
                            let numStr = trimmed.Substring(6, trimmed.Length - 7)
                            match Int32.TryParse(numStr) with
                            | true, n -> Stale n
                            | _ -> Fresh
                        else Fresh
                    | None -> Fresh

                match projType, lastRegen, eventCount with
                | Some pt, Some lr, Some ec ->
                    Some { ProjectionType = pt; LastRegenerated = lr; SourceEventCount = ec; Staleness = staleness }
                | _ -> None
            with _ -> None

    // Calculate staleness by counting domain events since last regeneration
    let calculateStaleness (basePath: string) (projType: ProjectionType) (lastRegen: DateTime) : Staleness =
        let domainEventsDir = Path.Combine(basePath, "nexus", "events", "domain", "active")
        if not (Directory.Exists(domainEventsDir)) then Fresh
        else
            try
                let newEventCount =
                    Directory.GetFiles(domainEventsDir, "*.md", SearchOption.AllDirectories)
                    |> Array.filter (fun path ->
                        let fileTime = File.GetLastWriteTime(path)
                        fileTime > lastRegen
                    )
                    |> Array.length

                if newEventCount = 0 then Fresh else Stale newEventCount
            with _ -> Fresh

