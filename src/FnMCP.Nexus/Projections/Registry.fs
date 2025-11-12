module FnMCP.Nexus.Projections.Registry

open System
open System.IO
open System.Text
open FnMCP.Nexus.Domain

// ============================================================================
// PHASE 2: Projection Registry - Centralized tracking of all projections
// ============================================================================

// Registry entry representing one projection
type RegistryEntry = {
    Name: string
    Path: string
    Type: ProjectionType
    LastRegenerated: DateTime
    Staleness: Staleness
}

// The full registry containing all projections
type Registry = {
    Projections: RegistryEntry list
}

module RegistryIO =
    open FnMCP.Nexus.Domain.Projections.FrontMatterParser

    let private registryPath (basePath: string) =
        Path.Combine(basePath, "nexus", "projections", ".registry", "registry.yaml")

    // Read the registry from disk
    let readRegistry (basePath: string) : Registry =
        let path = registryPath basePath
        if not (File.Exists(path)) then
            { Projections = [] }
        else
            try
                let content = File.ReadAllText(path)
                let lines = content.Replace("\r\n", "\n").Split('\n')

                // Simple YAML parser for registry format
                let mutable entries = []
                let mutable currentEntry : Map<string, string> option = None

                for line in lines do
                    let trimmed = line.Trim()
                    if trimmed = "projections:" then
                        ()  // Header line
                    elif trimmed.StartsWith("- name:") then
                        // Save previous entry if exists
                        match currentEntry with
                        | Some map ->
                            let name = map.TryFind("name") |> Option.defaultValue ""
                            let path = map.TryFind("path") |> Option.defaultValue ""
                            let typeStr = map.TryFind("type") |> Option.defaultValue ""
                            let regenStr = map.TryFind("last_regenerated") |> Option.defaultValue ""
                            let stalenessStr = map.TryFind("staleness") |> Option.defaultValue "Fresh"

                            let projType = try ProjectionType.Parse(typeStr) with _ -> Timeline
                            let regen = match DateTime.TryParse(regenStr) with | true, dt -> dt | _ -> DateTime.MinValue
                            let staleness = if stalenessStr = "Fresh" then Fresh else
                                            let trimmed = stalenessStr.Trim()
                                            if trimmed.StartsWith("Stale(") && trimmed.EndsWith(")") then
                                                let numStr = trimmed.Substring(6, trimmed.Length - 7)
                                                match Int32.TryParse(numStr) with
                                                | true, n -> Stale n
                                                | _ -> Fresh
                                            else Fresh

                            entries <- { Name = name; Path = path; Type = projType; LastRegenerated = regen; Staleness = staleness } :: entries
                        | None -> ()

                        // Start new entry
                        let nameValue = trimmed.Substring(7).Trim()  // After "- name:"
                        currentEntry <- Some (Map.empty.Add("name", nameValue))
                    elif trimmed.Contains(":") && currentEntry.IsSome then
                        let idx = trimmed.IndexOf(':')
                        let key = trimmed.Substring(0, idx).Trim()
                        let value = trimmed.Substring(idx + 1).Trim()
                        currentEntry <- currentEntry |> Option.map (fun map -> map.Add(key, value))

                // Don't forget the last entry
                match currentEntry with
                | Some map ->
                    let name = map.TryFind("name") |> Option.defaultValue ""
                    let path = map.TryFind("path") |> Option.defaultValue ""
                    let typeStr = map.TryFind("type") |> Option.defaultValue ""
                    let regenStr = map.TryFind("last_regenerated") |> Option.defaultValue ""
                    let stalenessStr = map.TryFind("staleness") |> Option.defaultValue "Fresh"

                    let projType = try ProjectionType.Parse(typeStr) with _ -> Timeline
                    let regen = match DateTime.TryParse(regenStr) with | true, dt -> dt | _ -> DateTime.MinValue
                    let staleness = if stalenessStr = "Fresh" then Fresh else
                                    let trimmed = stalenessStr.Trim()
                                    if trimmed.StartsWith("Stale(") && trimmed.EndsWith(")") then
                                        let numStr = trimmed.Substring(6, trimmed.Length - 7)
                                        match Int32.TryParse(numStr) with
                                        | true, n -> Stale n
                                        | _ -> Fresh
                                    else Fresh

                    entries <- { Name = name; Path = path; Type = projType; LastRegenerated = regen; Staleness = staleness } :: entries
                | None -> ()

                { Projections = List.rev entries }
            with _ ->
                { Projections = [] }

    // Write the registry to disk
    let writeRegistry (basePath: string) (registry: Registry) : unit =
        let path = registryPath basePath
        let dir = Path.GetDirectoryName(path)
        if not (Directory.Exists(dir)) then
            Directory.CreateDirectory(dir) |> ignore

        let sb = StringBuilder()
        sb.AppendLine("projections:") |> ignore

        for entry in registry.Projections do
            let timestamp = entry.LastRegenerated.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK")
            sb.AppendLine($"  - name: {entry.Name}") |> ignore
            sb.AppendLine($"    path: {entry.Path}") |> ignore
            sb.AppendLine($"    type: {entry.Type.AsString}") |> ignore
            sb.AppendLine($"    last_regenerated: {timestamp}") |> ignore
            sb.AppendLine($"    staleness: {entry.Staleness.AsString}") |> ignore

        File.WriteAllText(path, sb.ToString(), Text.Encoding.UTF8)

    // Update or add a projection entry in the registry
    let updateProjection (basePath: string) (entry: RegistryEntry) : unit =
        let registry = readRegistry basePath

        // Remove old entry with same name if exists, then add new one
        let filteredProjections = registry.Projections |> List.filter (fun e -> e.Name <> entry.Name)
        let updatedRegistry = { Projections = entry :: filteredProjections }

        writeRegistry basePath updatedRegistry

    // Get a specific projection from the registry
    let getProjection (basePath: string) (name: string) : RegistryEntry option =
        let registry = readRegistry basePath
        registry.Projections |> List.tryFind (fun e -> e.Name = name)

    // List all projections in the registry
    let listProjections (basePath: string) : RegistryEntry list =
        let registry = readRegistry basePath
        registry.Projections

    // Refresh staleness for a specific projection
    let refreshStaleness (basePath: string) (name: string) : unit =
        match getProjection basePath name with
        | Some entry ->
            let newStaleness = Projections.ProjectionMetadata.calculateStaleness basePath entry.Type entry.LastRegenerated
            let updatedEntry = { entry with Staleness = newStaleness }
            updateProjection basePath updatedEntry
        | None -> ()

    // Refresh staleness for all projections
    let refreshAllStaleness (basePath: string) : unit =
        let registry = readRegistry basePath
        for entry in registry.Projections do
            let newStaleness = Projections.ProjectionMetadata.calculateStaleness basePath entry.Type entry.LastRegenerated
            let updatedEntry = { entry with Staleness = newStaleness }
            updateProjection basePath updatedEntry
