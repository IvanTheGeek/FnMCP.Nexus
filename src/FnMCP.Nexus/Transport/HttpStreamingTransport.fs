module FnMCP.Nexus.Transport.HttpStreamingTransport

open System
open System.IO
open System.Text
open System.Text.Json
open System.Text.Json.Serialization
open System.Threading
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Oxpecker
open FnMCP.Nexus.Types
open FnMCP.Nexus.McpServer

// HTTP Streaming transport for MCP
// Uses bidirectional streaming over a single HTTP POST connection
// Messages are exchanged using newline-delimited JSON (NDJSON)

// JSON serialization options
let jsonOptions = JsonSerializerOptions()
jsonOptions.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
jsonOptions.WriteIndented <- false
jsonOptions.DefaultIgnoreCondition <- JsonIgnoreCondition.WhenWritingNull

// Log to stderr
let log message =
    Console.Error.WriteLine($"[HttpStreamingTransport] {message}")

// Read a line from the stream asynchronously
let readLineAsync (reader: StreamReader) (cancellationToken: CancellationToken) : Task<string option> =
    task {
        try
            let! line = reader.ReadLineAsync()
            if line = null then
                return None
            else
                return Some line
        with
        | :? OperationCanceledException -> return None
        | ex ->
            log $"Error reading from stream: {ex.Message}"
            return None
    }

// Write a line to the stream asynchronously
let writeLineAsync (writer: StreamWriter) (content: string) : Task<unit> =
    task {
        do! writer.WriteLineAsync(content)
        do! writer.FlushAsync()
    }

// Handle HTTP streaming endpoint - bidirectional communication over single POST
let handleHttpStreaming (server: McpServer) : EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            log "HTTP streaming connection established"

            // Ensure it's a POST request
            if ctx.Request.Method <> "POST" then
                ctx.Response.StatusCode <- 405 // Method Not Allowed
                ctx.Response.Headers.Add("Allow", "POST")
                do! ctx.Response.WriteAsync("Only POST method is allowed for HTTP streaming")
            else
                // Set response headers for streaming
                ctx.Response.ContentType <- "application/x-ndjson"
                ctx.Response.Headers.Append("Cache-Control", "no-cache")
                ctx.Response.Headers.Append("X-Accel-Buffering", "no") // Disable nginx buffering

                // Generate session ID
                let sessionId = Guid.NewGuid().ToString("N")
                ctx.Response.Headers.Add("Mcp-Session-Id", sessionId)
                log $"Generated session ID: {sessionId}"

                // Create readers/writers for bidirectional streaming
                use requestReader = new StreamReader(ctx.Request.Body)
                use responseWriter = new StreamWriter(ctx.Response.Body)

                // Process messages in a loop
                let mutable shouldContinue = true
                while shouldContinue && not ctx.RequestAborted.IsCancellationRequested do
                    try
                        // Read next line from request stream
                        let! lineOption = readLineAsync requestReader ctx.RequestAborted

                        match lineOption with
                        | None ->
                            shouldContinue <- false
                            log "Client closed connection or empty line received"
                        | Some line when String.IsNullOrWhiteSpace(line) ->
                            // Skip empty lines
                            ()
                        | Some line ->
                            log $"Received message: {line.Substring(0, min 100 line.Length)}..."

                            try
                                // Parse JSON-RPC request
                                let jsonRpcRequest = JsonSerializer.Deserialize<JsonRpcRequest>(line, jsonOptions)

                                // Process request
                                let! result = server.HandleRequest(jsonRpcRequest) |> Async.StartAsTask

                                // Create response
                                let response = server.CreateResponse(jsonRpcRequest.Id, result)
                                let responseJson = JsonSerializer.Serialize(response, jsonOptions)

                                log $"Sending response: {responseJson.Substring(0, min 500 responseJson.Length)}..."

                                // Write response as a single line
                                do! writeLineAsync responseWriter responseJson

                            with
                            | :? JsonException as ex ->
                                log $"JSON parsing error: {ex.Message}"
                                // Send error response
                                let errorResponse = {|
                                    jsonrpc = "2.0"
                                    error = {|
                                        code = -32700
                                        message = "Parse error"
                                        data = ex.Message
                                    |}
                                    id = null
                                |}
                                let errorJson = JsonSerializer.Serialize(errorResponse, jsonOptions)
                                do! writeLineAsync responseWriter errorJson
                            | ex ->
                                log $"Error processing message: {ex.Message}"
                                // Send internal error response
                                let errorResponse = {|
                                    jsonrpc = "2.0"
                                    error = {|
                                        code = -32603
                                        message = "Internal error"
                                        data = ex.Message
                                    |}
                                    id = null
                                |}
                                let errorJson = JsonSerializer.Serialize(errorResponse, jsonOptions)
                                do! writeLineAsync responseWriter errorJson

                    with
                    | :? OperationCanceledException ->
                        shouldContinue <- false
                        log "Connection cancelled by client"
                    | ex ->
                        shouldContinue <- false
                        log $"Stream error: {ex.Message}"

                log "HTTP streaming connection closed"
        }