namespace FnMCP.Nexus.Domain

open System

// Event type definitions for event-sourced Nexus
// Following F# conventions: discriminated unions and records

type EventType =
    | TechnicalDecision
    | DesignNote
    | ResearchFinding
    | FrameworkInsight
    | MethodologyInsight
    | NexusInsight
    | SessionState
    | CrossProjectIdea

with
    member this.AsString =
        match this with
        | TechnicalDecision -> "TechnicalDecision"
        | DesignNote -> "DesignNote"
        | ResearchFinding -> "ResearchFinding"
        | FrameworkInsight -> "FrameworkInsight"
        | MethodologyInsight -> "MethodologyInsight"
        | NexusInsight -> "NexusInsight"
        | SessionState -> "SessionState"
        | CrossProjectIdea -> "CrossProjectIdea"

    static member Parse(str: string) =
        let s = if isNull str then "" else str
        match s.Trim().ToLowerInvariant() with
        | "technicaldecision" | "technical_decision" | "decision" -> TechnicalDecision
        | "designnote" | "design_note" | "note" -> DesignNote
        | "researchfinding" | "research_finding" | "finding" -> ResearchFinding
        | "frameworkinsight" | "framework_insight" | "insight" -> FrameworkInsight
        | "methodologyinsight" | "methodology_insight" -> MethodologyInsight
        | "nexusinsight" | "nexus_insight" -> NexusInsight
        | "sessionstate" | "session_state" -> SessionState
        | "crossprojectidea" | "cross_project_idea" -> CrossProjectIdea
        | other -> failwith ($"Unknown event type: {other}")

type TechnicalDecisionDetails = {
    Status: string option // e.g., proposed | decided | superseded
    Decision: string option
    Context: string option
    Consequences: string option
}

type Priority =
    | Consider
    | Important
    | Low
with
    member this.AsString =
        match this with
        | Consider -> "Consider"
        | Important -> "Important"
        | Low -> "Low"

    static member Parse(str: string) =
        match str.Trim().ToLowerInvariant() with
        | "consider" -> Consider
        | "important" -> Important
        | "low" -> Low
        | other -> failwith ($"Unknown priority: {other}")

type IdeaStatus =
    | Pending
    | Exploring
    | Implemented
    | Rejected
with
    member this.AsString =
        match this with
        | Pending -> "Pending"
        | Exploring -> "Exploring"
        | Implemented -> "Implemented"
        | Rejected -> "Rejected"

    static member Parse(str: string) =
        match str.Trim().ToLowerInvariant() with
        | "pending" -> Pending
        | "exploring" -> Exploring
        | "implemented" -> Implemented
        | "rejected" -> Rejected
        | other -> failwith ($"Unknown idea status: {other}")

type CrossProjectIdeaDetails = {
    SourceProject: string
    TargetProject: string
    Idea: string
    Priority: Priority
    Status: IdeaStatus
    ContextLink: string option
}

type EventMeta = {
    Id: Guid
    Type: EventType
    Title: string
    Summary: string option
    OccurredAt: DateTime
    Tags: string list
    Author: string option
    Links: string list
    Technical: TechnicalDecisionDetails option
    CrossProjectIdea: CrossProjectIdeaDetails option
}

type TimelineItem = {
    Path: string
    Title: string
    Type: string
    OccurredAt: DateTime
}

// ============================================================================
// PHASE 2: System Events - Track operations, not narratives
// ============================================================================

// Projection types that can be regenerated
type ProjectionType =
    | Timeline
    | Metrics
    | Knowledge        // Phase 3: F# learning knowledge base
    | Documentation
    | BlogDrafts
    | ForumContent
with
    member this.AsString =
        match this with
        | Timeline -> "Timeline"
        | Metrics -> "Metrics"
        | Knowledge -> "Knowledge"
        | Documentation -> "Documentation"
        | BlogDrafts -> "BlogDrafts"
        | ForumContent -> "ForumContent"

    static member Parse(str: string) =
        match str.Trim() with
        | "Timeline" -> Timeline
        | "Metrics" -> Metrics
        | "Knowledge" -> Knowledge
        | "Documentation" -> Documentation
        | "BlogDrafts" -> BlogDrafts
        | "ForumContent" -> ForumContent
        | other -> failwith ($"Unknown projection type: {other}")

// Staleness indicator for projections
type Staleness =
    | Fresh                      // Up to date
    | Stale of eventCount: int   // N events since last regeneration
with
    member this.AsString =
        match this with
        | Fresh -> "Fresh"
        | Stale n -> $"Stale({n})"

// System event types - operations, not stories
type SystemEventType =
    | EventCreated                // Domain event was created
    | ProjectionRegenerated       // Projection was rebuilt
    | ProjectionQueried           // Projection was accessed
    | ToolInvoked                 // MCP tool was called
