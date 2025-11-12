module FnMCP.Nexus.Types

// Core MCP protocol types following the specification

type McpVersion = { 
    Protocol: string
    Implementation: string 
}

type Resource = {
    Uri: string
    Name: string
    Description: string option
    MimeType: string option
    Text: string option
}

type ResourceTemplate = {
    UriTemplate: string
    Name: string
    Description: string option
    MimeType: string option
}

type Tool = {
    Name: string
    Description: string option
    InputSchema: obj  // JSON Schema
}

type Prompt = {
    Name: string
    Description: string option
    Arguments: obj list option
}

// Request/Response types as per MCP spec

type InitializeRequest = {
    ProtocolVersion: string
    Capabilities: obj
    ClientInfo: obj
}

type InitializeResponse = {
    ProtocolVersion: string
    Capabilities: obj
    ServerInfo: obj
}

type ListResourcesRequest = { 
    Dummy: unit option  // Empty request
}

type ListResourcesResponse = { 
    Resources: Resource list 
}

type ReadResourceRequest = { 
    Uri: string 
}

type ReadResourceResponse = { 
    Contents: obj list 
}

type ListToolsRequest = { 
    Dummy: unit option  // Empty request
}

type ListToolsResponse = { 
    Tools: Tool list 
}

type CallToolRequest = {
    Name: string
    Arguments: obj option
}

type CallToolResponse = {
    Content: obj list
}

type ListPromptsRequest = { 
    Dummy: unit option  // Empty request
}

type ListPromptsResponse = { 
    Prompts: Prompt list 
}

type GetPromptRequest = { 
    Name: string
    Arguments: obj option
}

type GetPromptResponse = {
    Description: string option
    Messages: obj list
}

// JSON-RPC 2.0 message types

type JsonRpcRequest = {
    Jsonrpc: string
    Id: obj option
    Method: string
    Params: obj option
}

type JsonRpcResponse = {
    Jsonrpc: string
    Id: obj option
    Result: obj option
    Error: obj option
}

type JsonRpcError = {
    Code: int
    Message: string
    Data: obj option
}

// Error codes as per JSON-RPC 2.0 and MCP spec
module ErrorCodes =
    let ParseError = -32700
    let InvalidRequest = -32600
    let MethodNotFound = -32601
    let InvalidParams = -32602
    let InternalError = -32603

