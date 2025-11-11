# Domain Modeling Patterns

**Framework:** FnMCP.IvanTheGeekDevFramework  
**Language:** F#  
**Approach:** Type-Driven Development  
**Updated:** 2025-01-15  
**Status:** Core Technical Pattern

## Make Illegal States Unrepresentable

### The Principle

If a state shouldn't exist in your domain, make it impossible to create in your types. The compiler becomes your first line of defense against bugs.

### Before and After

❌ **Bad: Illegal states possible**
```fsharp
type Session = {
    Id: Guid
    Entries: Entry list  // Could be empty!
    Total: decimal       // Might not match entries!
    Status: string       // "Active"? "Complete"? "COMPLTED"? (typo)
    CompletedAt: DateTime option  // Completed but no date?
}
```

✅ **Good: Illegal states impossible**
```fsharp
type SessionId = SessionId of Guid

type ActiveSession = {
    Id: SessionId
    Entries: Entry list  // Can be empty while building
    StartedAt: DateTime
}

type CompletedSession = {
    Id: SessionId
    Entries: NonEmptyList<Entry>  // Must have entries!
    Total: PositiveMoney           // Calculated, not stored
    CompletedAt: DateTime          // Always has completion time
}

type Session =
    | Active of ActiveSession
    | Completed of CompletedSession
    // Can't be completed without entries
    // Can't have completed time while active
    // Status is encoded in type, no strings
```

## Single-Case Discriminated Unions

### Wrapper Types for Domain Concepts

Don't use primitives for domain concepts:

```fsharp
// Bad: Primitives everywhere
type Entry = {
    Quantity: int
    Price: decimal
    Location: string
}

// Good: Domain types
type Quantity = private Quantity of int
type Price = private Price of decimal<USD>
type Location = Location of string

type Entry = {
    Quantity: Quantity
    Price: Price
    Location: Location
}
```

### Smart Constructors

Validate at construction time:

```fsharp
module Quantity =
    let create n =
        if n < 1 then Error "Quantity must be positive"
        elif n > 99 then Error "Maximum quantity is 99"
        else Ok (Quantity n)
    
    let value (Quantity n) = n
    
    let increment (Quantity n) =
        create (n + 1)
    
    let decrement (Quantity n) =
        create (n - 1)
```

### Type-Safe IDs

Never mix up IDs:

```fsharp
type SessionId = SessionId of Guid
type EntryId = EntryId of Guid
type UserId = UserId of Guid

// Compiler prevents this mistake:
let findEntry (sessionId: SessionId) (entryId: EntryId) =
    // Can't accidentally pass UserId here
    database.FindEntry sessionId entryId
```

## Record Types with Validation

### Creating Valid Records

```fsharp
type Email = private Email of string

module Email =
    let create str =
        if String.IsNullOrWhiteSpace str then
            Error "Email cannot be empty"
        elif not (str.Contains "@") then
            Error "Email must contain @"
        else
            Ok (Email str)
    
    let value (Email str) = str

type ContactInfo = {
    Email: Email
    Phone: Phone option
}

// Can only create with valid email
let createContact email phone =
    match Email.create email with
    | Ok validEmail -> 
        Ok { Email = validEmail; Phone = phone }
    | Error msg -> 
        Error msg
```

## Discriminated Unions for States

### Modeling State Machines

```fsharp
type LaundryMachine =
    | Washer
    | Dryer

type PaymentMethod =
    | Quarters
    | CreditCard of last4:string
    | TruckStopPoints of accountId:string
    | Cash

type EntryState =
    | Draft of DraftEntry
    | Validated of ValidatedEntry
    | Saved of SavedEntry

// State transitions
let validate (draft: DraftEntry): Result<ValidatedEntry, ValidationError> =
    // Validation logic
    
let save (validated: ValidatedEntry): Async<SavedEntry> =
    // Save to database
```

### Modeling Results and Errors

```fsharp
type LocationError =
    | GPSUnavailable
    | GPSTimeout of seconds:int
    | PermissionDenied
    | UnknownLocation

type SaveError =
    | NetworkError of exn
    | StorageFull
    | InvalidData of ValidationError
    | ConcurrencyConflict of conflictingId:Guid

type OperationResult<'T> =
    | Success of 'T
    | Failure of SaveError
```

