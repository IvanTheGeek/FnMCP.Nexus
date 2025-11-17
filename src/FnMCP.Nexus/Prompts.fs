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
    {
        Name = "status-check"
        Description = Some "Check current project status from quick-start/project-status.md"
        Arguments = None
    }
    {
        Name = "update-status"
        Description = Some "Update project status with current phase, progress, and blockers"
        Arguments = Some [
            box {|
                name = "phase"
                description = "Current project phase"
                required = true
            |}
            box {|
                name = "last_completed"
                description = "What was recently completed"
                required = true
            |}
            box {|
                name = "what_works"
                description = "What's currently working"
                required = true
            |}
            box {|
                name = "next_up"
                description = "What's planned next"
                required = true
            |}
            box {|
                name = "blocked_by"
                description = "Current blockers (optional, use 'none' if no blockers)"
                required = false
            |}
        ]
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
let handleGetPrompt (basePath: string) (promptName: string) (args: Map<string, obj> option) : GetPromptResponse =
    match promptName with
    | "status-check" ->
        let statusFile = Path.Combine(basePath, "nexus", "quick-start", "project-status.md")
        let content =
            if File.Exists(statusFile) then
                try
                    File.ReadAllText(statusFile)
                with
                | ex -> $"Error reading status file: {ex.Message}"
            else
                "Status file not found. Use the 'update-status' prompt to create it."

        {
            Description = Some "Current project status"
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

    | "update-status" ->
        match args with
        | Some argMap ->
            let getArg key =
                argMap
                |> Map.tryFind key
                |> Option.map (fun v -> v.ToString())
                |> Option.defaultValue ""

            let phase = getArg "phase"
            let lastCompleted = getArg "last_completed"
            let whatWorks = getArg "what_works"
            let nextUp = getArg "next_up"
            let blockedBy = getArg "blocked_by"

            let blockedSection =
                if String.IsNullOrWhiteSpace(blockedBy) || blockedBy.ToLower() = "none" then
                    ""
                else
                    sprintf "\n### Blocked By 🚧\n%s\n" blockedBy

            let today = DateTime.Now.ToString("yyyy-MM-dd")

            let updateInstructions =
                sprintf "# Update Project Status\n\nPlease update the project status file at `quick-start/project-status.md` with the following information:\n\n```markdown\n# Nexus MCP - Current Project Status\n\n**Last Updated**: %s\n**Current Phase**: %s\n**Project Location**: `/home/linux/FnMCP.Nexus`\n\n## Quick Status Check\n\n### What's Working ✅\n%s\n\n### What's Next 🎯\n%s\n%s\n### Recently Completed\n%s\n\n### Key Locations\n```\nProject Root:    /home/linux/FnMCP.Nexus/\nMCP Resources:   /home/linux/Nexus-Data/\nWorking Binary:  /home/linux/FnMCP.Nexus/bin/publish_single/nexus\nVPS:             66.179.208.238\nDomains:         mcp.nexus.ivanthegeek.com\n```\n```\n\nUse the `update_documentation` tool to write this content to `quick-start/project-status.md`."
                    today phase whatWorks nextUp blockedSection lastCompleted

            {
                Description = Some "Update project status documentation"
                Messages = [
                    box {|
                        role = "user"
                        content = box {|
                            ``type`` = "text"
                            text = updateInstructions
                        |}
                    |}
                ]
            }
        | None ->
            {
                Description = Some "Missing arguments for update-status"
                Messages = [
                    box {|
                        role = "user"
                        content = box {|
                            ``type`` = "text"
                            text = "Error: update-status requires arguments: phase, last_completed, what_works, next_up, blocked_by (optional)"
                        |}
                    |}
                ]
            }

    | _ ->
        // Handle project continuation prompts
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
