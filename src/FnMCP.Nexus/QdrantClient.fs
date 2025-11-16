module FnMCP.Nexus.QdrantClient

open System
open System.Net.Http
open System.Text
open System.Text.Json
open System.Threading.Tasks

/// Configuration for Qdrant vector database
type QdrantConfig = {
    BaseUrl: string
    ApiKey: string
    CollectionName: string
}

/// Search result from Qdrant
type SearchResult = {
    Score: float
    ConversationTitle: string
    ConversationText: string
    ConversationUuid: string
    CreatedAt: string
}

/// HTTP client instance (reusable)
let private httpClient = new HttpClient()

/// Health check to verify Qdrant connection
let healthCheck (config: QdrantConfig) : bool =
    try
        use request = new HttpRequestMessage(HttpMethod.Get, $"{config.BaseUrl}/")
        if not (String.IsNullOrWhiteSpace(config.ApiKey)) then
            request.Headers.Add("api-key", config.ApiKey)

        let response = httpClient.SendAsync(request).Result
        response.IsSuccessStatusCode
    with
    | _ -> false

/// Search for similar vectors in Qdrant collection
let search (config: QdrantConfig) (vector: float list) (limit: int) : SearchResult list =
    try
        let searchUrl = $"{config.BaseUrl}/collections/{config.CollectionName}/points/search"

        // Build search request body
        let requestBody = {|
            vector = vector
            limit = limit
            with_payload = true
        |}

        let jsonBody = JsonSerializer.Serialize(requestBody)
        use content = new StringContent(jsonBody, Encoding.UTF8, "application/json")

        use request = new HttpRequestMessage(HttpMethod.Post, searchUrl)
        request.Content <- content

        if not (String.IsNullOrWhiteSpace(config.ApiKey)) then
            request.Headers.Add("api-key", config.ApiKey)

        let response = httpClient.SendAsync(request).Result

        if not response.IsSuccessStatusCode then
            eprintfn $"Qdrant search failed: {response.StatusCode}"
            []
        else
            let responseJson = response.Content.ReadAsStringAsync().Result
            let doc = JsonDocument.Parse(responseJson)

            // Parse response structure: { "result": [ { "score": 0.5, "payload": {...} } ] }
            let mutable resultProp = Unchecked.defaultof<JsonElement>
            match doc.RootElement.TryGetProperty("result", &resultProp) with
            | true when resultProp.ValueKind = JsonValueKind.Array ->
                resultProp.EnumerateArray()
                |> Seq.map (fun item ->
                    let mutable scoreProp = Unchecked.defaultof<JsonElement>
                    let score =
                        if item.TryGetProperty("score", &scoreProp) then
                            scoreProp.GetDouble()
                        else
                            0.0

                    let mutable payloadProp = Unchecked.defaultof<JsonElement>
                    let payload =
                        if item.TryGetProperty("payload", &payloadProp) then
                            Some payloadProp
                        else
                            None

                    match payload with
                    | Some p ->
                        let getStringProp (name: string) : string =
                            let mutable prop = Unchecked.defaultof<JsonElement>
                            if p.TryGetProperty(name, &prop) && prop.ValueKind = JsonValueKind.String then
                                prop.GetString()
                            else
                                ""

                        Some {
                            Score = score
                            ConversationTitle = getStringProp "conversation_title"
                            ConversationText = getStringProp "conversation_text"
                            ConversationUuid = getStringProp "conversation_uuid"
                            CreatedAt = getStringProp "created_at"
                        }
                    | None -> None
                )
                |> Seq.choose id
                |> Seq.toList
            | _ -> []
    with
    | ex ->
        eprintfn $"Qdrant search error: {ex.Message}"
        []

/// Get Qdrant configuration from environment variables or defaults
/// Environment variables are loaded from .env file at application startup
let getConfig () : QdrantConfig =
    {
        BaseUrl =
            Environment.GetEnvironmentVariable("QDRANT_URL")
            |> Option.ofObj
            |> Option.defaultValue "http://66.179.208.238:6333"
        ApiKey =
            Environment.GetEnvironmentVariable("QDRANT_API_KEY")
            |> Option.ofObj
            |> Option.defaultValue ""
        CollectionName =
            Environment.GetEnvironmentVariable("QDRANT_COLLECTION")
            |> Option.ofObj
            |> Option.defaultValue "conversations"
    }
