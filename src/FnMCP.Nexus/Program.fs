module FnMCP.Nexus.Program

open System
open System.IO
open System.Text
open System.Text.Json
open System.Text.Json.Serialization
open FnMCP.Nexus
open FnMCP.Nexus.Types
open FnMCP.Nexus.ContentProvider
open FnMCP.Nexus.FileSystemProvider
open FnMCP.Nexus.McpServer

// JSON serialization options
let jsonOptions = JsonSerializerOptions()
jsonOptions.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
jsonOptions.WriteIndented <- false
jsonOptions.DefaultIgnoreCondition <- JsonIgnoreCondition.WhenWritingNull

// Log to stderr (stdout is for JSON-RPC communication)
let log message =
    Console.Error.WriteLine($"[FnMCP.Nexus] {message}")

// Process a single JSON-RPC request (payload is a full JSON object string)
let processRequest (server: McpServer) (payload: string) = async {
    // Try to extract ID from the raw JSON first, in case parsing fails
    let tryExtractId () =
        try
            use doc = JsonDocument.Parse(payload)
            let root = doc.RootElement
            let mutable idProp = Unchecked.defaultof<JsonElement>
            if root.TryGetProperty("id", &idProp) then
                match idProp.ValueKind with
                | JsonValueKind.String -> Some (box (idProp.GetString()))
                | JsonValueKind.Number ->
                    // support both int and long ids
                    try Some (box (idProp.GetInt64())) with _ -> Some (box (idProp.GetInt32()))
                | JsonValueKind.Null -> None
                | _ -> None
            else None
        with _ -> None

    try
        // Parse JSON-RPC request
        let jsonRpcRequest = JsonSerializer.Deserialize<JsonRpcRequest>(payload, jsonOptions)
        log $"Received request: {jsonRpcRequest.Method}"

        match jsonRpcRequest.Id with
        | None ->
            // Notifications don't get responses
            log $"Processing notification: {jsonRpcRequest.Method}"
            let! _ = server.HandleRequest(jsonRpcRequest)
            return None
        | Some _ ->
            // Regular requests get responses
            let! result = server.HandleRequest(jsonRpcRequest)
            let response = server.CreateResponse(jsonRpcRequest.Id, result)
            let responseJson = JsonSerializer.Serialize(response, jsonOptions)
            return Some responseJson
    with ex ->
        log $"Error processing request: {ex.Message}"
        let requestId = tryExtractId ()
        match requestId with
        | Some id ->
            let errorResponse = {
                Jsonrpc = "2.0"
                Id = Some id
                Result = None
                Error = Some (box {
                    Code = ErrorCodes.ParseError
                    Message = $"Parse error: {ex.Message}"
                    Data = None
                })
            }
            return Some (JsonSerializer.Serialize(errorResponse, jsonOptions))
        | None ->
            log "No ID found in malformed request, skipping error response"
            return None
}

