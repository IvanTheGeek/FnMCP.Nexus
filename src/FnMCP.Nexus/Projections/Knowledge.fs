module FnMCP.Nexus.Projections.Knowledge

open System
open System.IO
open System.Text
open FnMCP.Nexus.Domain
open FnMCP.Nexus.Domain.Projections.FrontMatterParser
open FnMCP.Nexus.Projections.Registry

// ============================================================================
// PHASE 3: Knowledge Projections - F# Learning from events
// ============================================================================

// Parsed learning event data
type ParsedLearningEvent = {
    Meta: Map<string, string>
    Body: string
    EventType: LearningEventType
    OccurredAt: DateTime
}

// Pattern data aggregated from events
type PatternData = {
    Name: string
    Category: PatternCategory option
    Title: string
    Description: string
    CodeExample: string option
    UseCount: int
    SuccessCount: int
    FailureCount: int
    Confidence: float
    LastValidated: DateTime
    RelatedErrors: string list
    Tags: string list
}

// Error solution data
type ErrorSolutionData = {
    ErrorCode: string
    ErrorMessage: string option
    Occurrences: int
    Solutions: string list
    SuccessRate: float
    RelatedPatterns: string list
    LastEncountered: DateTime
}

module KnowledgeReader =
    // Parse a learning event file
    let private parseLearningEvent (path: string) : ParsedLearningEvent option =
        try
            let content = File.ReadAllText(path)
            let (fm, body) = parseFrontMatter content

            match fm.TryFind("type") with
            | Some typeStr ->
                let eventType = LearningEventType.Parse(typeStr)
                let occurredStr = fm.TryFind("occurred_at") |> Option.defaultValue ""
                let ok, dt = DateTime.TryParse(occurredStr)
                let occurredAt = if ok then dt else File.GetCreationTime(path)

                Some {
                    Meta = fm
                    Body = body.Trim()
                    EventType = eventType
                    OccurredAt = occurredAt
                }
            | None -> None
        with _ -> None

    // Read all learning events
    let readLearningEvents (basePath: string) : ParsedLearningEvent list =
        let learningDir = Path.Combine(basePath, "nexus", "events", "learning", "active")
        if not (Directory.Exists(learningDir)) then []
        else
            Directory.GetFiles(learningDir, "*.md", SearchOption.AllDirectories)
            |> Array.choose parseLearningEvent
            |> Array.sortBy (fun e -> e.OccurredAt)
            |> Array.toList

    // Aggregate pattern data from events
    let aggregatePatterns (events: ParsedLearningEvent list) : PatternData list =
        events
        |> List.filter (fun e ->
            match e.EventType with
            | PatternDiscovered | PatternValidated | SolutionApplied -> true
            | _ -> false)
        |> List.groupBy (fun e -> e.Meta.TryFind("pattern_name") |> Option.defaultValue "unknown")
        |> List.filter (fun (name, _) -> name <> "unknown")
        |> List.map (fun (patternName, patternEvents) ->
            let latestEvent = patternEvents |> List.maxBy (fun e -> e.OccurredAt)
            let title = latestEvent.Meta.TryFind("title") |> Option.defaultValue patternName
            let category = latestEvent.Meta.TryFind("pattern_category") |> Option.bind (fun s ->
                try Some (PatternCategory.Parse(s)) with _ -> None)

            let useCount =
                patternEvents
                |> List.choose (fun e -> e.Meta.TryFind("use_count"))
                |> List.choose (fun s -> match Int32.TryParse(s) with | true, n -> Some n | _ -> None)
                |> List.tryLast
                |> Option.defaultValue patternEvents.Length

            let successRate =
                patternEvents
                |> List.choose (fun e -> e.Meta.TryFind("success_rate"))
                |> List.choose (fun s -> match Double.TryParse(s) with | true, n -> Some n | _ -> None)
                |> List.tryLast
                |> Option.defaultValue 1.0

            let successCount = int (float useCount * successRate)
            let failureCount = useCount - successCount

            let relatedErrors =
                patternEvents
                |> List.choose (fun e -> e.Meta.TryFind("error_code"))
                |> List.distinct

            let tags =
                patternEvents
                |> List.collect (fun e ->
                    e.Meta.TryFind("tags") |> Option.defaultValue "" |> fun s -> s.Split(',') |> Array.toList)
                |> List.filter (fun s -> not (String.IsNullOrWhiteSpace(s)))
                |> List.distinct

            {
                Name = patternName
                Category = category
                Title = title
                Description = latestEvent.Body
                CodeExample = None  // Extract from body if needed
                UseCount = useCount
                SuccessCount = successCount
                FailureCount = failureCount
                Confidence = successRate
                LastValidated = latestEvent.OccurredAt
                RelatedErrors = relatedErrors
                Tags = tags
            }
        )
        |> List.sortByDescending (fun p -> p.Confidence)

    // Aggregate error solution data
    let aggregateErrorSolutions (events: ParsedLearningEvent list) : ErrorSolutionData list =
        events
        |> List.filter (fun e ->
            match e.EventType with
            | ErrorEncountered | SolutionApplied -> true
            | _ -> false)
        |> List.choose (fun e ->
            match e.Meta.TryFind("error_code") with
            | Some errorCode -> Some (errorCode, e)
            | None -> None)
        |> List.groupBy fst
        |> List.map (fun (errorCode, errorEvents) ->
            let events = errorEvents |> List.map snd
            let latestEvent = events |> List.maxBy (fun e -> e.OccurredAt)

            let errorMessage = latestEvent.Meta.TryFind("error_message")

            let solutionEvents =
                events |> List.filter (fun e -> e.EventType = SolutionApplied)

            let solutions =
                solutionEvents
                |> List.map (fun e -> e.Body)
                |> List.distinct

            let occurrences = events.Length
            let solvedCount = solutionEvents.Length
            let successRate = if occurrences > 0 then float solvedCount / float occurrences else 0.0

            let relatedPatterns =
                solutionEvents
                |> List.choose (fun e -> e.Meta.TryFind("pattern_name"))
                |> List.distinct

            {
                ErrorCode = errorCode
                ErrorMessage = errorMessage
                Occurrences = occurrences
                Solutions = solutions
                SuccessRate = successRate
                RelatedPatterns = relatedPatterns
                LastEncountered = latestEvent.OccurredAt
            }
        )
        |> List.sortByDescending (fun e -> e.Occurrences)