## Option and Result Types

### Avoiding Nulls

```fsharp
// Bad: Nullable everything
type Session = {
    Location: Location  // Could be null!
    Notes: string       // Could be null!
    Receipt: Receipt    // Could be null!
}

// Good: Explicit optionality
type Session = {
    Location: Location        // Required
    Notes: string option     // Optional
    Receipt: Receipt option  // Optional
}

// Usage is clear
match session.Notes with
| Some notes -> displayNotes notes
| None -> displayNoNotes()
```

### Result for Error Handling

```fsharp
// Bad: Exceptions for control flow
let addEntry entry =
    if entry.Quantity <= 0 then
        raise (ArgumentException "Invalid quantity")
    // ... more validation throwing exceptions
    
// Good: Result type
let addEntry entry: Result<Session, AddEntryError> =
    match validateEntry entry with
    | Ok validEntry ->
        session
        |> Session.addEntry validEntry
        |> Ok
    | Error validationError ->
        Error (ValidationFailed validationError)
```

## NonEmptyList Pattern

### Ensuring Collections Have Items

```fsharp
type NonEmptyList<'T> = {
    Head: 'T
    Tail: 'T list
}

module NonEmptyList =
    let create head tail = {
        Head = head
        Tail = tail
    }
    
    let singleton item = {
        Head = item
        Tail = []
    }
    
    let toList nel =
        nel.Head :: nel.Tail
    
    let fromList = function
        | [] -> None
        | head :: tail -> Some (create head tail)
    
    let map f nel = {
        Head = f nel.Head
        Tail = List.map f nel.Tail
    }
```

### Usage in Domain

```fsharp
type CompletedSession = {
    Entries: NonEmptyList<Entry>  // Must have at least one!
    // ... other fields
}

let completeSession (entries: Entry list) =
    match NonEmptyList.fromList entries with
    | Some nonEmptyEntries ->
        Ok { Entries = nonEmptyEntries; CompletedAt = DateTime.Now }
    | None ->
        Error "Cannot complete session without entries"
```

## Units of Measure

### Type-Safe Calculations

```fsharp
[<Measure>] type USD
[<Measure>] type load

type LaundryPrice = decimal<USD/load>

let calculateTotal (quantity: int<load>) (price: LaundryPrice) =
    decimal quantity * price  // Result is decimal<USD>

// Compiler prevents unit errors
let quantity = 3<load>
let price = 3.50m<USD/load>
let total = calculateTotal quantity price  // 10.50<USD>
```

## Phantom Types

### Encoding State in Types

```fsharp
type ValidationState = Unvalidated | Validated

type Entry<'State> = {
    Quantity: int
    Price: decimal
    _phantom: 'State option
}

// Functions that only work on validated entries
let save (entry: Entry<Validated>): Async<unit> =
    async {
        // Can only save validated entries
    }

// Validation produces correct type
let validate (entry: Entry<Unvalidated>): Result<Entry<Validated>, ValidationError> =
    // Validation logic
```

## Function Composition Patterns

### Pipeline Pattern

```fsharp
let processLaundrySession location =
    location
    |> GPS.capture
    |> Result.bind Session.create
    |> Result.map (Session.addDefaultEntries)
    |> Result.map (Session.calculateTotals)
    |> Async.map (saveToDatabase)
    |> Async.map (notifyUser)
```

### Kleisli Composition

```fsharp
let (>=>) f g = fun x -> 
    match f x with
    | Ok y -> g y
    | Error e -> Error e

let processEntry =
    validateEntry
    >=> enrichWithLocation
    >=> calculateTax
    >=> saveToDatabase
```

## Active Patterns

### Domain-Specific Pattern Matching

```fsharp
let (|HighVolume|Normal|Low|) quantity =
    if quantity > 10 then HighVolume
    elif quantity > 3 then Normal
    else Low

let (|TruckStop|RestArea|Other|) location =
    if location.Name.Contains("Love's") || 
       location.Name.Contains("Pilot") then TruckStop
    elif location.Name.Contains("Rest Area") then RestArea
    else Other

// Usage
let calculateDiscount quantity location =
    match quantity, location with
    | HighVolume, TruckStop -> 0.10m  // 10% discount
    | HighVolume, _ -> 0.05m
    | Normal, TruckStop -> 0.03m
    | _ -> 0.00m
```