// Reader that supports JSON-RPC over stdio with Content-Length framing (LSP-style)
// and falls back to NDJSON with multi-line support.
let readNextMessage (sr: StreamReader) = async {
    // Helper: read headers until blank line; returns map and true when headers were present
    let! firstLine = sr.ReadLineAsync() |> Async.AwaitTask
    if isNull firstLine then
        return None
    else
        let lineTrim = firstLine.Trim()
        let isHeaderStart = lineTrim.StartsWith("Content-Length", StringComparison.OrdinalIgnoreCase)
                            || lineTrim.Contains(":")
        if isHeaderStart && not (lineTrim.StartsWith("{") || lineTrim.StartsWith("[")) then
            // Parse headers
            let mutable contentLengthOpt: int option = None
            let mutable line = firstLine
            let mutable doneHeaders = false
            let mutable headerBytes = 0
            while not doneHeaders do
                // Parse header line
                let idx = line.IndexOf(':')
                if idx > 0 then
                    let key = line.Substring(0, idx).Trim()
                    let value = line.Substring(idx + 1).Trim()
                    if key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase) then
                        let ok, v = Int32.TryParse(value)
                        if ok then contentLengthOpt <- Some v
                // Read next header line
                let! l = sr.ReadLineAsync() |> Async.AwaitTask
                if isNull l || l.Length = 0 then doneHeaders <- true else line <- l
            match contentLengthOpt with
            | Some len when len >= 0 ->
                // Read len characters (best effort; JSON typically ASCII). If fewer are read, continue until satisfied.
                // Note: Content-Length is bytes; with UTF-8 ASCII this matches chars; for non-ASCII payloads this may slightly over-read/under-read.
                let buffer = Array.zeroCreate<char> len
                let mutable offset = 0
                while offset < len do
                    let! read = sr.ReadAsync(buffer, offset, len - offset) |> Async.AwaitTask
                    if read = 0 then offset <- len else offset <- offset + read
                let payload = new string(buffer, 0, len)
                return Some payload
            | _ ->
                // No usable Content-Length; fall back to NDJSON accumulation starting with an empty line
                let sb = StringBuilder()
                let mutable depth = 0
                let mutable inStr = false
                let mutable esc = false
                let mutable started = false
                let rec readMore () = async {
                    let! l = sr.ReadLineAsync() |> Async.AwaitTask
                    if isNull l then return () else
                    if not started then
                        // look for start char in this line
                        for ch in l do
                            match ch with
                            | '{' | '[' when not inStr -> depth <- depth + 1; started <- true
                            | '}' | ']' when not inStr -> depth <- Math.Max(0, depth - 1)
                            | '"' -> if not esc then inStr <- not inStr; esc <- false
                            | '\\' -> esc <- not esc
                            | _ -> esc <- false
                            sb.Append(ch) |> ignore
                        sb.Append('\n') |> ignore
                    else
                        for ch in l do
                            match ch with
                            | '"' -> if not esc then inStr <- not inStr; esc <- false
                            | '\\' -> esc <- not esc
                            | '{' | '[' when not inStr -> depth <- depth + 1; esc <- false
                            | '}' | ']' when not inStr -> depth <- Math.Max(0, depth - 1); esc <- false
                            | _ -> esc <- false
                            sb.Append(ch) |> ignore
                        sb.Append('\n') |> ignore
                    if started && depth = 0 && not inStr then return () else return! readMore ()
                }
                do! readMore ()
                return Some (sb.ToString())
        else
            // NDJSON or pretty-printed JSON starting immediately
            let sb = StringBuilder()
            let mutable depth = 0
            let mutable inStr = false
            let mutable esc = false
            let addLine (l: string) =
                for ch in l do
                    match ch with
                    | '"' -> if not esc then inStr <- not inStr; esc <- false
                    | '\\' -> esc <- not esc
                    | '{' | '[' when not inStr -> depth <- depth + 1; esc <- false
                    | '}' | ']' when not inStr -> depth <- Math.Max(0, depth - 1); esc <- false
                    | _ -> esc <- false
                    sb.Append(ch) |> ignore
                sb.Append('\n') |> ignore
            addLine firstLine
            // Continue reading until we've closed all braces/brackets and are not inside a string
            let mutable doneReading = depth = 0 && not inStr
            while not doneReading do
                let! l = sr.ReadLineAsync() |> Async.AwaitTask
                if isNull l then
                    // EOF reached; stop with what we have (may be partial)
                    doneReading <- true
                else
                    addLine l
                    doneReading <- (depth = 0 && not inStr)
            return Some (sb.ToString())
}

// CLI mode argument parser
let parseCliArgs (args: string array) =
    let rec parseArgs (remaining: string list) (acc: Map<string, obj>) =
        match remaining with
        | [] -> acc
        | flag :: value :: rest when flag.StartsWith("--") ->
            let key = flag.Substring(2).Replace("-", "_")  // Remove "--" and normalize hyphens to underscores
            // Try to parse as JSON array/object, otherwise treat as string
            let parsedValue =
                if value.StartsWith("[") || value.StartsWith("{") then
                    try
                        let jsonElement = JsonSerializer.Deserialize<JsonElement>(value)
                        box jsonElement
                    with _ -> box value
                else
                    box value
            parseArgs rest (Map.add key parsedValue acc)
        | flag :: rest when flag.StartsWith("--") ->
            // Flag without value - treat as true
            let key = flag.Substring(2).Replace("-", "_")  // Normalize hyphens to underscores
            parseArgs rest (Map.add key (box true) acc)
        | unexpected :: rest ->
            // Non-flag argument - skip or could error
            parseArgs rest acc

    parseArgs (Array.toList args) Map.empty

