# LaundryLog Event Model

**App:** LaundryLog  
**Architecture:** Event Sourcing  
**Updated:** 2025-01-15  
**Status:** Implementation Ready

## Commands (User Intentions)

### Session Commands

```fsharp
type SessionCommand =
    | StartSession of StartSessionData
    | AddEntry of AddEntryData
    | RemoveEntry of RemoveEntryData  
    | UpdateEntry of UpdateEntryData
    | CompleteSession of CompleteSessionData
    | CancelSession of CancelSessionData

and StartSessionData = {
    Location: Location
    StartTime: DateTime
    DeviceInfo: DeviceInfo option
}

and AddEntryData = {
    SessionId: SessionId
    Machine: MachineType
    Quantity: Quantity
    Price: Price
    PaymentMethod: PaymentMethod
}
```

### Settings Commands

```fsharp
type SettingsCommand =
    | UpdateDefaultLocation of Location
    | SetPriceDefaults of PriceDefaults
    | EnableCommunitySharing of bool
    | ConfigureExport of ExportSettings
```

## Events (What Happened)

### Session Events

```fsharp
type SessionEvent =
    | SessionStarted of SessionStartedEvent
    | EntryAdded of EntryAddedEvent
    | EntryRemoved of EntryRemovedEvent
    | EntryUpdated of EntryUpdatedEvent
    | SessionCompleted of SessionCompletedEvent
    | SessionCancelled of SessionCancelledEvent
    | LocationUpdated of LocationUpdatedEvent

and SessionStartedEvent = {
    SessionId: SessionId
    Location: Location
    StartedAt: DateTime
    UserId: UserId option
    EventId: EventId
    Timestamp: DateTime
}

and EntryAddedEvent = {
    SessionId: SessionId
    EntryId: EntryId
    Machine: MachineType
    Quantity: Quantity
    Price: Price
    PaymentMethod: PaymentMethod
    Total: Money
    AddedAt: DateTime
    EventId: EventId
    Timestamp: DateTime
}
```

### System Events

```fsharp
type SystemEvent =
    | LocationCaptured of LocationCapturedEvent
    | LocationFailed of LocationFailedEvent
    | PriceSuggestionCalculated of PriceSuggestionEvent
    | CommunityDataUpdated of CommunityDataEvent
    | ExportGenerated of ExportGeneratedEvent
    | SyncCompleted of SyncCompletedEvent
```

## Read Models (Views)

### Current Session View

```fsharp
type CurrentSessionView = {
    SessionId: SessionId
    Location: Location
    StartedAt: DateTime
    Entries: EntryView list
    RunningTotal: Money
    Status: SessionStatus
    LastUpdated: DateTime
}

and EntryView = {
    EntryId: EntryId
    Machine: MachineType
    Quantity: Quantity  
    Price: Price
    PaymentMethod: PaymentMethod
    Total: Money
    Timestamp: DateTime
}

and SessionStatus =
    | InProgress
    | Completed
    | Cancelled
```

### Session History View

```fsharp
type SessionHistoryView = {
    UserId: UserId
    Sessions: SessionSummary list
    TotalSpent: Money
    TotalSessions: int
    LastSession: DateTime option
    MostCommonLocation: Location option
    AveragePerSession: Money
}

and SessionSummary = {
    SessionId: SessionId
    Location: Location
    Date: DateTime
    EntryCount: int
    Total: Money
    Status: SessionStatus
}
```

### Price Intelligence View

```fsharp
type PriceIntelligenceView = {
    Location: Location
    LastPrice: Price option
    HistoricalAverage: Price option
    CommunityAverage: Price option
    PriceHistory: PricePoint list
    Confidence: ConfidenceLevel
}

and PricePoint = {
    Price: Price
    Date: DateTime
    Source: PriceSource
}

and PriceSource =
    | UserHistory
    | CommunityData
    | SystemDefault
```

