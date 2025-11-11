# Naming Conventions

## Penpot Screen Naming

**Pattern:** `Purpose_Action_State`

**Examples:**
- `SessionEntry_Add_Initial` - Adding first entry in a session
- `SessionEntry_Edit_Quantities` - Editing quantity values
- `SessionEntry_Review_Complete` - Review before submission
- `Settings_Location_Manual` - Manual location entry settings
- `Reports_Export_PDF` - PDF export screen for reports

**Purpose:** First segment identifies the feature area (SessionEntry, Settings, Reports).

**Action:** Second segment describes what user is doing (Add, Edit, Review, Export).

**State:** Third segment indicates the current condition (Initial, Complete, Manual, Error).

## Penpot Component Organization

**Boards:** Group related screens into functional areas.

**Components:** Name with forward slash hierarchy.
- `button / primary / large`
- `button / quantity / increment`
- `input / numeric / currency`
- `card / entry / complete`

**Variants:** Use Penpot variants for states.
- Default, Hover, Active, Disabled
- Valid, Invalid, Warning
- Empty, Partial, Complete

## F# Type Conventions

**Domain types:** Descriptive names using PascalCase.
```fsharp
type MachineType = Washer | Dryer
type PaymentMethod = Quarters | CreditCard | TruckStopPoints | Cash
type ValidationState = Valid | Missing of string list
```

**Record types:** Suffix with context when needed.
```fsharp
type LaundryEntry = { ... }
type LaundrySession = { ... }
type LocationData = { ... }
```

**Module organization:** Feature-based modules.
```fsharp
module LaundryLog.Domain
module LaundryLog.Validation  
module LaundryLog.GPS
```

## Event Model Visual Conventions

**Colors:**
- **Orange (#FFB366):** Commands - user intentions
- **Blue (#93C5FD):** Events - things that happened  
- **Green (#86EFAC):** Read models/Views - displayed data

**Shapes:**
- Commands: Rounded rectangles with user icon
- Events: Square/rectangular cards
- Views: Screenshots or mockup representations

**Swimlanes:** Organize by system component.
- UI, Domain, Infrastructure, External Services

## File and Folder Structure

**Context library paths:**
```
framework/event-modeling-approach.md
apps/laundrylog/design-v7-spec.md
technical/f-sharp-conventions.md
```

**Code organization:**
```
src/LaundryLog.Domain/Types.fs
src/LaundryLog.Web/Components/Entry.fs
src/LaundryLog.Infrastructure/GPS.fs
```

**Test naming:**
```
tests/LaundryLog.Domain.Tests/ValidationTests.fs
tests/LaundryLog.Web.Tests/EntryComponentTests.fs
```

## API and Message Naming

**Commands:** Imperative verb phrases.
```fsharp
type Command =
    | StartSession of LocationData
    | AddEntry of LaundryEntry
    | CompleteSession of SessionId
```

**Events:** Past tense descriptions.
```fsharp
type Event =
    | SessionStarted of SessionId * LocationData * DateTime
    | EntryAdded of SessionId * LaundryEntry
    | SessionCompleted of SessionId * Total * DateTime
```

**Validation messages:** Clear problem statements.
```fsharp
"Quantity must be greater than zero"
"Payment method required"
"Location required for IRS compliance"
```

## Git Conventions

**Branch names:**
- `feature/laundrylog-gps-integration`
- `fix/validation-state-display`
- `docs/event-model-update`

**Commit messages:**
- Start with verb: "Add", "Fix", "Update", "Remove"
- Reference component: "Add GPS location service"
- Keep under 50 characters

**PR titles:**
- Include app/module: "[LaundryLog] Add session management"
- Reference issue: "[PerDiem] Phase 1 design (#42)"

## Documentation Headers

**Markdown files:** Include metadata.
```markdown
# Title
**App:** LaundryLog
**Module:** GPS Integration  
**Updated:** 2025-01-15
**Status:** Implementation Ready
```

**Code comments:** Explain why, not what.
```fsharp
// IRS requires location for expense validation
// Using fallback to manual entry when GPS fails
```

## Consistency Rules

1. **Be descriptive:** `CalculateSessionTotal` not `CalcTot`
2. **Use domain language:** `LaundryEntry` not `Record`
3. **Avoid abbreviations:** `Quantity` not `Qty`
4. **Match Penpot to code:** Screen names map to component names
5. **Version iteratively:** `design-v7-spec.md` shows evolution