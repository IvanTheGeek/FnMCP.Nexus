# Event Modeling Approach

**Framework:** FnMCP.IvanTheGeekDevFramework  
**Methodology:** Event Modeling with Paths  
**Updated:** 2025-01-15  
**Status:** Core Framework Pattern

## Overview

Event Modeling is a visual methodology for designing software systems that shows the flow of data through time. Our framework extends traditional Event Modeling with multi-layer perspectives, perspective filtering, and the concept of "paths" - concrete execution traces that serve as specifications, tests, and documentation.

## Core Event Modeling Concepts

### The Three Elements

**Commands (Orange)** - User intentions
- Represented as orange sticky notes or rectangles
- Named as imperative verbs: "Add Entry", "Complete Session"
- Include the actor (user, system, time-based trigger)
- Show required data/parameters

**Events (Blue)** - Things that happened
- Represented as blue cards or rectangles
- Named in past tense: "Entry Added", "Session Completed"
- Immutable facts stored in the event store
- Include all relevant data about what occurred

**Views/Read Models (Green)** - Displayed information
- Represented as green areas or screenshots
- Show what users see on screens
- Built from projecting events
- Optimized for specific UI needs

### Visual Layout

```
Time flows left to right →

Swimlanes (vertical):
┌─────────────────────────────────────┐
│ UI Layer                            │
├─────────────────────────────────────┤
│ Command Handlers                    │
├─────────────────────────────────────┤
│ Event Store                         │
├─────────────────────────────────────┤
│ Read Model Projections              │
├─────────────────────────────────────┤
│ External Systems                    │
└─────────────────────────────────────┘
```

## Multi-Layer Event Models

### The Three Layers

**Layer 1: The Story** - Business narrative
- Human-readable story of what happens
- No technical details
- Focuses on business value
- Used for stakeholder communication

**Layer 2: The Components** - System design
- Shows system boundaries
- Identifies microservices/modules
- Maps to development teams
- Defines integration points

**Layer 3: The Implementation** - Technical details
- Specific technologies
- Database schemas
- API contracts
- Infrastructure requirements

### Layer Relationships
```
Story Layer       →  "What happens"
     ↓
Component Layer   →  "What handles it"
     ↓
Implementation    →  "How it's built"
```

## Perspective Filtering

Different stakeholders need different views of the same Event Model:

### Business Perspective
Shows only:
- Commands users can perform
- Business events
- User-facing screens
- Business rules/policies

Hides:
- Technical events
- System internals
- Infrastructure details

### Technical Perspective
Shows everything:
- All commands and events
- System boundaries
- Integration points
- Error handling
- Infrastructure events

### Team Perspective
Shows only:
- Components owned by the team
- Integration points with other teams
- Events the team produces/consumes
- Shared data contracts

## The Concept of Paths

### What Is a Path?

A **path** is one complete execution trace through the Event Model using concrete example data. Inspired by Choose Your Own Adventure books, each path represents one specific journey through all decision points.

### Path Characteristics

**Concrete, not abstract:**
```yaml
# Good - Concrete path with real data
path: driver_adds_laundry_entry
actor: John Smith (Driver #4823)
location: Love's Travel Stop, Amarillo TX
machine: Washer #3
quantity: 2 loads
price: $3.50 per load
payment: Quarters

# Bad - Abstract path with placeholders
path: user_adds_entry
actor: [USER]
location: [LOCATION]
data: [DATA]
```

**Complete execution trace:**
- Starts from initial trigger
- Follows through all steps
- Includes all decision outcomes
- Ends at final state

### Types of Paths

**Happy Path** - Everything works perfectly
- Most common successful scenario
- No errors or edge cases
- Demonstrates core value

**Error Paths** - Things go wrong
- Network failures
- Invalid data
- Missing requirements
- System unavailable

**Edge Paths** - Unusual but valid
- Maximum quantities
- Minimum values
- Rare combinations
- Special cases

**Alternative Paths** - Different valid routes
- Different payment methods
- Various user types
- Multiple valid outcomes

### Path as Multi-Purpose Tool

Each path serves as:
1. **Narrative** - Tells a story
2. **Test Case** - Defines expected behavior
3. **Documentation** - Shows how system works
4. **Prototype** - Visualizes user journey
5. **Validation** - Ensures Event Model completeness