// CLI mode execution
let runCliMode (contextLibraryPath: string) (command: string) (args: string array) =
    try
        // Parse CLI arguments into map
        let argsMap = parseCliArgs args

        // Convert map to JsonElement for tool execution
        let jsonString = JsonSerializer.Serialize(argsMap)
        let jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonString)

        // Execute tool
        match ToolRegistry.executeTool contextLibraryPath command (Some (box jsonElement)) with
        | Ok content ->
            // Print tool output - serialize to JSON then extract text
            for item in content do
                let itemJson = JsonSerializer.Serialize(item)
                let itemElement = JsonSerializer.Deserialize<JsonElement>(itemJson)
                let mutable textProp = Unchecked.defaultof<JsonElement>
                if itemElement.TryGetProperty("text", &textProp) then
                    Console.WriteLine(textProp.GetString())
            0
        | Error err ->
            Console.Error.WriteLine($"Error: {err}")
            1
    with ex ->
        Console.Error.WriteLine($"CLI Error: {ex.Message}")
        Console.Error.WriteLine($"Stack trace: {ex.StackTrace}")
        1

[<EntryPoint>]
let main argv =
    try
        // Load environment variables from .env file (if it exists)
        // This must be done before any configuration is read
        try
            DotNetEnv.Env.Load() |> ignore
            log "Loaded environment variables from .env file"
        with
        | :? FileNotFoundException ->
            log "No .env file found - using environment variables and defaults"
        | ex ->
            log $"Warning: Failed to load .env file: {ex.Message}"

        // Detect mode: CLI (2+ args) vs MCP (0-1 args)
        match argv with
        | args when args.Length >= 2 ->
            // CLI Mode
            let knownCommands = Set.ofList [
                "create-event"; "create_event"
                "timeline-projection"; "timeline_projection"
                "capture-idea"; "capture_idea"
                "enhance-nexus"; "enhance_nexus"
                "record-learning"; "record_learning"
                "lookup-pattern"; "lookup_pattern"
                "lookup-error-solution"; "lookup_error_solution"
                "update-documentation"; "update_documentation"
            ]

            // Determine if first arg is context path or command
            let contextPath, command, remainingArgs =
                if knownCommands.Contains(args.[0]) || knownCommands.Contains(args.[0].Replace("-", "_")) then
                    // First arg is command, use default context path
                    let projectRoot = Path.GetDirectoryName(AppContext.BaseDirectory)
                    let defaultPath = Path.Combine(projectRoot, "context-library")
                    let cmd = args.[0].Replace("-", "_")  // Normalize to underscore
                    defaultPath, cmd, args.[1..]
                else
                    // First arg is context path, second is command
                    let cmd = if args.Length > 1 then args.[1].Replace("-", "_") else ""
                    args.[0], cmd, if args.Length > 2 then args.[2..] else [||]

            if String.IsNullOrEmpty(command) then
                Console.Error.WriteLine("Error: No command specified")
                Console.Error.WriteLine("Usage: nexus [context-path] <command> [--arg value ...]")
                Console.Error.WriteLine("Commands: create-event, timeline-projection, enhance-nexus, record-learning, lookup-pattern, lookup-error-solution, update-documentation")
                1
            else
                runCliMode contextPath command remainingArgs

        | _ ->
            // MCP Server Mode
            let contextLibraryPath =
                match argv with
                | [| path |] -> path
                | _ ->
                    // Default to context-library relative to binary location
                    let projectRoot = Path.GetDirectoryName(AppContext.BaseDirectory)
                    Path.Combine(projectRoot, "context-library")

            log "FnMCP.Nexus MCP Server starting..."
            log $"Protocol version: 2024-11-05"
            log $"Context library path: {contextLibraryPath}"

            if not (Directory.Exists(contextLibraryPath)) then
                log $"Warning: Context library directory does not exist: {contextLibraryPath}"
                log "Server will start but no resources will be available until the directory is created."

            // Create provider and server
            let provider = FileSystemProvider(contextLibraryPath) :> IContentProvider
            let server = McpServer(provider, contextLibraryPath)

            log "Server initialized. Ready to receive JSON-RPC requests on stdin."
            log "Logging to stderr. JSON-RPC responses on stdout."

            use sr = new StreamReader(Console.OpenStandardInput(), Encoding.UTF8)
            let rec loop () = async {
                let! msgOpt = readNextMessage sr
                match msgOpt with
                | None ->
                    log "EOF received, shutting down."
                    return ()
                | Some payload ->
                    let! responseJsonOpt = processRequest server payload
                    match responseJsonOpt with
                    | Some json ->
                        Console.WriteLine(json)
                        Console.Out.Flush()
                    | None -> ()
                    return! loop ()
            }
            loop () |> Async.RunSynchronously
            log "Server shutting down."
            0
    with ex ->
        Console.Error.WriteLine($"Fatal error: {ex.Message}")
        Console.Error.WriteLine($"Stack trace: {ex.StackTrace}")
        1