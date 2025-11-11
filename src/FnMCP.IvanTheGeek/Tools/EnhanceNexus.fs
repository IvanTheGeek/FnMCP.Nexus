module FnMCP.IvanTheGeek.Tools.EnhanceNexus

open System
open System.Collections.Generic
open System.Text.Json
open FnMCP.IvanTheGeek.Types
open FnMCP.IvanTheGeek.Domain
open FnMCP.IvanTheGeek.Domain.EventWriter

// ============================================================================
// PHASE 2: EnhanceNexus - High-level workflow tool for Claude
// ============================================================================

// Tool definition
let enhanceNexusTool : Tool = {
    Name = "enhance_nexus"
    Description = Some "High-level workflow: Create multiple events and regenerate projections in a single operation. Use this when you want to document rich context about the conversation or project state."
    InputSchema = box {|
        ``type`` = "object"
        properties = {|
            events = {|
                ``type`` = "array"
                description = "List of events to create"
                items = {|
                    ``type`` = "object"
                    properties = {|
                        ``type`` = {|
                            ``type`` = "string"
                            description = "Event type (optional, defaults to ContextEnhancement)"
                        |}
                        title = {|
                            ``type`` = "string"
                            description = "Event title"
                        |}
                        narrative = {|
                            ``type`` = "string"
                            description = "Markdown narrative body"
                        |}
                        tags = {|
                            ``type`` = "array"
                            items = {| ``type`` = "string" |}
                            description = "List of tags"
                        |}
                    |}
                |}
            |}
            regenerate_projections = {|
                ``type`` = "array"
                description = "List of projection names to regenerate (e.g., ['timeline', 'metrics'])"
                items = {| ``type`` = "string" |}
            |}
        |}
        required = [| "events" |]
    |}
}

// Event specification for batch creation
type EventSpec = {
    Type: string option
    Title: string option
    Narrative: string option
    Tags: string list option
}

// Result of enhance_nexus operation
type EnhanceResult = {
    EventsCreated: int
    EventPaths: string list
    ProjectionsRegenerated: string list
    ProjectionPaths: string list
}

// Parse EventSpec from JSON
let parseEventSpec (json: JsonElement) : EventSpec =
    let tryGetString (name: string) =
        match json.TryGetProperty(name) with
        | true, prop when prop.ValueKind <> JsonValueKind.Null -> Some (prop.GetString())
        | _ -> None

    let tryGetStringList (name: string) =
        match json.TryGetProperty(name) with
        | true, prop when prop.ValueKind = JsonValueKind.Array ->
            Some ([
                for item in prop.EnumerateArray() do
                    if item.ValueKind = JsonValueKind.String then
                        yield item.GetString()
            ])
        | _ -> None

    {
        Type = tryGetString "type"
        Title = tryGetString "title"
        Narrative = tryGetString "narrative"
        Tags = tryGetStringList "tags"
    }

// Create a single event from spec
let private createEventFromSpec (basePath: string) (spec: EventSpec) : string =
    let eventTypeStr = spec.Type |> Option.defaultValue "FrameworkInsight"
    let eventType = EventType.Parse(eventTypeStr)
    let title = spec.Title |> Option.defaultValue "Enhanced Nexus Context"
    let narrative = spec.Narrative |> Option.defaultValue ""
    let tags = spec.Tags |> Option.defaultValue []

    let eventId = Guid.NewGuid()
    let meta : EventMeta = {
        Id = eventId
        Type = eventType
        Title = title
        Summary = None
        OccurredAt = DateTime.Now
        Tags = tags
        Author = None
        Links = []
        Technical = None
    }

    let eventPath = EventWriter.writeEventFile basePath None meta narrative

    // Emit EventCreated system event
    let systemEvent = {
        SystemEventMeta.Id = Guid.NewGuid()
        Type = EventCreated
        OccurredAt = DateTime.Now
        EventId = Some eventId
        EventType = Some eventTypeStr
        ProjectionType = None
        Duration = None
        EventCount = None
        Staleness = None
        ToolName = None
        Success = None
    }
    EventWriter.writeSystemEvent basePath None systemEvent |> ignore

    eventPath

// Main enhance_nexus workflow
let enhanceNexus (basePath: string) (eventSpecs: EventSpec list) (projectionsToRegen: string list) : EnhanceResult =
    let startTime = DateTime.Now

    // Phase 1: Create all events
    let eventPaths = eventSpecs |> List.map (createEventFromSpec basePath)

    // Phase 2: Regenerate specified projections
    let projectionResults = ResizeArray<string * string>()

    for projName in projectionsToRegen do
        let projPath =
            match projName.ToLower() with
            | "timeline" ->
                FnMCP.IvanTheGeek.Projections.Timeline.regenerateTimeline basePath
            | "metrics" ->
                FnMCP.IvanTheGeek.Projections.Metrics.MetricsWriter.regenerateMetrics basePath
            | _ ->
                ""  // Unknown projection type

        if projPath <> "" then
            projectionResults.Add((projName, projPath))

    // Phase 3: Emit ToolInvoked system event for enhance_nexus itself
    let duration = DateTime.Now - startTime
    let toolInvokedEvent = {
        SystemEventMeta.Id = Guid.NewGuid()
        Type = ToolInvoked
        OccurredAt = DateTime.Now
        EventId = None
        EventType = None
        ProjectionType = None
        Duration = Some duration
        EventCount = Some eventPaths.Length
        Staleness = None
        ToolName = Some "enhance_nexus"
        Success = Some true
    }
    EventWriter.writeSystemEvent basePath None toolInvokedEvent |> ignore

    // Return results
    {
        EventsCreated = eventPaths.Length
        EventPaths = eventPaths
        ProjectionsRegenerated = [for (name, _) in projectionResults -> name]
        ProjectionPaths = [for (_, path) in projectionResults -> path]
    }

// Format result as JSON response
let formatResult (result: EnhanceResult) : Dictionary<string, obj> =
    let dict = Dictionary<string, obj>()
    dict["events_created"] <- result.EventsCreated :> obj
    dict["event_paths"] <- result.EventPaths :> obj
    dict["projections_regenerated"] <- result.ProjectionsRegenerated :> obj
    dict["projection_paths"] <- result.ProjectionPaths :> obj
    dict["success"] <- true :> obj
    dict

// Tool handler for MCP
let handleEnhanceNexus (basePath: string) (arguments: JsonElement) : Dictionary<string, obj> =
    try
        // Parse events array
        let eventSpecs =
            match arguments.TryGetProperty("events") with
            | true, eventsArray when eventsArray.ValueKind = JsonValueKind.Array ->
                [
                    for eventJson in eventsArray.EnumerateArray() do
                        yield parseEventSpec eventJson
                ]
            | _ -> []

        // Parse projections array
        let projections =
            match arguments.TryGetProperty("regenerate_projections") with
            | true, projArray when projArray.ValueKind = JsonValueKind.Array ->
                [
                    for projJson in projArray.EnumerateArray() do
                        if projJson.ValueKind = JsonValueKind.String then
                            yield projJson.GetString()
                ]
            | _ -> []

        // Execute workflow
        let result = enhanceNexus basePath eventSpecs projections

        formatResult result

    with ex ->
        let errorDict = Dictionary<string, obj>()
        errorDict["success"] <- false :> obj
        errorDict["error"] <- ex.Message :> obj
        errorDict
