module FnMCP.Nexus.ContentProvider

open FnMCP.Nexus.Types

// Content provider abstraction for different content sources
type IContentProvider =
    abstract member GetResource: uri:string -> Async<Result<Resource, string>>
    abstract member ListResources: unit -> Async<Resource list>