## Event Flow Diagrams

### Happy Path: Complete Session

```
User                System              Event Store       Read Model
 |                    |                      |                |
 |--StartSession----->|                      |                |
 |                    |--SessionStarted----->|                |
 |                    |                      |--Update------->|
 |<--Session Active---|                      |                |
 |                    |                      |                |
 |--AddEntry--------->|                      |                |
 |                    |--EntryAdded--------->|                |
 |                    |                      |--Update------->|
 |<--Entry Listed-----|                      |                |
 |                    |                      |                |
 |--CompleteSession-->|                      |                |
 |                    |--SessionCompleted--->|                |
 |                    |                      |--Update------->|
 |<--Receipt----------|                      |                |
```

### Error Path: GPS Failure

```
User                System              Location Service
 |                    |                      |
 |--StartSession----->|                      |
 |                    |--RequestLocation---->|
 |                    |                      |
 |                    |<--Timeout/Error-------|
 |<--Manual Entry-----|                      |
 |                    |                      |
 |--Enter Location--->|                      |
 |                    |--SessionStarted----->|
 |<--Session Active---|                      |
```

## Event Handlers

### Command to Event

```fsharp
let handleCommand (state: State) (command: Command) : Result<Event list, Error> =
    match command with
    | StartSession data ->
        match state.CurrentSession with
        | Some _ -> 
            Error "Session already in progress"
        | None ->
            let sessionId = SessionId.create()
            Ok [SessionStarted {
                SessionId = sessionId
                Location = data.Location
                StartedAt = data.StartTime
                UserId = state.UserId
                EventId = EventId.create()
                Timestamp = DateTime.UtcNow
            }]
    
    | AddEntry data ->
        match state.CurrentSession with
        | None -> 
            Error "No active session"
        | Some session when session.SessionId <> data.SessionId ->
            Error "Session ID mismatch"
        | Some _ ->
            let entryId = EntryId.create()
            let total = calculateTotal data.Quantity data.Price
            Ok [EntryAdded {
                SessionId = data.SessionId
                EntryId = entryId
                Machine = data.Machine
                Quantity = data.Quantity
                Price = data.Price
                PaymentMethod = data.PaymentMethod
                Total = total
                AddedAt = DateTime.UtcNow
                EventId = EventId.create()
                Timestamp = DateTime.UtcNow
            }]
```

### Event to State

```fsharp
let apply (state: State) (event: Event) : State =
    match event with
    | SessionStarted e ->
        { state with
            CurrentSession = Some {
                SessionId = e.SessionId
                Location = e.Location
                StartedAt = e.StartedAt
                Entries = []
                RunningTotal = Money.zero
                Status = InProgress
            }
        }
    
    | EntryAdded e ->
        match state.CurrentSession with
        | Some session when session.SessionId = e.SessionId ->
            let newEntry = {
                EntryId = e.EntryId
                Machine = e.Machine
                Quantity = e.Quantity
                Price = e.Price
                PaymentMethod = e.PaymentMethod
                Total = e.Total
                Timestamp = e.AddedAt
            }
            { state with
                CurrentSession = Some {
                    session with
                        Entries = newEntry :: session.Entries
                        RunningTotal = session.RunningTotal + e.Total
                }
            }
        | _ -> state  // Ignore if no session
    
    | SessionCompleted e ->
        match state.CurrentSession with
        | Some session when session.SessionId = e.SessionId ->
            { state with
                CurrentSession = None
                CompletedSessions = 
                    { session with Status = Completed } :: state.CompletedSessions
            }
        | _ -> state
```

## Projections

### Building Read Models

