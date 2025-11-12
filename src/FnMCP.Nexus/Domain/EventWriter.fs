module FnMCP.Nexus.Domain.EventWriter

open System
open System.IO
open System.Text
open FnMCP.Nexus.Domain

// Event file writing with YAML frontmatter and markdown body

module private Helpers =
    let sanitizeFilePart (s: string) =
        // Keep letters, numbers, hyphens, and underscores; convert spaces to underscores
        let cleaned = s.Trim().Replace(" ", "_")
        let sb = StringBuilder(cleaned.Length)
        for ch in cleaned do
            if Char.IsLetterOrDigit(ch) || ch = '-' || ch = '_' then sb.Append(ch) |> ignore
            elif ch = '.' then sb.Append('.') |> ignore // allow dot in case
            else ()
        if sb.Length = 0 then "untitled" else sb.ToString()

    let monthFolder (dt: DateTime) = dt.ToString("yyyy-MM")

    let fileTimestamp (dt: DateTime) =
        // Format: YYYY-MM-DDTHH-MM-SSZ (UTC with Z suffix)
        dt.ToUniversalTime().ToString("yyyy-MM-dd'T'HH-mm-ss") + "Z"

    let yamlEscape (s: string) =
        if isNull s then "" else s.Replace("\\", "\\\\").Replace("\"", "\\\"")

    let toYaml (meta: EventMeta) (body: string) =
        let b = StringBuilder()
        let append (s: string) = b.AppendLine(s) |> ignore
        append "---"
        append ($"id: {meta.Id}")
        append ($"type: {meta.Type.AsString}")
        append ($"title: \"{yamlEscape meta.Title}\"")
        meta.Summary |> Option.iter (fun s -> append ($"summary: \"{yamlEscape s}\""))
        // ISO 8601 for readability, include offset if available
        let occurred = meta.OccurredAt.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK")
        append ($"occurred_at: {occurred}")
        if meta.Tags |> List.isEmpty |> not then
            append "tags:"
            for t in meta.Tags do append ($"  - {t}")
        meta.Author |> Option.iter (fun a -> append ($"author: {a}"))
        if meta.Links |> List.isEmpty |> not then
            append "links:"
            for l in meta.Links do append ($"  - {l}")
        match meta.Type, meta.Technical with
        | TechnicalDecision, Some td ->
            append "technical_decision:"
            td.Status |> Option.iter (fun v -> append ($"  status: {v}"))
            td.Decision |> Option.iter (fun v -> append ($"  decision: \"{yamlEscape v}\""))
            td.Context |> Option.iter (fun v -> append ($"  context: \"{yamlEscape v}\""))
            td.Consequences |> Option.iter (fun v -> append ($"  consequences: \"{yamlEscape v}\""))
        | _ -> ()
        match meta.Type, meta.CrossProjectIdea with
        | CrossProjectIdea, Some cpi ->
            append "cross_project_idea:"
            append ($"  source_project: {cpi.SourceProject}")
            append ($"  target_project: {cpi.TargetProject}")
            append ($"  idea: \"{yamlEscape cpi.Idea}\"")
            append ($"  priority: {cpi.Priority.AsString}")
            append ($"  status: {cpi.Status.AsString}")
            cpi.ContextLink |> Option.iter (fun v -> append ($"  context_link: \"{yamlEscape v}\""))
        | _ -> ()
        append "---"
        append ""
        if String.IsNullOrWhiteSpace(body) then () else append body
        b.ToString()

open Helpers

let ensureDirectory (path: string) =
    if not (Directory.Exists(path)) then Directory.CreateDirectory(path) |> ignore

let eventDirectory (basePath: string) (project: string option) (dt: DateTime) =
    match project with
    | Some proj -> Path.Combine(basePath, "nexus", "events", proj, "domain", "active", monthFolder dt)
    | None -> Path.Combine(basePath, "nexus", "events", "domain", "active", monthFolder dt)

let buildFilename (etype: EventType) (title: string) (dt: DateTime) =
    let guid = System.Guid.NewGuid().ToString("N").Substring(0, 8) // 8-char GUID suffix
    let ts = fileTimestamp dt
    let et = etype.AsString
    let name = sanitizeFilePart title
    $"{ts}_{et}_{name}_{guid}.md"

let writeEventFile (basePath: string) (project: string option) (meta: EventMeta) (body: string) =
    let dir = eventDirectory basePath project meta.OccurredAt
    ensureDirectory dir
    let filename = buildFilename meta.Type meta.Title meta.OccurredAt
    let fullPath = Path.Combine(dir, filename)
    let yaml = toYaml meta body
    File.WriteAllText(fullPath, yaml, Encoding.UTF8)
    fullPath

// ============================================================================
// PHASE 2: System Event Writing - YAML only, no markdown body
// ============================================================================