## Type Providers (Advanced)

### Compile-Time Data Access

```fsharp
// JSON Type Provider for configuration
type Config = JsonProvider<"config.json">

let config = Config.Load("production.json")
let apiUrl = config.ApiUrl  // Typed access!

// SQL Type Provider for database
type DB = SqlDataProvider<ConnectionString="...">

let getSessionsByUser userId =
    query {
        for session in DB.Sessions do
        where (session.UserId = userId)
        select session
    }
```

## Computation Expressions

### Custom Workflows

```fsharp
type ValidationBuilder() =
    member _.Bind(x, f) = 
        match x with
        | Ok value -> f value
        | Error e -> Error e
    
    member _.Return(x) = Ok x
    member _.ReturnFrom(x) = x

let validation = ValidationBuilder()

let validateEntry entry = validation {
    let! quantity = validateQuantity entry.Quantity
    let! price = validatePrice entry.Price
    let! location = validateLocation entry.Location
    return {
        Quantity = quantity
        Price = price
        Location = location
    }
}
```

## Event Sourcing Types

### Modeling Events

```fsharp
type SessionEvent =
    | SessionStarted of SessionStartedData
    | EntryAdded of EntryAddedData
    | EntryRemoved of EntryId
    | SessionCompleted of SessionCompletedData
    | SessionCancelled of CancellationReason

and SessionStartedData = {
    SessionId: SessionId
    Location: Location
    StartedAt: DateTime
}

and EntryAddedData = {
    SessionId: SessionId
    EntryId: EntryId
    Machine: LaundryMachine
    Quantity: Quantity
    Price: Price
}

// Event sourcing fold
let apply state event =
    match event with
    | SessionStarted data ->
        { state with 
            Id = data.SessionId
            Location = data.Location
            Status = Active }
    | EntryAdded data ->
        { state with 
            Entries = data :: state.Entries }
    | SessionCompleted data ->
        { state with 
            Status = Completed
            CompletedAt = Some data.CompletedAt }
    // ... handle other events
```

## Testing Patterns

### Property-Based Testing

```fsharp
open FsCheck

[<Property>]
let ``Quantity increment then decrement returns original`` quantity =
    quantity > 0 && quantity < 99 ==> lazy
    let original = Quantity.create quantity |> Result.get
    let result = 
        original 
        |> Quantity.increment 
        |> Result.bind Quantity.decrement
    result = Ok original

[<Property>]
let ``Session total equals sum of entries`` (entries: Entry list) =
    let session = { 
        Entries = entries
        Total = calculateTotal entries 
    }
    session.Total = (entries |> List.sumBy (fun e -> e.Total))
```

## Best Practices

### Type Design Guidelines

1. **Start with types** - Design types before implementation
2. **Use discriminated unions** - For closed sets of choices
3. **Wrap primitives** - Create domain types
4. **Validate at boundaries** - Parse, don't validate
5. **Make invalid states impossible** - Use types as documentation

### Function Design Guidelines

1. **Pure functions** - Separate pure from impure
2. **Total functions** - Return Result, not exceptions
3. **Composition** - Small functions that compose
4. **Pipeline-friendly** - Data parameter last
5. **Async at edges** - Keep core logic synchronous

### Module Organization

```fsharp
module LaundryLog.Domain =
    // Types first
    type SessionId = SessionId of Guid
    type Session = // ...
    
    // Smart constructors
    module Session =
        let create location = // ...
        let addEntry entry session = // ...
        
    // Business logic
    module BusinessRules =
        let calculateDiscount = // ...
        let validateEntry = // ...
```

## Key Takeaways

1. **Types are documentation** - They explain the domain
2. **Compiler is your friend** - Let it catch errors
3. **Parse, don't validate** - Return new types, not booleans
4. **Composition over inheritance** - Build from simple parts
5. **Make impossible states impossible** - Through type design

---

*Domain modeling in F# transforms business rules into types that the compiler enforces. This approach eliminates entire categories of bugs and makes the code self-documenting.*