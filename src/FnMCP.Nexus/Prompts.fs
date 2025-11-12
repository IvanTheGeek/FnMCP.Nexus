module FnMCP.Nexus.Prompts

open System
open System.IO
open FnMCP.Nexus.Types
open FnMCP.Nexus.Domain
open FnMCP.Nexus.Domain.Projections
open FnMCP.Nexus.Projections.PendingIdeas

// Define available prompts for each project
let getPromptList () : Prompt list = [
    {
        Name = "continue-core"
        Description = Some "Continue working on Nexus core development"
        Arguments = None
    }
    {
        Name = "continue-laundrylog"
        Description = Some "Continue working on LaundryLog project"
        Arguments = None
    }
    {
        Name = "continue-perdiem"
        Description = Some "Continue working on PerDiem project"
        Arguments = None
    }
    {
        Name = "continue-fnmcp-nexus"
        Description = Some "Continue working on FnMCP.Nexus MCP server"
        Arguments = None
    }
]

// Read the latest SessionState event for a specific project
let readLatestSessionState (basePath: string) (project: string) : TimelineItem option =
    let projectPath = Path.Combine(basePath, "nexus", "events", project, "domain", "active")

    if Directory.Exists(projectPath) then
        Directory.GetFiles(projectPath, "*SessionState*.md", SearchOption.AllDirectories)
        |> Array.toList
        |> List.choose tryParseEvent
        |> List.filter (fun e -> e.Type = "SessionState")
        |> List.sortByDescending (fun e -> e.OccurredAt)
        |> List.tryHead
    else
        None

// Generate continuation context for a project
let generateContinuationContext (basePath: string) (project: string) : string =
    // Get pending ideas for this project
    let pendingIdeas = readPendingIdeas basePath project
    let ideasSection =
        if List.isEmpty pendingIdeas then
            ""
        else
            sprintf """

## Pending Cross-Project Ideas (%d)

You have ideas from other projects that might be relevant:

%s
"""
                pendingIdeas.Length
                (formatPendingIdeasSummary pendingIdeas)

    match readLatestSessionState basePath project with
    | Some sessionEvent ->
        // Read the actual event file to get the full content
        let content =
            try
                File.ReadAllText(sessionEvent.Path)
            with
            | _ -> "Unable to read session details."

        sprintf """# Continue %s Session

**Last Session:** %s
**Occurred:** %s

## Session Context

%s%s

---

*Use the MCP resources to read project documentation and recent events for full context.*
"""
            (project.ToUpper())
            sessionEvent.Title
            (sessionEvent.OccurredAt.ToString("yyyy-MM-dd HH:mm"))
            content
            ideasSection
    | None ->
        sprintf """# Start %s Session

No previous session found for this project.

## Getting Started

- Use MCP resources to read project documentation
- Review recent events in the timeline
- Check current project status%s

---

*This is the first session for %s.*
"""
            (project.ToUpper())
            ideasSection
            project

// Handle prompts/get request
let handleGetPrompt (basePath: string) (promptName: string) : GetPromptResponse =
    let projectMap =
        [
            ("continue-core", "core")
            ("continue-laundrylog", "laundrylog")
            ("continue-perdiem", "perdiem")
            ("continue-fnmcp-nexus", "fnmcp-nexus")
        ]
        |> Map.ofList

    match projectMap |> Map.tryFind promptName with
    | Some project ->
        let content = generateContinuationContext basePath project
        {
            Description = Some $"Continue {project} session with latest context"
            Messages = [
                box {|
                    role = "user"
                    content = box {|
                        ``type`` = "text"
                        text = content
                    |}
                |}
            ]
        }
    | None ->
        {
            Description = Some "Unknown prompt"
            Messages = [
                box {|
                    role = "user"
                    content = box {|
                        ``type`` = "text"
                        text = $"Prompt not found: {promptName}"
                    |}
                |}
            ]
        }
