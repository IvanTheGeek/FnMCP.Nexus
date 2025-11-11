namespace FnMCP.IvanTheGeek.Tools

open System
open System.Text
open System.Text.Json
open FnMCP.IvanTheGeek.Types
open FnMCP.IvanTheGeek.Domain
open FnMCP.IvanTheGeek.Domain.EventWriter
open FnMCP.IvanTheGeek.Projections.Timeline

// Tool definitions for event-sourced Nexus
module EventTools =

    let createEventTool : Tool = {
        Name = "create_event"
        Description = Some "Create a Nexus event file with YAML frontmatter and markdown body."
        InputSchema = box {|
            ``type`` = "object"
            properties = {|
                ``type`` = {|
                    ``type`` = "string"
                    description = "Event type: TechnicalDecision | DesignNote | ResearchFinding | FrameworkInsight | MethodologyInsight | NexusInsight | SessionState | CrossProjectIdea"
                |}
                title = {|
                    ``type`` = "string"
                    description = "Short event title used in filename and frontmatter"
                |}
                summary = {|
                    ``type`` = "string"
                    description = "One-line summary"
                |}
                body = {|
                    ``type`` = "string"
                    description = "Markdown narrative body"
                |}
                tags = {|
                    ``type`` = "array"
                    items = {| ``type`` = "string" |}
                    description = "List of tag strings"
                |}
                author = {|
                    ``type`` = "string"
                    description = "Author name or id"
                |}
                links = {|
                    ``type`` = "array"
                    items = {| ``type`` = "string" |}
                    description = "Related links"
                |}
                occurredAt = {|
                    ``type`` = "string"
                    description = "ISO 8601 timestamp; defaults to now"
                |}
                decision_status = {|
                    ``type`` = "string"
                    description = "TechnicalDecision: status (proposed/decided/superseded)"
                |}
                decision = {|
                    ``type`` = "string"
                    description = "TechnicalDecision: decision statement"
                |}
                context = {|
                    ``type`` = "string"
                    description = "TechnicalDecision: context"
                |}
                consequences = {|
                    ``type`` = "string"
                    description = "TechnicalDecision: consequences"
                |}
            |}
            required = [| "type"; "title" |]
        |}
    }

    let timelineProjectionTool : Tool = {
        Name = "timeline_projection"
        Description = Some "Read Nexus domain events and return a simple time-ordered timeline."
        InputSchema = box {|
            ``type`` = "object"
            properties = {||}
        |}
    }

    // Helper JSON extraction functions
    let private tryGetProperty (elem: JsonElement) (name: string) =
        let mutable prop = Unchecked.defaultof<JsonElement>
        if elem.TryGetProperty(name, &prop) then Some prop else None

    let private getStringOpt (elem: JsonElement) (name: string) =
        match tryGetProperty elem name with
        | Some v when v.ValueKind = JsonValueKind.String -> Some (v.GetString())
        | _ -> None

    let private getArrayStrings (elem: JsonElement) (name: string) =
        match tryGetProperty elem name with
        | Some v when v.ValueKind = JsonValueKind.Array ->
            v.EnumerateArray() |> Seq.choose (fun x -> if x.ValueKind = JsonValueKind.String then Some (x.GetString()) else None) |> Seq.toList
        | _ -> []

    // Tool execution handlers
    let handleCreateEvent (basePath: string) (args: JsonElement) : Result<string, string> =
        try
            let etypeStr = args.GetProperty("type").GetString()
            let etype = EventType.Parse(etypeStr)
            let title = args.GetProperty("title").GetString()
            let summary = getStringOpt args "summary"
            let body = getStringOpt args "body" |> Option.defaultValue ""
            let tags = getArrayStrings args "tags"
            let author = getStringOpt args "author"
            let links = getArrayStrings args "links"
            let occurredAt =
                match getStringOpt args "occurredAt" with
                | Some s ->
                    match DateTime.TryParse(s) with
                    | true, dt -> dt
                    | _ -> DateTime.Now
                | None -> DateTime.Now
            let techDetails =
                match etype with
                | TechnicalDecision ->
                    let status = getStringOpt args "decision_status"
                    let decision = getStringOpt args "decision"
                    let context = getStringOpt args "context"
                    let consequences = getStringOpt args "consequences"
                    Some { Status = status; Decision = decision; Context = context; Consequences = consequences }
                | _ -> None
            let eventId = Guid.NewGuid()
            let meta : EventMeta = {
                Id = eventId
                Type = etype
                Title = title
                Summary = summary
                OccurredAt = occurredAt
                Tags = tags
                Author = author
                Links = links
                Technical = techDetails
            }
            let fullPath = writeEventFile basePath meta body

            // PHASE 2: Emit EventCreated system event
            let systemEvent : SystemEventMeta = {
                Id = Guid.NewGuid()
                Type = EventCreated
                OccurredAt = DateTime.Now
                EventId = Some eventId
                EventType = Some etypeStr
                ProjectionType = None
                Duration = None
                EventCount = None
                Staleness = None
                ToolName = None
                Success = None
            }
            EventWriter.writeSystemEvent basePath systemEvent |> ignore

            Ok (sprintf "Event created: %s" (System.IO.Path.GetRelativePath(basePath, fullPath)))
        with
        | ex -> Error ($"Failed to create event: {ex.Message}")

    let handleTimelineProjection (basePath: string) : Result<string, string> =
        try
            let startTime = DateTime.Now
            let items = readTimeline basePath

            // PHASE 2: Emit ProjectionQueried system event
            let duration = DateTime.Now - startTime
            let systemEvent : SystemEventMeta = {
                Id = Guid.NewGuid()
                Type = ProjectionQueried
                OccurredAt = DateTime.Now
                EventId = None
                EventType = None
                ProjectionType = Some ProjectionType.Timeline
                Duration = Some duration
                EventCount = Some items.Length
                Staleness = None
                ToolName = None
                Success = None
            }
            EventWriter.writeSystemEvent basePath systemEvent |> ignore

            if List.isEmpty items then Ok "No events found."
            else
                let sb = StringBuilder()
                sb.AppendLine("Nexus Timeline:") |> ignore
                for it in items do
                    sb.AppendLine(sprintf "- %s | %s | %s" (it.OccurredAt.ToString("yyyy-MM-dd HH:mm:ss")) it.Type it.Title) |> ignore
                Ok (sb.ToString())
        with ex -> Error ($"Failed to read timeline: {ex.Message}")
