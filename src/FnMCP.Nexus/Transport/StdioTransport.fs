module FnMCP.Nexus.Transport.StdioTransport

open System
open System.IO
open System.Text
open System.Text.Json
open System.Text.Json.Serialization
open FnMCP.Nexus.Types
open FnMCP.Nexus.McpServer

// JSON-RPC over stdio transport (LSP-style with Content-Length framing or NDJSON)
// This is extracted from the original Program.fs to maintain backward compatibility

// JSON serialization options
let jsonOptions = JsonSerializerOptions()
jsonOptions.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
jsonOptions.WriteIndented <- false
jsonOptions.DefaultIgnoreCondition <- JsonIgnoreCondition.WhenWritingNull

// Log to stderr (stdout is for JSON-RPC communication)
let log message =
    Console.Error.WriteLine($"[StdioTransport] {message}")

// Process a single JSON-RPC request
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
            while not doneHeaders do
                let idx = line.IndexOf(':')
                if idx > 0 then
                    let key = line.Substring(0, idx).Trim()
                    let value = line.Substring(idx + 1).Trim()
                    if key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase) then
                        let ok, v = Int32.TryParse(value)
                        if ok then contentLengthOpt <- Some v
                let! l = sr.ReadLineAsync() |> Async.AwaitTask
                if isNull l || l.Length = 0 then doneHeaders <- true else line <- l
            match contentLengthOpt with
            | Some len when len >= 0 ->
                let buffer = Array.zeroCreate<char> len
                let mutable offset = 0
                while offset < len do
                    let! read = sr.ReadAsync(buffer, offset, len - offset) |> Async.AwaitTask
                    if read = 0 then offset <- len else offset <- offset + read
                let payload = new string(buffer, 0, len)
                return Some payload
            | _ ->
                // No usable Content-Length; fall back to NDJSON accumulation
                let sb = StringBuilder()
                let mutable depth = 0
                let mutable inStr = false
                let mutable esc = false
                let mutable started = false
                let rec readMore () = async {
                    let! l = sr.ReadLineAsync() |> Async.AwaitTask
                    if isNull l then return () else
                    if not started then
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
            let mutable doneReading = depth = 0 && not inStr
            while not doneReading do
                let! l = sr.ReadLineAsync() |> Async.AwaitTask
                if isNull l then
                    doneReading <- true
                else
                    addLine l
                    doneReading <- (depth = 0 && not inStr)
            return Some (sb.ToString())
}

// Run stdio transport
let runStdioTransport (server: McpServer) : int =
    log "Starting stdio transport..."

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
    log "Stdio transport shutting down."
    0
