# Paths and Given-When-Then

**Framework:** FnMCP.IvanTheGeekDevFramework  
**Pattern:** Choose Your Own Adventure Testing  
**Updated:** 2025-01-15  
**Status:** Core Testing Pattern

## The Path Concept

### Inspiration: Choose Your Own Adventure

Remember those books where you made choices?
```
You arrive at the truck stop laundry room.
The washers are $3.50 per load.

Do you:
1. Pay with quarters → Turn to page 42
2. Pay with credit card → Turn to page 67
3. Check if you have enough quarters first → Turn to page 23
```

Each choice leads to a different **path** through the story. Our framework uses the same concept for specifying, testing, and documenting software behavior.

### Definition of a Path

A **path** is:
- One complete execution trace through the system
- Uses concrete example data (not abstract placeholders)
- Follows one route through all decision points
- Ends at a definite outcome
- Tells a complete story

### Concrete vs Abstract

❌ **Abstract (Bad):**
```yaml
path: user_adds_entry
actor: <USER>
location: <LOCATION>
amount: <AMOUNT>
result: <SUCCESS>
```

✅ **Concrete (Good):**
```yaml
path: truck_driver_does_laundry_at_loves
actor: John Smith (CDL #4823567)
location: Love's Travel Stop, Amarillo TX (GPS: 35.1234, -101.5678)
time: Tuesday 2:47 PM
machines:
  - Washer #3: 2 loads @ $3.50
  - Dryer #7: 2 loads @ $3.00
payment: Quarters (exact change)
total: $13.00
result: Session completed, receipt #2024-11-08-0247PM
```

## Types of Paths

### Happy Paths

The "everything works perfectly" scenarios:
```yaml
happy_path_1_single_washer:
  description: Driver washes one load with quarters
  actor: Experienced driver who knows the system
  preconditions:
    - Has exact change
    - GPS working
    - Network connected
  flow:
    - Opens app at laundry room
    - GPS captures location automatically
    - Taps washer button
    - Quantity defaults to 1
    - Price shows historical $3.50
    - Taps quarters payment
    - Taps complete
  outcome: Entry saved with all IRS required fields
```

### Error Paths

When things go wrong:
```yaml
error_path_no_gps:
  description: GPS unavailable in metal building
  actor: Driver inside truck stop
  preconditions:
    - GPS permission granted
    - Inside metal building blocking signal
  flow:
    - Opens app at laundry room
    - GPS timeout after 5 seconds
    - "GPS Unavailable" message shows
    - Manual location button appears
    - Driver types "Love's Amarillo"
    - Autocomplete suggests full address
    - Driver selects correct location
    - Continues with entry
  outcome: Entry saved with manual location
```

### Edge Paths

Unusual but valid scenarios:
```yaml
edge_path_max_loads:
  description: Driver washing everything (max loads)
  actor: Driver after 3 weeks on road
  preconditions:
    - Accumulated maximum laundry
  flow:
    - Adds washer entry
    - Taps + button repeatedly
    - Reaches 99 (maximum)
    - + button disables
    - Warning shows "Maximum 99 loads"
    - Continues with 99 loads
  outcome: Entry saved with maximum quantity
```

### Alternative Paths

Different valid routes to similar outcomes:
```yaml
alternative_path_credit_card:
  description: Same laundry, paid with card
  # ... similar flow but selects credit card payment
  
alternative_path_truck_stop_points:
  description: Same laundry, paid with loyalty points
  # ... similar flow but selects points payment
```

## Given-When-Then Format

### Structure

**Given** - The starting context
**When** - The trigger action  
**Then** - The expected outcome

### Simple Example
```gherkin
Given I am at Love's Travel Stop in Amarillo
And I have selected a washer
When I tap the Add Entry button
Then an entry is added to my session
And the session total increases by $3.50
```