```fsharp
let projectToSessionView (events: Event list) : CurrentSessionView option =
    events
    |> List.fold apply State.initial
    |> (fun state -> state.CurrentSession)

let projectToHistory (events: Event list) : SessionHistoryView =
    let state = events |> List.fold apply State.initial
    {
        UserId = state.UserId
        Sessions = state.CompletedSessions |> List.map toSummary
        TotalSpent = state.CompletedSessions |> List.sumBy (fun s -> s.Total)
        TotalSessions = List.length state.CompletedSessions
        LastSession = state.CompletedSessions |> List.tryHead |> Option.map (fun s -> s.Date)
        MostCommonLocation = findMostCommon state.CompletedSessions
        AveragePerSession = calculateAverage state.CompletedSessions
    }
```

### Real-time Updates

```fsharp
type Subscription =
    | SubscribeToSession of SessionId * (SessionEvent -> unit)
    | SubscribeToUser of UserId * (Event -> unit)
    | SubscribeToLocation of Location * (CommunityDataEvent -> unit)

let subscribe (subscription: Subscription) : IDisposable =
    match subscription with
    | SubscribeToSession (sessionId, handler) ->
        EventStore.subscribe (fun e ->
            match e with
            | SessionEvent se when se.SessionId = sessionId ->
                handler se
            | _ -> ()
        )
```

## Event Storage

### Event Schema

```sql
CREATE TABLE events (
    event_id UUID PRIMARY KEY,
    aggregate_id UUID NOT NULL,
    aggregate_type TEXT NOT NULL,
    event_type TEXT NOT NULL,
    event_data JSONB NOT NULL,
    metadata JSONB,
    timestamp TIMESTAMPTZ NOT NULL,
    sequence_number BIGINT NOT NULL
);

CREATE INDEX idx_events_aggregate ON events(aggregate_id, sequence_number);
CREATE INDEX idx_events_timestamp ON events(timestamp);
```

### Event Serialization

```fsharp
let serialize (event: Event) : Json =
    match event with
    | SessionStarted e ->
        JsonObject [
            "type", JsonString "SessionStarted"
            "sessionId", JsonString (e.SessionId.ToString())
            "location", serializeLocation e.Location
            "startedAt", JsonString (e.StartedAt.ToString("O"))
            "userId", JsonString (e.UserId |> Option.map string |> Option.defaultValue null)
        ]
    | EntryAdded e ->
        // ... serialization logic
```

## Testing Strategy

### Event-Based Testing

```fsharp
[<Test>]
let ``Adding entry to active session produces EntryAdded event`` () =
    // Given
    let state = { State.initial with 
        CurrentSession = Some testSession 
    }
    let command = AddEntry {
        SessionId = testSession.SessionId
        Machine = Washer
        Quantity = Quantity.create 2 |> Result.get
        Price = Price.create 3.50m |> Result.get
        PaymentMethod = Quarters
    }
    
    // When
    let result = handleCommand state command
    
    // Then
    match result with
    | Ok [EntryAdded e] ->
        e.Machine |> should equal Washer
        e.Total |> should equal (Money.create 7.00m)
    | _ -> 
        failwith "Expected EntryAdded event"
```

### Projection Testing

```fsharp
[<Test>]
let ``Session view updates correctly with events`` () =
    // Given
    let events = [
        SessionStarted testSessionStarted
        EntryAdded testEntry1
        EntryAdded testEntry2
    ]
    
    // When
    let view = projectToSessionView events
    
    // Then
    match view with
    | Some v ->
        v.Entries |> should haveLength 2
        v.RunningTotal |> should equal (Money.create 10.50m)
        v.Status |> should equal InProgress
    | None ->
        failwith "Expected session view"
```

## Key Benefits

1. **Complete audit trail** - Every action recorded
2. **Time travel debugging** - Replay to any point
3. **Event replay** - Rebuild state from events
4. **GDPR compliance** - Selective event removal
5. **Analytics ready** - Events are perfect for analysis
6. **Conflict-free sync** - Events merge naturally

---

*The Event Model provides a robust foundation for LaundryLog, ensuring data integrity, enabling powerful features, and supporting future evolution.*