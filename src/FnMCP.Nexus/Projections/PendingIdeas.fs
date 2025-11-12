module FnMCP.Nexus.Projections.PendingIdeas

open System
open System.IO
open System.Text
open FnMCP.Nexus.Domain
open FnMCP.Nexus.Domain.Projections

// PendingIdeas projection: show active cross-project ideas for a project

type PendingIdeaItem = {
    Id: Guid
    Title: string
    SourceProject: string
    TargetProject: string
    Idea: string
    Priority: Priority
    Status: IdeaStatus
    OccurredAt: DateTime
    ContextLink: string option
    FilePath: string
}

// Read all CrossProjectIdea events for a specific project
let readPendingIdeas (basePath: string) (project: string) : PendingIdeaItem list =
    let projectPath = Path.Combine(basePath, "nexus", "events", project, "domain", "active")

    if not (Directory.Exists(projectPath)) then
        []
    else
        Directory.GetFiles(projectPath, "*CrossProjectIdea*.md", SearchOption.AllDirectories)
        |> Array.toList
        |> List.choose tryParseEvent
        |> List.filter (fun e -> e.Type = "CrossProjectIdea")
        |> List.choose (fun e ->
            try
                // Read the full file to extract CrossProjectIdea details
                let content = File.ReadAllText(e.Path)

                // Parse YAML frontmatter to extract details
                let lines = content.Split([|'\n'; '\r'|], StringSplitOptions.RemoveEmptyEntries)
                let yamlSection =
                    lines
                    |> Array.skipWhile (fun l -> l.Trim() <> "---")
                    |> Array.skip 1
                    |> Array.takeWhile (fun l -> l.Trim() <> "---")

                let getValue key =
                    yamlSection
                    |> Array.tryFind (fun l -> l.Trim().StartsWith(key + ":"))
                    |> Option.map (fun l ->
                        let idx = l.IndexOf(':')
                        if idx >= 0 && idx < l.Length - 1 then
                            l.Substring(idx + 1).Trim().Trim('"')
                        else "")

                let getNestedValue section key =
                    let inSection = ref false
                    yamlSection
                    |> Array.tryPick (fun l ->
                        if l.Trim() = section + ":" then
                            inSection := true
                            None
                        elif !inSection && l.Trim().StartsWith(key + ":") then
                            let idx = l.IndexOf(':')
                            if idx >= 0 && idx < l.Length - 1 then
                                Some (l.Substring(idx + 1).Trim().Trim('"'))
                            else None
                        elif !inSection && not (l.StartsWith("  ")) then
                            inSection := false
                            None
                        else None)

                let idStr = getValue "id" |> Option.defaultValue ""
                let sourceProject = getNestedValue "cross_project_idea" "source_project" |> Option.defaultValue ""
                let targetProject = getNestedValue "cross_project_idea" "target_project" |> Option.defaultValue ""
                let idea = getNestedValue "cross_project_idea" "idea" |> Option.defaultValue ""
                let priorityStr = getNestedValue "cross_project_idea" "priority" |> Option.defaultValue "Consider"
                let statusStr = getNestedValue "cross_project_idea" "status" |> Option.defaultValue "Pending"
                let contextLink = getNestedValue "cross_project_idea" "context_link"

                if String.IsNullOrWhiteSpace(sourceProject) then
                    None
                else
                    let priority = Priority.Parse(priorityStr)
                    let status = IdeaStatus.Parse(statusStr)

                    // Only include Pending or Exploring ideas
                    match status with
                    | Pending | Exploring ->
                        Some {
                            Id = Guid.Parse(idStr)
                            Title = e.Title
                            SourceProject = sourceProject
                            TargetProject = targetProject
                            Idea = idea
                            Priority = priority
                            Status = status
                            OccurredAt = e.OccurredAt
                            ContextLink = contextLink
                            FilePath = e.Path
                        }
                    | _ -> None
            with _ -> None)
        |> List.sortByDescending (fun i -> i.Priority, i.OccurredAt)

// Format pending ideas for display
let formatPendingIdeas (ideas: PendingIdeaItem list) : string =
    if List.isEmpty ideas then
        "No pending cross-project ideas."
    else
        let sb = StringBuilder()
        sb.AppendLine($"# Pending Cross-Project Ideas ({ideas.Length})") |> ignore
        sb.AppendLine() |> ignore

        // Group by source project
        let bySource = ideas |> List.groupBy (fun i -> i.SourceProject)

        for (source, sourceIdeas) in bySource do
            sb.AppendLine($"## From {source} ({sourceIdeas.Length})") |> ignore
            sb.AppendLine() |> ignore

            for idea in sourceIdeas do
                let priorityBadge =
                    match idea.Priority with
                    | Important -> "**[IMPORTANT]**"
                    | Consider -> "[Consider]"
                    | Low -> "[Low]"

                sb.AppendLine($"### {priorityBadge} {idea.Title}") |> ignore
                sb.AppendLine() |> ignore
                sb.AppendLine($"**Idea:** {idea.Idea}") |> ignore
                sb.AppendLine() |> ignore
                sb.AppendLine($"**Status:** {idea.Status.AsString}") |> ignore
                sb.AppendLine() |> ignore
                let capturedDate = idea.OccurredAt.ToString("yyyy-MM-dd")
                sb.AppendLine($"**Captured:** {capturedDate}") |> ignore
                sb.AppendLine() |> ignore

                match idea.ContextLink with
                | Some link ->
                    sb.AppendLine($"**Context:** {link}") |> ignore
                    sb.AppendLine() |> ignore
                | None -> ()

                sb.AppendLine("---") |> ignore
                sb.AppendLine() |> ignore

        sb.ToString()

// Generate markdown summary for continuation prompts (concise version)
let formatPendingIdeasSummary (ideas: PendingIdeaItem list) : string =
    if List.isEmpty ideas then
        ""
    else
        let sb = StringBuilder()

        for idea in ideas |> List.take (min 5 ideas.Length) do
            let priorityMark = if idea.Priority = Important then "❗" else "•"
            sb.AppendLine($"{priorityMark} **{idea.Title}** (from {idea.SourceProject})") |> ignore
            sb.AppendLine($"  {idea.Idea}") |> ignore

        if ideas.Length > 5 then
            sb.AppendLine($"  _(+{ideas.Length - 5} more ideas)_") |> ignore

        sb.ToString()
