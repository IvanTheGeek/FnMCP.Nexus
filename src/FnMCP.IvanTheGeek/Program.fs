module FnMCP.IvanTheGeek.Program

open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open FnMCP.IvanTheGeek.Types
open FnMCP.IvanTheGeek.ContentProvider
open FnMCP.IvanTheGeek.FileSystemProvider
open FnMCP.IvanTheGeek.McpServer

// JSON serialization options
let jsonOptions = JsonSerializerOptions()
jsonOptions.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
jsonOptions.WriteIndented <- false
jsonOptions.DefaultIgnoreCondition <- JsonIgnoreCondition.WhenWritingNull

// Log to stderr (stdout is for JSON-RPC communication)
let log message =
    Console.Error.WriteLine($"[FnMCP.IvanTheGeek] {message}")

// Process a single JSON-RPC request
let processRequest (server: McpServer) (requestLine: string) = async {
    // Try to extract ID from the raw JSON first, in case parsing fails
    let tryExtractId () =
        try
            use doc = JsonDocument.Parse(requestLine)
            let root = doc.RootElement
            let mutable idProp = Unchecked.defaultof<JsonElement>
            if root.TryGetProperty("id", &idProp) then
                match idProp.ValueKind with
                | JsonValueKind.String -> Some (box (idProp.GetString()))
                | JsonValueKind.Number -> Some (box (idProp.GetInt32()))
                | JsonValueKind.Null -> None
                | _ -> None
            else
                None
        with
        | _ -> None
    
    try
        // Parse JSON-RPC request
        let jsonRpcRequest = JsonSerializer.Deserialize<JsonRpcRequest>(requestLine, jsonOptions)
        
        log $"Received request: {jsonRpcRequest.Method}"
        
        // Check if this is a notification (no id field)
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
        
    with
    | ex ->
        log $"Error processing request: {ex.Message}"
        // Try to get the ID from the raw JSON
        let requestId = tryExtractId ()
        
        // Only return an error response if we have an ID (not a notification)
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
            // No ID means it was likely a notification or completely malformed
            log "No ID found in malformed request, skipping error response"
            return None
}

[<EntryPoint>]
let main argv =
    try
        // Get context library path from config or argument
        let contextLibraryPath = 
            match argv with
            | [| path |] -> path
            | _ -> 
                // Default to context-library in project root
                let projectRoot = Path.GetDirectoryName(Path.GetDirectoryName(__SOURCE_DIRECTORY__))
                Path.Combine(projectRoot, "context-library")

        log "FnMCP.IvanTheGeek MCP Server starting..."
        log $"Protocol version: 2025-06-18"
        log $"Context library path: {contextLibraryPath}"

        // Check if context library exists
        if not (Directory.Exists(contextLibraryPath)) then
            log $"Warning: Context library directory does not exist: {contextLibraryPath}"
            log "Server will start but no resources will be available until the directory is created."

        // Create provider and server
        let provider = FileSystemProvider(contextLibraryPath) :> IContentProvider
        let server = McpServer(provider)

        log "Server initialized. Ready to receive JSON-RPC requests on stdin."
        log "Logging to stderr. JSON-RPC responses on stdout."

        // Main message loop - read from stdin, write to stdout
        let rec messageLoop () =
            let line = Console.ReadLine()
            if line <> null then
                // Process request
                let responseJson = processRequest server line |> Async.RunSynchronously
                
                // Write response to stdout (only if not a notification)
                match responseJson with
                | Some json ->
                    Console.WriteLine(json)
                    Console.Out.Flush()
                | None ->
                    // Notification - no response needed
                    ()
                
                // Continue loop
                messageLoop ()
            else
                log "EOF received, shutting down."

        // Start the message loop
        messageLoop ()

        log "Server shutting down."
        0  // Exit code

    with
    | ex ->
        log $"Fatal error: {ex.Message}"
        log $"Stack trace: {ex.StackTrace}"
        1  // Error exit code