### Complex Example
```gherkin
Given I have an active session with 3 entries
And my GPS is not available
And I have manually entered "Love's Amarillo" as location
And my last payment method was quarters
When I add a dryer entry for 2 loads at $3.00 each
Then the entry is added with manual location
And the payment method defaults to quarters
And the session total increases by $6.00
And the entry shows in the list with a location icon
```

## Path Execution

### Path as Test

Each path becomes an automated test:
```fsharp
[<Test>]
let ``Truck driver does laundry at Love's`` () =
    // Given
    let location = GPS.coordinate 35.1234 -101.5678
    let session = Session.start location
    
    // When
    let entry1 = Entry.create Washer 2 3.50 Quarters
    let entry2 = Entry.create Dryer 2 3.00 Quarters
    let updatedSession =
        session
        |> Session.addEntry entry1
        |> Session.addEntry entry2
        |> Session.complete
    
    // Then
    updatedSession.Total |> should equal 13.00
    updatedSession.Entries |> should haveCount 2
    updatedSession.Location |> should equal location
    updatedSession.Status |> should equal Completed
```

### Path as Documentation

Paths serve as user documentation:
```markdown
## How to Track Laundry Expenses

### Scenario: Paying with Quarters

1. Open LaundryLog when you reach the laundry room
2. The app will capture your location automatically
3. Tap the washer or dryer button
4. Adjust quantity with the + and - buttons
5. Verify the price (uses last price at this location)
6. Select quarters as payment
7. Tap "Add Entry"
8. Repeat for additional machines
9. Tap "Complete Session" when done

Your receipt is automatically generated with IRS-compliant information.
```

### Path as Prototype

Paths define Penpot prototypes:
```yaml
prototype_flow:
  - screen: SessionEntry_Add_Initial
    action: Tap washer button
    transition: Highlight washer, enable quantity
    
  - screen: SessionEntry_Add_Quantity
    action: Tap + button twice
    transition: Quantity shows 3, total updates to $10.50
    
  - screen: SessionEntry_Add_Payment
    action: Tap quarters button
    transition: Quarters selected, Add Entry enables
    
  - screen: SessionEntry_Add_Active
    action: Tap Add Entry
    transition: Entry appears in list, form resets
```

## Path Organization

### Hierarchical Structure
```
LaundryLog/
├── Happy Paths/
│   ├── Single washer with quarters
│   ├── Multiple machines mixed payment
│   ├── Quick session with defaults
│   └── Full session with all fields
│
├── Error Paths/
│   ├── GPS unavailable
│   ├── Network disconnected
│   ├── Invalid data entry
│   └── Session interrupted
│
├── Edge Cases/
│   ├── Maximum quantities
│   ├── Zero price entries
│   ├── Session spanning midnight
│   └── Duplicate prevention
│
└── Alternative Flows/
    ├── Different payment methods
    ├── Edit existing entries
    ├── Cancel and restart
    └── Export variations
```

### Path Metadata
```yaml
path:
  id: happy_path_single_washer_v1
  category: Happy Path
  priority: High (Core functionality)
  frequency: Very Common (70% of sessions)
  last_validated: 2024-11-08
  test_status: Passing
  documentation_status: Complete
  prototype_status: Implemented in Penpot
  related_paths:
    - error_path_no_gps (fallback)
    - alternative_path_credit_card (variation)
```

## Generating Tests from Paths

### Automated Test Generation
```fsharp
let generateTest (path: Path) =
    let testName = path.Id.Replace("-", "_")
    
    sprintf """
    [<Test>]
    let ``%s`` () =
        // Given
        %s
        
        // When
        %s
        
        // Then
        %s
    """ 
        path.Description
        (generateGiven path.Preconditions)
        (generateWhen path.Actions)
        (generateThen path.Expectations)
```

### Property-Based Testing from Paths
```fsharp
[<Property>]
let ``All happy paths complete successfully`` () =
    Prop.forAll (Arb.from happyPaths) (fun path ->
        let result = executePath path
        result.Status = Success
    )

[<Property>]
let ``All error paths handle gracefully`` () =
    Prop.forAll (Arb.from errorPaths) (fun path ->
        let result = executePath path
        result.Status <> Crash &&
        result.ErrorMessage <> null
    )
```