## Given-When-Then (GWT)

### Structure

**Given** - Initial state/context
```
Given John is at Love's Travel Stop
And John has selected Washer
And the default price is $3.50
```

**When** - Action/trigger
```
When John taps the Add Entry button
```

**Then** - Expected outcome
```
Then an EntryAdded event is created
And the session total increases by $7.00
And the entry appears in the list
```

### GWT to Event Model Mapping

```
Given → Read Models (current state)
When  → Commands (user action)
Then  → Events + Updated Read Models
```

### GWT Organization

Store GWT scenarios in hierarchical structure:
```
LaundryLog/
├── Happy Paths/
│   ├── Add single washer entry
│   ├── Add multiple dryer entries
│   └── Complete session with receipt
├── Error Handling/
│   ├── GPS location unavailable
│   ├── Network connection lost
│   └── Invalid quantity entered
└── Edge Cases/
    ├── Maximum quantity (99)
    ├── Zero price entry
    └── Session spanning midnight
```

## EMapp Vision

### Interactive Event Model Application

**EMapp** will be an interactive tool for:
- Creating Event Models visually
- Navigating paths like Choose Your Own Adventure
- Filtering by perspective
- Generating tests from paths
- Validating model completeness

### Choose Your Own Adventure Navigation

```
Current State: Session Started
You are at the laundry machines.

What do you want to do?
1. Add a washer entry → [Continue to Washer Entry]
2. Add a dryer entry → [Continue to Dryer Entry]
3. Complete session → [Continue to Session Summary]
4. Cancel session → [Continue to Cancellation]

[Shows relevant Event Model slice for chosen path]
```

### Perspective Toggle

UI control to switch perspectives:
```
View as: [Business] [Developer] [QA] [Operations]

Business View shows:
- User commands
- Business events
- Screen mockups

Developer View adds:
- Technical events
- System boundaries
- API calls
```

## Implementation in F#

### Event Types
```fsharp
type Command =
    | StartSession of LocationData
    | AddEntry of LaundryEntry
    | CompleteSession of SessionId

type Event =
    | SessionStarted of SessionStartedData
    | EntryAdded of EntryAddedData
    | SessionCompleted of SessionCompletedData

type State = {
    Sessions: Map<SessionId, Session>
    Entries: Map<EntryId, Entry>
}
```

### Path Definition
```fsharp
type Path = {
    Name: string
    Description: string
    Actor: Actor
    Context: Context
    Steps: Step list
    Assertions: Assertion list
}

type Step =
    | UserAction of Command
    | SystemEvent of Event
    | StateChange of State -> State
    | ExternalCall of ServiceCall
```

## Best Practices

### Model Creation
1. Start with the Story Layer
2. Identify the main workflows
3. Add commands and events
4. Create read models for screens
5. Define system boundaries
6. Add technical details last

### Path Development
1. Begin with happy paths
2. Add error paths for each failure point
3. Include edge cases discovered in testing
4. Document alternative flows
5. Keep paths concrete and specific

### Team Collaboration
1. Use physical workshop for initial model
2. Transfer to digital tool (Miro/Figma)
3. Store paths in version control
4. Review paths in pull requests
5. Update model as system evolves

## Integration with Framework

### Penpot Integration
- Screens in Event Model link to Penpot designs
- Penpot prototypes follow path flows
- Component names match Event Model

### Forum Integration
- Paths discussed and refined in forum
- GWT scenarios stored and organized
- Community validates paths
- Edge cases discovered through discussion

### MCP Integration
- Event Models served as resources
- Paths available for test generation
- Perspectives filtered based on context
- Interactive navigation through EMapp

## Key Principles

1. **Model before coding** - Understand the flow first
2. **Concrete over abstract** - Use real examples
3. **Visual over textual** - Pictures beat documentation
4. **Collaborative design** - Include all stakeholders
5. **Living model** - Update as system evolves
6. **Test from model** - Generate tests from paths

---

*Event Modeling with paths provides a complete specification that's simultaneously human-readable and machine-executable. It bridges the gap between business understanding and technical implementation.*