module SystemEventHelpers =
    // System events go to different directory structure
    let systemEventDirectory (basePath: string) (project: string option) (dt: DateTime) =
        match project with
        | Some proj -> Path.Combine(basePath, "nexus", "events", proj, "system", "active", Helpers.monthFolder dt)
        | None -> Path.Combine(basePath, "nexus", "events", "system", "active", Helpers.monthFolder dt)

    // System event filename: timestamp_EventType_guid.yaml
    let buildSystemFilename (etype: SystemEventType) (dt: DateTime) =
        let guid = System.Guid.NewGuid().ToString("N").Substring(0, 8)
        let ts = Helpers.fileTimestamp dt
        let et = etype.AsString
        $"{ts}_{et}_{guid}.yaml"

    // Convert system event to pure YAML (no markdown body)
    let toSystemYaml (meta: SystemEventMeta) =
        let b = StringBuilder()
        let append (s: string) = b.AppendLine(s) |> ignore

        append "---"
        append ($"id: {meta.Id}")
        append ($"type: {meta.Type.AsString}")

        // ISO 8601 timestamp
        let occurred = meta.OccurredAt.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK")
        append ($"occurred_at: {occurred}")

        // Conditional fields based on event type
        match meta.Type with
        | EventCreated ->
            meta.EventId |> Option.iter (fun id -> append ($"event_id: {id}"))
            meta.EventType |> Option.iter (fun et -> append ($"event_type: {et}"))

        | ProjectionRegenerated ->
            meta.ProjectionType |> Option.iter (fun pt -> append ($"projection_type: {pt.AsString}"))
            meta.Duration |> Option.iter (fun d -> append ($"duration_ms: {d.TotalMilliseconds}"))
            meta.EventCount |> Option.iter (fun c -> append ($"event_count: {c}"))

        | ProjectionQueried ->
            meta.ProjectionType |> Option.iter (fun pt -> append ($"projection_type: {pt.AsString}"))
            meta.Staleness |> Option.iter (fun s -> append ($"staleness: {s.AsString}"))

        | ToolInvoked ->
            meta.ToolName |> Option.iter (fun tn -> append ($"tool_name: {tn}"))
            meta.Success |> Option.iter (fun s -> append ($"success: {s}"))

        b.ToString()

open SystemEventHelpers

// Write system event (YAML only, no body)
let writeSystemEvent (basePath: string) (project: string option) (meta: SystemEventMeta) : string =
    let dir = systemEventDirectory basePath project meta.OccurredAt
    ensureDirectory dir
    let filename = buildSystemFilename meta.Type meta.OccurredAt
    let fullPath = Path.Combine(dir, filename)
    let yaml = toSystemYaml meta
    File.WriteAllText(fullPath, yaml, Encoding.UTF8)
    fullPath

// ============================================================================
// PHASE 3: Learning Event Writing - YAML frontmatter + markdown body with code
// ============================================================================

module LearningEventHelpers =
    // Learning events go to learning directory
    let learningEventDirectory (basePath: string) (project: string option) (dt: DateTime) =
        match project with
        | Some proj -> Path.Combine(basePath, "nexus", "events", proj, "learning", "active", Helpers.monthFolder dt)
        | None -> Path.Combine(basePath, "nexus", "events", "learning", "active", Helpers.monthFolder dt)

    // Learning event filename: timestamp_EventType_PatternName_guid.md
    let buildLearningFilename (etype: LearningEventType) (patternName: string option) (dt: DateTime) =
        let guid = System.Guid.NewGuid().ToString("N").Substring(0, 8)
        let ts = Helpers.fileTimestamp dt
        let et = etype.AsString
        match patternName with
        | Some name ->
            let sanitized = Helpers.sanitizeFilePart name
            $"{ts}_{et}_{sanitized}_{guid}.md"
        | None ->
            $"{ts}_{et}_{guid}.md"

    // Convert learning event to YAML frontmatter
    let toLearningYaml (meta: LearningEventMeta) (body: string) =
        let b = StringBuilder()
        let append (s: string) = b.AppendLine(s) |> ignore

        append "---"
        append ($"id: {meta.Id}")
        append ($"type: {meta.Type.AsString}")
        append ($"title: \"{Helpers.yamlEscape meta.Title}\"")
        meta.Summary |> Option.iter (fun s -> append ($"summary: \"{Helpers.yamlEscape s}\""))

        let occurred = meta.OccurredAt.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK")
        append ($"occurred_at: {occurred}")

        if meta.Tags |> List.isEmpty |> not then
            append "tags:"
            for t in meta.Tags do append ($"  - {t}")

        // Error-specific fields
        meta.ErrorCode |> Option.iter (fun ec -> append ($"error_code: {ec}"))
        meta.ErrorMessage |> Option.iter (fun em -> append ($"error_message: \"{Helpers.yamlEscape em}\""))

        // Pattern-specific fields
        meta.PatternName |> Option.iter (fun pn -> append ($"pattern_name: {pn}"))
        meta.PatternCategory |> Option.iter (fun pc -> append ($"pattern_category: {pc.AsString}"))
        meta.UseCount |> Option.iter (fun uc -> append ($"use_count: {uc}"))
        meta.SuccessRate |> Option.iter (fun sr -> append ($"success_rate: {sr}"))

        // Context fields
        meta.FilePath |> Option.iter (fun fp -> append ($"file_path: \"{Helpers.yamlEscape fp}\""))
        meta.ConversationContext |> Option.iter (fun cc -> append ($"conversation_context: \"{Helpers.yamlEscape cc}\""))

        if meta.RelatedPatterns |> List.isEmpty |> not then
            append "related_patterns:"
            for rp in meta.RelatedPatterns do append ($"  - {rp}")

        append "---"
        append ""
        append body

        b.ToString()

open LearningEventHelpers

// Write learning event (YAML frontmatter + markdown body)
let writeLearningEvent (basePath: string) (project: string option) (meta: LearningEventMeta) (body: string) : string =
    let dir = learningEventDirectory basePath project meta.OccurredAt
    ensureDirectory dir
    let filename = buildLearningFilename meta.Type meta.PatternName meta.OccurredAt
    let fullPath = Path.Combine(dir, filename)
    let content = toLearningYaml meta body
    File.WriteAllText(fullPath, content, Encoding.UTF8)
    fullPath