## Path Validation

### Completeness Check
```fsharp
let validatePathCompleteness (path: Path) =
    [
        path.Actor <> Generic, "Actor must be specific"
        path.Context.IsComplete, "Context must be fully specified"
        path.Actions.Length > 0, "Path must have actions"
        path.Outcome <> Undefined, "Outcome must be defined"
        path.Data |> List.forall isConcrete, "Data must be concrete"
    ]
    |> List.filter (fun (valid, _) -> not valid)
    |> List.map snd
```

### Path Coverage Analysis
```fsharp
let analyzeCoverage (paths: Path list) =
    let commands = paths |> List.collect (fun p -> p.Commands)
    let events = paths |> List.collect (fun p -> p.Events)
    let screens = paths |> List.collect (fun p -> p.Screens)
    
    {
        CommandCoverage = commands |> percentageOf allCommands
        EventCoverage = events |> percentageOf allEvents
        ScreenCoverage = screens |> percentageOf allScreens
        DecisionCoverage = calculateDecisionCoverage paths
    }
```

## Integration with Event Modeling

### Path Through Event Model
```
Path: happy_path_single_washer

[User] → StartSession command
       ↓
[System] → SessionStarted event
       ↓
[View] → SessionEntry_Add_Initial screen
       ↓
[User] → AddEntry command
       ↓
[System] → EntryAdded event
       ↓
[View] → SessionEntry_Add_Active screen
       ↓
[User] → CompleteSession command
       ↓
[System] → SessionCompleted event
       ↓
[View] → Session_Complete screen
```

### Validation Against Event Model
```fsharp
let validatePathAgainstModel (path: Path) (model: EventModel) =
    path.Steps
    |> List.pairwise
    |> List.forall (fun (step1, step2) ->
        model.IsValidTransition step1 step2
    )
```

## Best Practices

### Writing Good Paths

1. **Be specific** - Use real names, places, amounts
2. **Tell a story** - Make it readable and relatable
3. **Cover decisions** - Each path follows one route
4. **Include context** - Why is the user doing this?
5. **Define success** - What does "working" mean?

### Path Maintenance

1. **Version paths** - Track changes over time
2. **Validate regularly** - Run against current system
3. **Update with features** - New features need new paths
4. **Retire obsolete** - Remove paths for removed features
5. **Document changes** - Why did the path change?

### Team Collaboration

1. **Write paths together** - Developer + QA + Product
2. **Review in PRs** - Paths are code
3. **Discuss in forum** - Community finds edge cases
4. **Share between apps** - Common patterns emerge
5. **Learn from failures** - Failed paths become error paths

## Tooling Support

### Future EMapp Features

Interactive path exploration:
```
Current: You're at the laundry room
Choice: What kind of machine?
  > [Washer] - Continue to washer entry
  > [Dryer] - Continue to dryer entry
  > [Both] - Continue to multiple entries
  
[Selected: Washer]

Current: Washer selected
Choice: How many loads?
  > [1 load] - Common single load
  > [2-3 loads] - Typical amount
  > [Many loads] - Edge case testing
```

### Path Execution Engine
```fsharp
type PathExecutor = {
    Execute: Path -> ExecutionResult
    Record: Path -> TestRecording
    Replay: TestRecording -> ReplayResult
    Compare: ExecutionResult -> ExecutionResult -> Comparison
}
```

## Key Benefits

1. **Concrete specifications** - Real examples, not abstract rules
2. **Living documentation** - Paths explain the system
3. **Automated testing** - Paths become tests
4. **Shared understanding** - Everyone speaks paths
5. **Complete coverage** - All routes explored
6. **Prototype validation** - Designs follow paths

---

*Paths transform abstract requirements into concrete journeys through your application. By thinking in paths, we build software that handles real-world scenarios gracefully.*