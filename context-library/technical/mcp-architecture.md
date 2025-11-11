# MCP Architecture

**Framework:** FnMCP.IvanTheGeekDevFramework  
**Protocol:** Model Context Protocol  
**Updated:** 2025-01-15

## MCP Server Implementation

### F# Server Structure
```fsharp
type McpServer() =
    interface IServer with
        member _.GetTools() = tools
        member _.GetResources() = resources
        member _.ExecuteTool(name, args) = 
            match name with
            | "update_documentation" -> updateDocs args
            | _ -> Error "Unknown tool"
```

### Read-on-Request Pattern
```fsharp
// No caching, always fresh
let getResource uri = async {
    let path = uriToPath uri
    let! content = File.ReadAllTextAsync(path) |> Async.AwaitTask
    return {
        Uri = uri
        MimeType = "text/markdown"
        Text = content
    }
}
```

### Tool Implementation
```fsharp
type UpdateDocumentationArgs = {
    Path: string
    Content: string
    Mode: WriteMode
}

let updateDocumentation args =
    let fullPath = Path.Combine(contextLibrary, args.Path)
    match args.Mode with
    | Overwrite -> File.WriteAllText(fullPath, args.Content)
    | Append -> File.AppendAllText(fullPath, args.Content)
```

### Resource Discovery
```fsharp
let discoverResources root =
    Directory.EnumerateFiles(root, "*.md", SearchOption.AllDirectories)
    |> Seq.map (fun path ->
        {
            Uri = pathToUri path
            Name = Path.GetFileNameWithoutExtension path
            Description = extractDescription path
            MimeType = "text/markdown"
        }
    )
    |> Seq.toList
```

## Deployment

### Binary Deployment
```fsharp
// Use AppContext for deployed binaries
let baseDirectory = AppContext.BaseDirectory
// NOT __SOURCE_DIRECTORY__ (build-time only)
```

### Configuration
```json
{
    "mcpServers": {
        "nexus": {
            "command": "dotnet",
            "args": ["/path/to/FnMCP.IvanTheGeek.dll"],
            "env": {
                "CONTEXT_LIBRARY": "/home/linux/context-library"
            }
        }
    }
}
```

### Security
```fsharp
let validatePath requestedPath =
    let full = Path.GetFullPath(requestedPath)
    if full.StartsWith(allowedRoot) then
        Ok full
    else
        Error "Path traversal attempt"
```

## Integration Patterns

### Two-Tier Knowledge
**Tier 1:** Quick Start (Project Knowledge)
- 2-3KB files
- Always loaded
- Essential context

**Tier 2:** Detailed (MCP Resources)
- 4-10KB files  
- On-demand loading
- Comprehensive docs

### Token Optimization
```
Before: 15,000 tokens loaded always
After:   2,000 tokens + on-demand
Savings: 87% reduction
```

### Usage Monitoring
```fsharp
let trackUsage (resource: Resource) =
    metrics.Increment "resource_accessed"
    metrics.Record "resource_size" resource.Text.Length
    logger.Info(sprintf "Resource accessed: %s" resource.Uri)
```

---

*MCP architecture enables efficient context management for LLM interactions.*