module FnMCP.IvanTheGeek.Tools.Learning

open System
open System.Collections.Generic
open System.Text.Json
open FnMCP.IvanTheGeek.Types
open FnMCP.IvanTheGeek.Domain
open FnMCP.IvanTheGeek.Domain.EventWriter
open FnMCP.IvanTheGeek.Projections.Knowledge

// ============================================================================
// PHASE 3: Learning Tools - Record and query F# knowledge
// ============================================================================

// Tool definition for recording learning
let recordLearningTool : Tool = {
    Name = "record_learning"
    Description = Some "Record a learning event (pattern, error solution, lesson) to build F# knowledge base"
    InputSchema = box {|
        ``type`` = "object"
        properties = {|
            event_type = {|
                ``type`` = "string"
                description = "Type of learning: error_encountered | solution_applied | pattern_discovered | lesson_learned"
            |}
            title = {|
                ``type`` = "string"
                description = "Title of the learning"
            |}
            description = {|
                ``type`` = "string"
                description = "Detailed description with code examples (markdown)"
            |}
            error_code = {|
                ``type`` = "string"
                description = "F# error code (e.g., FS3373, FS0039) - for errors only"
            |}
            pattern_name = {|
                ``type`` = "string"
                description = "Pattern identifier (e.g., interpolated-string-extraction) - for patterns only"
            |}
            category = {|
                ``type`` = "string"
                description = "Pattern category: Syntax | Types | Modules | Architecture | etc"
            |}
            tags = {|
                ``type`` = "array"
                items = {| ``type`` = "string" |}
                description = "Tags for categorization"
            |}
        |}
        required = [| "event_type"; "title"; "description" |]
    |}
}

// Tool definition for lookup_pattern
let lookupPatternTool : Tool = {
    Name = "lookup_pattern"
    Description = Some "Search F# coding patterns by name, category, or tag"
    InputSchema = box {|
        ``type`` = "object"
        properties = {|
            query = {|
                ``type`` = "string"
                description = "Search query (pattern name or keyword)"
            |}
        |}
    |}
}

// Tool definition for lookup_error_solution
let lookupErrorSolutionTool : Tool = {
    Name = "lookup_error_solution"
    Description = Some "Get solution for F# compiler error code"
    InputSchema = box {|
        ``type`` = "object"
        properties = {|
            error_code = {|
                ``type`` = "string"
                description = "F# error code (e.g., 'FS3373', 'FS0039')"
            |}
        |}
        required = [| "error_code" |]
    |}
}

// Handle record_learning tool
let handleRecordLearning (basePath: string) (args: JsonElement) : Result<string, string> =
    try
        let eventTypeStr = args.GetProperty("event_type").GetString()
        let title = args.GetProperty("title").GetString()
        let description = args.GetProperty("description").GetString()

        let eventType =
            match eventTypeStr.ToLower() with
            | "error_encountered" -> ErrorEncountered
            | "solution_applied" -> SolutionApplied
            | "pattern_discovered" -> PatternDiscovered
            | "lesson_learned" -> LessonLearned
            | _ -> LessonLearned

        let mutable prop = Unchecked.defaultof<JsonElement>

        let errorCode =
            if args.TryGetProperty("error_code", &prop) && prop.ValueKind = JsonValueKind.String then
                Some (prop.GetString())
            else None

        let patternName =
            if args.TryGetProperty("pattern_name", &prop) && prop.ValueKind = JsonValueKind.String then
                Some (prop.GetString())
            else None

        let category =
            if args.TryGetProperty("category", &prop) && prop.ValueKind = JsonValueKind.String then
                try Some (PatternCategory.Parse(prop.GetString())) with _ -> None
            else None

        let tags =
            if args.TryGetProperty("tags", &prop) && prop.ValueKind = JsonValueKind.Array then
                [for item in prop.EnumerateArray() do
                    if item.ValueKind = JsonValueKind.String then yield item.GetString()]
            else []

        let meta : LearningEventMeta = {
            Id = Guid.NewGuid()
            Type = eventType
            Title = title
            Summary = None
            OccurredAt = DateTime.Now
            Tags = tags
            ErrorCode = errorCode
            ErrorMessage = None
            PatternName = patternName
            PatternCategory = category
            UseCount = None
            SuccessRate = None
            FilePath = None
            ConversationContext = None
            RelatedPatterns = []
        }

        let eventPath = writeLearningEvent basePath None meta description
        Ok (sprintf "Learning event recorded: %s" (System.IO.Path.GetFileName(eventPath)))
    with
    | ex -> Error ($"Failed to record learning: {ex.Message}")

// Handle lookup_pattern tool
let handleLookupPattern (basePath: string) (args: JsonElement) : Result<string, string> =
    try
        let query = args.GetProperty("query").GetString().ToLower()

        // Read and filter patterns
        let events = KnowledgeReader.readLearningEvents basePath
        let patterns = KnowledgeReader.aggregatePatterns events

        let matches =
            patterns
            |> List.filter (fun p ->
                p.Name.ToLower().Contains(query) ||
                p.Title.ToLower().Contains(query) ||
                p.Tags |> List.exists (fun t -> t.ToLower().Contains(query)))

        if matches.IsEmpty then
            Ok $"No patterns found matching '{query}'"
        else
            let sb = System.Text.StringBuilder()
            sb.AppendLine($"Found {matches.Length} pattern(s) matching '{query}':\n") |> ignore

            for pattern in matches do
                let pct = int (pattern.Confidence * 100.0)
                sb.AppendLine($"**{pattern.Title}**") |> ignore
                sb.AppendLine($"Confidence: {pct}%% ({pattern.UseCount} uses)") |> ignore
                sb.AppendLine($"\n{pattern.Description}\n") |> ignore

            Ok (sb.ToString())
    with
    | ex -> Error ($"Failed to lookup pattern: {ex.Message}")

// Handle lookup_error_solution tool
let handleLookupErrorSolution (basePath: string) (args: JsonElement) : Result<string, string> =
    try
        let errorCode = args.GetProperty("error_code").GetString()

        // Read and filter errors
        let events = KnowledgeReader.readLearningEvents basePath
        let errors = KnowledgeReader.aggregateErrorSolutions events

        match errors |> List.tryFind (fun e -> e.ErrorCode = errorCode) with
        | Some error ->
            let sb = System.Text.StringBuilder()
            let pct = int (error.SuccessRate * 100.0)
            sb.AppendLine($"## {error.ErrorCode}\n") |> ignore

            error.ErrorMessage |> Option.iter (fun msg ->
                sb.AppendLine($"**Error:** {msg}\n") |> ignore)

            sb.AppendLine($"**Encountered:** {error.Occurrences} times") |> ignore
            sb.AppendLine($"**Success Rate:** {pct}%%\n") |> ignore

            if not error.Solutions.IsEmpty then
                sb.AppendLine("**Solutions:**\n") |> ignore
                for solution in error.Solutions do
                    sb.AppendLine(solution) |> ignore
                    sb.AppendLine() |> ignore

            Ok (sb.ToString())
        | None ->
            Ok $"No solution found for error code '{errorCode}'"
    with
    | ex -> Error ($"Failed to lookup error solution: {ex.Message}")
