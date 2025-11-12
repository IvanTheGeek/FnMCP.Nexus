module FnMCP.Nexus.FileSystemProvider

open System.IO
open FnMCP.Nexus.Types
open FnMCP.Nexus.ContentProvider

// File system provider that reads markdown files on-demand (no caching)
type FileSystemProvider(rootPath: string) =

    interface IContentProvider with

        member _.ListResources() = async {
            let files = 
                Directory.GetFiles(rootPath, "*.md", SearchOption.AllDirectories)
                |> Array.map (fun path ->
                    let relativePath = Path.GetRelativePath(rootPath, path)
                    let uri = $"context://{relativePath.Replace(Path.DirectorySeparatorChar, '/')}"
                    let name = Path.GetFileNameWithoutExtension(path)
                    {
                        Uri = uri
                        Name = name
                        Description = Some $"Documentation: {name}"
                        MimeType = Some "text/markdown"
                        Text = None  // Don't load content in list
                    }
                )
                |> Array.toList
            return files
        }

        member _.GetResource(uri: string) = async {
            try
                // Parse URI: context://framework/overview.md -> framework/overview.md
                let relativePath = uri.Replace("context://", "").Replace('/', Path.DirectorySeparatorChar)
                let fullPath = Path.Combine(rootPath, relativePath)

                if not (File.Exists(fullPath)) then
                    return Error $"Resource not found: {uri}"
                else
                    // Read fresh from disk every time (no caching)
                    let! content = File.ReadAllTextAsync(fullPath) |> Async.AwaitTask

                    let resource = {
                        Uri = uri
                        Name = Path.GetFileNameWithoutExtension(fullPath)
                        Description = Some $"Documentation from {relativePath}"
                        MimeType = Some "text/markdown"
                        Text = Some content
                    }

                    return Ok resource
            with
            | ex -> return Error $"Error reading resource: {ex.Message}"
        }

