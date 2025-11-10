// Quick test to verify JSON serialization ignores null values
#r "nuget: System.Text.Json"

open System.Text.Json
open System.Text.Json.Serialization

// Configure JSON options same as the server
let jsonOptions = JsonSerializerOptions()
jsonOptions.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
jsonOptions.WriteIndented <- true
jsonOptions.DefaultIgnoreCondition <- JsonIgnoreCondition.WhenWritingNull

// Test type matching JsonRpcResponse
type TestResponse = {
    Jsonrpc: string
    Id: int option
    Result: obj option
    Error: obj option
}

// Test 1: Success response (should only have 'result', no 'error')
let successResponse = {
    Jsonrpc = "2.0"
    Id = Some 1
    Result = Some (box {| message = "success" |})
    Error = None
}

printfn "Success Response:"
printfn "%s" (JsonSerializer.Serialize(successResponse, jsonOptions))
printfn ""

// Test 2: Error response (should only have 'error', no 'result')
let errorResponse = {
    Jsonrpc = "2.0"
    Id = Some 2
    Result = None
    Error = Some (box {| code = -32601; message = "Method not found" |})
}

printfn "Error Response:"
printfn "%s" (JsonSerializer.Serialize(errorResponse, jsonOptions))
printfn ""

// Expected output:
// Success Response should NOT contain "error" field
// Error Response should NOT contain "result" field

printfn "✓ If you see 'result' without 'error' in success, and 'error' without 'result' in error - the fix works!"
