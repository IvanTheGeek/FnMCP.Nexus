module FnMCP.Nexus.EmbeddingService

open System
open System.Net.Http
open System.Text
open System.Text.Json

type EmbedRequest = { text: string }
type EmbedResponse = {
    embedding: float list
    dimension: int
}

/// Get embedding API URL from environment variable or use default
/// Environment variables are loaded from .env file at application startup
let private getEmbeddingApiUrl () =
    Environment.GetEnvironmentVariable("EMBEDDING_API_URL")
    |> Option.ofObj
    |> Option.defaultValue "http://66.179.208.238:5000/embed"

let private httpClient = new HttpClient()

let embed (text: string) : Async<float list> =
    async {
        try
            let apiUrl = getEmbeddingApiUrl()
            let request = { text = text }
            let jsonContent = JsonSerializer.Serialize(request)
            let content = new StringContent(jsonContent, Encoding.UTF8, "application/json")

            let! response = httpClient.PostAsync(apiUrl, content) |> Async.AwaitTask
            let! responseBody = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            
            if response.IsSuccessStatusCode then
                let embedResponse = JsonSerializer.Deserialize<EmbedResponse>(responseBody)
                return embedResponse.embedding
            else
                printfn "Embedding API error: %s" responseBody
                return List.replicate 384 0.0 // Fallback to zero vector
        with ex ->
            printfn "Embedding service exception: %s" ex.Message
            return List.replicate 384 0.0 // Fallback to zero vector
    }
/// Future implementation note:
///
/// When the embedding API is ready on the VPS, this function should:
/// 1. Make HTTP POST request to VPS embedding endpoint (e.g., http://66.179.208.238:8000/embed)
/// 2. Send JSON body: { "text": "query string" }
/// 3. Parse response: { "embedding": [0.1, 0.2, ...] }
/// 4. Return the 384-dimensional vector
///
/// Example implementation (when ready):
///
/// open System.Net.Http
/// open System.Text
/// open System.Text.Json
///
/// let embedReal (text: string) : Async<float list> =
///     async {
///         let httpClient = new HttpClient()
///         let embeddingUrl = "http://66.179.208.238:8000/embed"
///
///         let requestBody = {| text = text |}
///         let jsonBody = JsonSerializer.Serialize(requestBody)
///         use content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
///
///         let! response = httpClient.PostAsync(embeddingUrl, content) |> Async.AwaitTask
///         let! responseJson = response.Content.ReadAsStringAsync() |> Async.AwaitTask
///
///         let doc = JsonDocument.Parse(responseJson)
///         let embeddingArray = doc.RootElement.GetProperty("embedding")
///         let embedding =
///             embeddingArray.EnumerateArray()
///             |> Seq.map (fun e -> e.GetDouble())
///             |> Seq.toList
///
///         return embedding
///     }