with
    member this.AsString =
        match this with
        | EventCreated -> "EventCreated"
        | ProjectionRegenerated -> "ProjectionRegenerated"
        | ProjectionQueried -> "ProjectionQueried"
        | ToolInvoked -> "ToolInvoked"

    static member Parse(str: string) =
        match str.Trim() with
        | "EventCreated" -> EventCreated
        | "ProjectionRegenerated" -> ProjectionRegenerated
        | "ProjectionQueried" -> ProjectionQueried
        | "ToolInvoked" -> ToolInvoked
        | other -> failwith ($"Unknown system event type: {other}")

// System event metadata - YAML only, no markdown body
type SystemEventMeta = {
    Id: Guid
    Type: SystemEventType
    OccurredAt: DateTime

    // EventCreated fields
    EventId: Guid option
    EventType: string option

    // ProjectionRegenerated fields
    ProjectionType: ProjectionType option
    Duration: TimeSpan option
    EventCount: int option

    // ProjectionQueried fields
    Staleness: Staleness option

    // ToolInvoked fields
    ToolName: string option
    Success: bool option
}

// ============================================================================
// PHASE 3: Learning Events - Event-sourced F# knowledge
// ============================================================================

// Pattern categories for organizing F# knowledge
type PatternCategory =
    | Syntax           // String interpolation, operators, etc.
    | Types            // Type inference, annotations, records, DUs
    | Modules          // Module organization, namespaces
    | FunctionComposition
    | Architecture     // Overall code organization
    | ErrorHandling
    | Async
    | Collections
with
    member this.AsString =
        match this with
        | Syntax -> "Syntax"
        | Types -> "Types"
        | Modules -> "Modules"
        | FunctionComposition -> "FunctionComposition"
        | Architecture -> "Architecture"
        | ErrorHandling -> "ErrorHandling"
        | Async -> "Async"
        | Collections -> "Collections"

    static member Parse(str: string) =
        match str.Trim() with
        | "Syntax" -> Syntax
        | "Types" -> Types
        | "Modules" -> Modules
        | "FunctionComposition" -> FunctionComposition
        | "Architecture" -> Architecture
        | "ErrorHandling" -> ErrorHandling
        | "Async" -> Async
        | "Collections" -> Collections
        | other -> failwith ($"Unknown pattern category: {other}")

// Learning event types - tracking F# coding knowledge
type LearningEventType =
    | ErrorEncountered       // Hit a compiler/runtime error
    | SolutionApplied        // Fixed an error successfully
    | PatternDiscovered      // Found a useful coding pattern
    | PatternValidated       // Pattern worked again (confidence++)
    | PatternDeprecated      // Pattern no longer recommended
    | LessonLearned          // General insight/principle
    | CodeReviewFeedback     // User corrected my code
    | CompilationSuccess     // Code compiled first try
    | RefactoringApplied     // Successful code improvement
with
    member this.AsString =
        match this with
        | ErrorEncountered -> "ErrorEncountered"
        | SolutionApplied -> "SolutionApplied"
        | PatternDiscovered -> "PatternDiscovered"
        | PatternValidated -> "PatternValidated"
        | PatternDeprecated -> "PatternDeprecated"
        | LessonLearned -> "LessonLearned"
        | CodeReviewFeedback -> "CodeReviewFeedback"
        | CompilationSuccess -> "CompilationSuccess"
        | RefactoringApplied -> "RefactoringApplied"

    static member Parse(str: string) =
        match str.Trim() with
        | "ErrorEncountered" -> ErrorEncountered
        | "SolutionApplied" -> SolutionApplied
        | "PatternDiscovered" -> PatternDiscovered
        | "PatternValidated" -> PatternValidated
        | "PatternDeprecated" -> PatternDeprecated
        | "LessonLearned" -> LessonLearned
        | "CodeReviewFeedback" -> CodeReviewFeedback
        | "CompilationSuccess" -> CompilationSuccess
        | "RefactoringApplied" -> RefactoringApplied
        | other -> failwith ($"Unknown learning event type: {other}")

// Learning event metadata - Markdown body with code examples
type LearningEventMeta = {
    Id: Guid
    Type: LearningEventType
    Title: string
    Summary: string option
    OccurredAt: DateTime
    Tags: string list

    // Error-specific fields
    ErrorCode: string option          // "FS3373", "FS0039", etc.
    ErrorMessage: string option

    // Pattern-specific fields
    PatternName: string option        // "interpolated-string-extraction"
    PatternCategory: PatternCategory option
    UseCount: int option              // How many times used
    SuccessRate: float option         // 0.0 - 1.0

    // Context
    FilePath: string option           // Where it happened
    ConversationContext: string option // What we were building
    RelatedPatterns: string list      // Links to other patterns
}