module KnowledgeWriter =
    // Generate patterns.md
    let generatePatternsMarkdown (basePath: string) (patterns: PatternData list) : string =
        let sb = StringBuilder()

        let genTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        sb.AppendLine("# F# Coding Patterns - Nexus Knowledge Base") |> ignore
        sb.AppendLine() |> ignore
        sb.AppendLine($"**Generated:** {genTime}") |> ignore
        sb.AppendLine($"**Total Patterns:** {patterns.Length}") |> ignore
        sb.AppendLine() |> ignore

        if patterns.IsEmpty then
            sb.AppendLine("_No patterns discovered yet._") |> ignore
        else
            // Group by category
            let grouped = patterns |> List.groupBy (fun p -> p.Category)

            for (category, categoryPatterns) in grouped do
                let catName = category |> Option.map (fun c -> c.AsString) |> Option.defaultValue "Uncategorized"
                sb.AppendLine($"## {catName}") |> ignore
                sb.AppendLine() |> ignore

                for pattern in categoryPatterns do
                    let stars = String.replicate (int (pattern.Confidence * 5.0)) "⭐"
                    let pct = int (pattern.Confidence * 100.0)
                    let lastVal = pattern.LastValidated.ToString("yyyy-MM-dd")

                    sb.AppendLine($"### {pattern.Title}") |> ignore
                    sb.AppendLine($"**Confidence:** {stars} ({pct}%% success, {pattern.UseCount} uses)") |> ignore
                    sb.AppendLine($"**Last validated:** {lastVal}") |> ignore
                    sb.AppendLine() |> ignore
                    sb.AppendLine(pattern.Description) |> ignore
                    sb.AppendLine() |> ignore

                    if not pattern.RelatedErrors.IsEmpty then
                        let errList = String.Join(", ", pattern.RelatedErrors)
                        sb.AppendLine($"**Related errors:** {errList}") |> ignore
                        sb.AppendLine() |> ignore

                    sb.AppendLine("---") |> ignore
                    sb.AppendLine() |> ignore

        sb.ToString()

    // Generate error-solutions.md
    let generateErrorSolutionsMarkdown (basePath: string) (errors: ErrorSolutionData list) : string =
        let sb = StringBuilder()

        let genTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        sb.AppendLine("# F# Error Solutions - Nexus Knowledge Base") |> ignore
        sb.AppendLine() |> ignore
        sb.AppendLine($"**Generated:** {genTime}") |> ignore
        sb.AppendLine($"**Total Errors Documented:** {errors.Length}") |> ignore
        sb.AppendLine() |> ignore

        if errors.IsEmpty then
            sb.AppendLine("_No errors encountered yet._") |> ignore
        else
            for error in errors do
                sb.AppendLine($"## {error.ErrorCode}") |> ignore
                sb.AppendLine() |> ignore

                error.ErrorMessage |> Option.iter (fun msg ->
                    sb.AppendLine($"**Full Error:** \"{msg}\"") |> ignore
                    sb.AppendLine() |> ignore)

                let pct = int (error.SuccessRate * 100.0)
                sb.AppendLine($"**Occurrences:** {error.Occurrences} times") |> ignore
                sb.AppendLine($"**Solution Success Rate:** {pct}%%") |> ignore
                sb.AppendLine() |> ignore

                if not error.Solutions.IsEmpty then
                    sb.AppendLine("### Solutions") |> ignore
                    sb.AppendLine() |> ignore
                    for solution in error.Solutions do
                        sb.AppendLine(solution) |> ignore
                        sb.AppendLine() |> ignore

                if not error.RelatedPatterns.IsEmpty then
                    let patList = String.Join(", ", error.RelatedPatterns)
                    sb.AppendLine($"**Related patterns:** {patList}") |> ignore
                    sb.AppendLine() |> ignore

                let lastEnc = error.LastEncountered.ToString("yyyy-MM-dd")
                sb.AppendLine($"**Last encountered:** {lastEnc}") |> ignore
                sb.AppendLine() |> ignore
                sb.AppendLine("---") |> ignore
                sb.AppendLine() |> ignore

        sb.ToString()

    // Generate confidence-scores.yaml
    let generateConfidenceScoresYaml (basePath: string) (patterns: PatternData list) (errors: ErrorSolutionData list) : string =
        let sb = StringBuilder()

        let genAt = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK")
        sb.AppendLine("---") |> ignore
        sb.AppendLine($"generated_at: {genAt}") |> ignore
        sb.AppendLine("patterns:") |> ignore

        for pattern in patterns do
            sb.AppendLine($"  - name: {pattern.Name}") |> ignore
            pattern.Category |> Option.iter (fun c -> sb.AppendLine($"    category: {c.AsString}") |> ignore)
            sb.AppendLine($"    confidence: {pattern.Confidence}") |> ignore
            sb.AppendLine($"    use_count: {pattern.UseCount}") |> ignore
            sb.AppendLine($"    success_count: {pattern.SuccessCount}") |> ignore
            sb.AppendLine($"    failure_count: {pattern.FailureCount}") |> ignore
            let lastVal = pattern.LastValidated.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK")
            sb.AppendLine($"    last_validated: {lastVal}") |> ignore

        sb.AppendLine() |> ignore
        sb.AppendLine("errors:") |> ignore

        for error in errors do
            sb.AppendLine($"  - code: {error.ErrorCode}") |> ignore
            sb.AppendLine($"    occurrences: {error.Occurrences}") |> ignore
            sb.AppendLine($"    success_rate: {error.SuccessRate}") |> ignore

        sb.ToString()

    // Write all knowledge projections
    let writeKnowledgeProjections (basePath: string) : string =
        let events = KnowledgeReader.readLearningEvents basePath
        let patterns = KnowledgeReader.aggregatePatterns events
        let errors = KnowledgeReader.aggregateErrorSolutions events

        let outputDir = Path.Combine(basePath, "nexus", "projections", "knowledge")
        if not (Directory.Exists(outputDir)) then
            Directory.CreateDirectory(outputDir) |> ignore

        // Write patterns.md
        let patternsPath = Path.Combine(outputDir, "patterns.md")
        let patternsContent = generatePatternsMarkdown basePath patterns
        File.WriteAllText(patternsPath, patternsContent, Text.Encoding.UTF8)

        // Write error-solutions.md
        let errorsPath = Path.Combine(outputDir, "error-solutions.md")
        let errorsContent = generateErrorSolutionsMarkdown basePath errors
        File.WriteAllText(errorsPath, errorsContent, Text.Encoding.UTF8)

        // Write confidence-scores.yaml
        let confidencePath = Path.Combine(outputDir, "confidence-scores.yaml")
        let confidenceContent = generateConfidenceScoresYaml basePath patterns errors
        File.WriteAllText(confidencePath, confidenceContent, Text.Encoding.UTF8)

        outputDir

    // Regenerate knowledge projections (with system events and registry)
    let regenerateKnowledge (basePath: string) : string =
        let startTime = DateTime.Now
        let outputPath = writeKnowledgeProjections basePath
        let duration = DateTime.Now - startTime

        // Emit ProjectionRegenerated system event
        let systemEvent : SystemEventMeta = {
            Id = Guid.NewGuid()
            Type = ProjectionRegenerated
            OccurredAt = DateTime.Now
            EventId = None
            EventType = None
            ProjectionType = Some ProjectionType.Knowledge
            Duration = Some duration
            EventCount = None
            Staleness = None
            ToolName = None
            Success = None
        }
        EventWriter.writeSystemEvent basePath None systemEvent |> ignore

        // Update projection registry
        let registryEntry : RegistryEntry = {
            Name = "knowledge"
            Path = outputPath
            Type = ProjectionType.Knowledge
            LastRegenerated = DateTime.Now
            Staleness = Fresh
        }
        RegistryIO.updateProjection basePath registryEntry

        // Write .meta.yaml
        let events = KnowledgeReader.readLearningEvents basePath
        let meta : Projections.ProjectionMeta = {
            ProjectionType = ProjectionType.Knowledge
            LastRegenerated = DateTime.Now
            SourceEventCount = events.Length
            Staleness = Fresh
        }
        Projections.ProjectionMetadata.writeMetaFile outputPath meta

        outputPath
