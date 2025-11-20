namespace FnMCP.Nexus

open System
open System.IO
open System.Text.Json
open FnMCP.Nexus.Types
open FnMCP.Nexus.Tools
open FnMCP.Nexus.Auth.ApiKeyService
open FnMCP.Nexus.Domain

// Tool definitions for the MCP server
module ToolRegistry =

    let getTools (contextLibraryPath: string) : Tool list =
        [
            {
                Name = "update_documentation"
                Description = Some "Update or create markdown files in the context library. Supports full file rewrites or appending content."
                InputSchema = box {|
                    ``type`` = "object"
                    properties = {|
                        path = {|
                            ``type`` = "string"
                            description = "Relative path within context-library (e.g., 'framework/overview.md' or 'apps/laundrylog/overview.md')"
                        |}
                        content = {|
                            ``type`` = "string"
                            description = "The markdown content to write"
                        |}
                        mode = {|
                            ``type`` = "string"
                            description = "Update mode: 'overwrite' (replace entire file) or 'append' (add to end)"
                            ``enum`` = [| "overwrite"; "append" |]
                            ``default`` = "overwrite"
                        |}
                    |}
                    required = [| "path"; "content" |]
                |}
            };
            EventTools.createEventTool;
            EventTools.timelineProjectionTool;
            EventTools.captureIdeaTool;
            EnhanceNexus.enhanceNexusTool;
            Learning.recordLearningTool;
            Learning.lookupPatternTool;
            Learning.lookupErrorSolutionTool;
            SemanticSearch.searchKnowledgeTool;
            {
                Name = "generate_api_key"
                Description = Some "Generate a new API key for remote MCP access via HTTPS (SSE/WebSocket). Keys are stored securely as hashed events."
                InputSchema = box {|
                    ``type`` = "object"
                    properties = {|
                        scope = {|
                            ``type`` = "string"
                            description = "Access scope for the key"
                            ``enum`` = [| "full_access"; "read_only"; "files_only_public" |]
                        |}
                        description = {|
                            ``type`` = "string"
                            description = "Human-readable description of this key's purpose (e.g., 'Claude Web access', 'Mobile app')"
                        |}
                        expires_in_days = {|
                            ``type`` = "integer"
                            description = "Optional: Number of days until key expires (omit for no expiration)"
                        |}
                    |}
                    required = [| "scope"; "description" |]
                |}
            };
        ]

    // Tool execution handlers
    let handleUpdateDocumentation (contextLibraryPath: string) (args: JsonElement) : Result<string, string> =
        try
            // Extract parameters
            let path = args.GetProperty("path").GetString()
            let content = args.GetProperty("content").GetString()
            let mutable modeProp = Unchecked.defaultof<JsonElement>
            let mode =
                if args.TryGetProperty("mode", &modeProp) then
                    modeProp.GetString()
                else
                    "overwrite"

            // Validate path safety - must be within context library
            let fullPath = Path.Combine(contextLibraryPath, path)
            let normalizedPath = Path.GetFullPath(fullPath)
            let normalizedBase = Path.GetFullPath(contextLibraryPath)

            if not (normalizedPath.StartsWith(normalizedBase)) then
                Error "Invalid path: must be within context-library directory"
            else
                // Ensure directory exists
                let directory = Path.GetDirectoryName(normalizedPath)
                if not (Directory.Exists(directory)) then
                    Directory.CreateDirectory(directory) |> ignore

                // Write content based on mode
                match mode with
                | "overwrite" ->
                    File.WriteAllText(normalizedPath, content)
                    Ok $"Successfully updated {path} (overwrite)"
                | "append" ->
                    File.AppendAllText(normalizedPath, "\n" + content)
                    Ok $"Successfully appended to {path}"
                | _ ->
                    Error $"Invalid mode: {mode}. Use 'overwrite' or 'append'"

        with
        | ex -> Error $"Failed to update documentation: {ex.Message}"

    // Main tool execution router
    let executeTool (contextLibraryPath: string) (name: string) (arguments: obj option) : Result<obj list, string> =
        match arguments with
        | None -> Error "Missing tool arguments"
        | Some args ->
            let jsonElement = args :?> JsonElement

            match name with
            | "update_documentation" ->
                match handleUpdateDocumentation contextLibraryPath jsonElement with
                | Ok message -> Ok [ box {| ``type`` = "text"; text = message |} ]
                | Error err -> Error err
            | "create_event" ->
                match EventTools.handleCreateEvent contextLibraryPath jsonElement with
                | Ok message -> Ok [ box {| ``type`` = "text"; text = message |} ]
                | Error err -> Error err
            | "timeline_projection" ->
                match EventTools.handleTimelineProjection contextLibraryPath with
                | Ok txt -> Ok [ box {| ``type`` = "text"; text = txt |} ]
                | Error err -> Error err
            | "capture_idea" ->
                match EventTools.handleCaptureIdea contextLibraryPath jsonElement with
                | Ok message -> Ok [ box {| ``type`` = "text"; text = message |} ]
                | Error err -> Error err
            | "enhance_nexus" ->
                let result = EnhanceNexus.handleEnhanceNexus contextLibraryPath jsonElement
                let success = result["success"] :?> bool
                if success then
                    let eventsCreated = result["events_created"] :?> int
                    let projectionsRegenerated = result["projections_regenerated"] :?> string list
                    let projList = String.Join(", ", projectionsRegenerated)
                    let message = $"✓ Enhanced Nexus: Created {eventsCreated} events, regenerated {projectionsRegenerated.Length} projections: {projList}"
                    Ok [ box {| ``type`` = "text"; text = message |} ]
                else
                    Error (result["error"] :?> string)
            | "record_learning" ->
                match Learning.handleRecordLearning contextLibraryPath jsonElement with
                | Ok message -> Ok [ box {| ``type`` = "text"; text = message |} ]
                | Error err -> Error err
            | "lookup_pattern" ->
                match Learning.handleLookupPattern contextLibraryPath jsonElement with
                | Ok txt -> Ok [ box {| ``type`` = "text"; text = txt |} ]
                | Error err -> Error err
            | "lookup_error_solution" ->
                match Learning.handleLookupErrorSolution contextLibraryPath jsonElement with
                | Ok txt -> Ok [ box {| ``type`` = "text"; text = txt |} ]
                | Error err -> Error err
            | "search_knowledge" ->
                match SemanticSearch.handleSearchKnowledge contextLibraryPath jsonElement with
                | Ok txt -> Ok [ box {| ``type`` = "text"; text = txt |} ]
                | Error err -> Error err
            | "generate_api_key" ->
                try
                    // Parse arguments
                    let scopeStr = jsonElement.GetProperty("scope").GetString()
                    let description = jsonElement.GetProperty("description").GetString()

                    let mutable expiresInDaysProp = Unchecked.defaultof<JsonElement>
                    let expiresInDays =
                        if jsonElement.TryGetProperty("expires_in_days", &expiresInDaysProp) then
                            Some (expiresInDaysProp.GetInt32())
                        else
                            None

                    // Parse scope
                    let scope =
                        match scopeStr with
                        | "full_access" -> ApiKeyScope.FullAccess
                        | "read_only" -> ApiKeyScope.ReadOnly
                        | "files_only_public" -> ApiKeyScope.FilesOnly [SecurityClassification.Public]
                        | _ -> ApiKeyScope.ReadOnly // Default to read-only for safety

                    // Generate key
                    let (key, keyId) = generateApiKey contextLibraryPath scope description expiresInDays "user"

                    // Format response with prominent warning
                    let expiryText =
                        match expiresInDays with
                        | Some days -> $" (expires in {days} days)"
                        | None -> " (no expiration)"

                    let message = $"""
API Key Generated Successfully!
================================

⚠️  IMPORTANT: Save this key now - it will never be shown again!

API Key: {key}
Key ID: {keyId}
Scope: {scopeStr}{expiryText}
Description: {description}

To use this key with Claude Web/Desktop/Mobile:
1. Add to your MCP client configuration
2. Use as: Authorization: Bearer {key}
3. Connect to: https://mcp.nexus.ivanthegeek.com/mcp (HTTP streaming)
   Or legacy SSE: https://mcp.nexus.ivanthegeek.com/sse

Example curl test:
curl -H "Authorization: Bearer {key}" https://mcp.nexus.ivanthegeek.com/health
"""

                    Ok [ box {| ``type`` = "text"; text = message |} ]
                with
                | ex -> Error $"Failed to generate API key: {ex.Message}"
            | _ -> Error $"Unknown tool: {name}"
