module FnMCP.Nexus.McpServer

open System
open System.Reflection
open System.Text.Json
open FnMCP.Nexus.Types
open FnMCP.Nexus.ContentProvider
open FnMCP.Nexus
open FnMCP.Nexus.Prompts

// MCP Server implementation with JSON-RPC message handling
type McpServer(provider: IContentProvider, contextLibraryPath: string) =

    // Resolve server version from assembly metadata so we don't hardcode it in code
    let getServerVersion () =
        try
            let asm = Assembly.GetExecutingAssembly()
            let infoAttr = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            if not (isNull infoAttr) && not (System.String.IsNullOrWhiteSpace(infoAttr.InformationalVersion)) then
                infoAttr.InformationalVersion
            else
                let v = asm.GetName().Version
                if isNull v then "0.0.0" else v.ToString()
        with _ -> "0.0.0"

    member _.HandleInitialize(request: InitializeRequest) = async {
        return {
            ProtocolVersion = "2024-11-05"
            Capabilities = box {|
                resources = {| listChanged = false |}
                prompts = {| listChanged = false |}
            |}
            ServerInfo = box {|
                name = "FnMCP.Nexus"
                version = getServerVersion ()
            |}
        }
    }

    member _.HandleListResources() = async {
        let! resources = provider.ListResources()
        return { Resources = resources }
    }

    member _.HandleReadResource(uri: string) = async {
        let! result = provider.GetResource(uri)
        match result with
        | Ok resource ->
            return {
                Contents = [ box {| uri = resource.Uri; text = resource.Text; mimeType = resource.MimeType |} ]
            }
        | Error msg ->
            return failwith msg  // Will be caught and converted to JSON-RPC error
    }

    member _.HandleListTools() = async {
        let tools = ToolRegistry.getTools contextLibraryPath
        return { Tools = tools }
    }

    member _.HandleListPrompts() = async {
        let prompts = getPromptList()
        return { Prompts = prompts }
    }

    member _.HandleGetPrompt(name: string, args: Map<string, obj> option) = async {
        let response = handleGetPrompt contextLibraryPath name args
        return response
    }

    // Main request handler - routes to appropriate method
    member this.HandleRequest(jsonRpcRequest: JsonRpcRequest) = async {
        try
            match jsonRpcRequest.Method with
            | "initialize" ->
                // Parse initialize request from params
                let initRequest = {
                    ProtocolVersion = "2024-11-05"
                    Capabilities = box {||}
                    ClientInfo = box {||}
                }
                let! response = this.HandleInitialize(initRequest)
                return Ok (box response)

            | "resources/list" ->
                let! response = this.HandleListResources()
                return Ok (box response)

            | "resources/read" ->
                match jsonRpcRequest.Params with
                | Some parameters ->
                    let jsonElement = parameters :?> JsonElement
                    let uri = jsonElement.GetProperty("uri").GetString()
                    let! response = this.HandleReadResource(uri)
                    return Ok (box response)
                | None ->
                    return Error {
                        Code = ErrorCodes.InvalidParams
                        Message = "Missing uri parameter"
                        Data = None
                    }

            | "tools/list" ->
                let! response = this.HandleListTools()
                return Ok (box response)

            | "prompts/list" ->
                let! response = this.HandleListPrompts()
                return Ok (box response)

            | "prompts/get" ->
                match jsonRpcRequest.Params with
                | Some parameters ->
                    let jsonElement = parameters :?> JsonElement
                    let name = jsonElement.GetProperty("name").GetString()

                    // Extract arguments if present
                    let args =
                        if jsonElement.TryGetProperty("arguments") |> fst then
                            let argsElement = jsonElement.GetProperty("arguments")
                            let argMap =
                                argsElement.EnumerateObject()
                                |> Seq.map (fun prop -> (prop.Name, box (prop.Value.GetString())))
                                |> Map.ofSeq
                            Some argMap
                        else
                            None

                    let! response = this.HandleGetPrompt(name, args)
                    return Ok (box response)
                | None ->
                    return Error {
                        Code = ErrorCodes.InvalidParams
                        Message = "Missing prompt name parameter"
                        Data = None
                    }

            | "tools/call" ->
                match jsonRpcRequest.Params with
                | Some parameters ->
                    let jsonElement = parameters :?> JsonElement
                    let name = jsonElement.GetProperty("name").GetString()
                    let mutable argsProp = Unchecked.defaultof<JsonElement>
                    let arguments = 
                        if jsonElement.TryGetProperty("arguments", &argsProp) then
                            Some (box argsProp)
                        else
                            None
                    
                    match ToolRegistry.executeTool contextLibraryPath name arguments with
                    | Ok content ->
                        let response = { Content = content }
                        return Ok (box response)
                    | Error err ->
                        return Error {
                            Code = ErrorCodes.InternalError
                            Message = err
                            Data = None
                        }
                | None ->
                    return Error {
                        Code = ErrorCodes.InvalidParams
                        Message = "Missing tool call parameters"
                        Data = None
                    }

            | _ ->
                return Error {
                    Code = ErrorCodes.MethodNotFound
                    Message = $"Method not found: {jsonRpcRequest.Method}"
                    Data = None
                }
        with
        | ex ->
            return Error {
                Code = ErrorCodes.InternalError
                Message = $"Internal error: {ex.Message}"
                Data = Some (box ex.StackTrace)
            }
    }

    // Create JSON-RPC response
    member _.CreateResponse(id: obj option, result: Result<obj, JsonRpcError>) =
        match result with
        | Ok res ->
            {
                Jsonrpc = "2.0"
                Id = id
                Result = Some res
                Error = None
            }
        | Error err ->
            {
                Jsonrpc = "2.0"
                Id = id
                Result = None
                Error = Some (box err)
            }

