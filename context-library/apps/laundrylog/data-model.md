# LaundryLog Data Model

**App:** LaundryLog  
**Language:** F#  
**Persistence:** SQLite + Event Store  
**Updated:** 2025-01-15  
**Status:** Implementation Ready

## Core Domain Types

### Identity Types

```fsharp
type SessionId = SessionId of Guid
type EntryId = EntryId of Guid  
type UserId = UserId of Guid
type LocationId = LocationId of Guid

module SessionId =
    let create () = SessionId (Guid.NewGuid())
    let value (SessionId id) = id
    let toString (SessionId id) = id.ToString()
    let parse str = 
        match Guid.TryParse str with
        | true, guid -> Ok (SessionId guid)
        | false, _ -> Error "Invalid SessionId format"
```

### Machine Types

```fsharp
type MachineType = 
    | Washer
    | Dryer

module MachineType =
    let toString = function
        | Washer -> "washer"
        | Dryer -> "dryer"
    
    let fromString = function
        | "washer" -> Ok Washer
        | "dryer" -> Ok Dryer
        | s -> Error $"Unknown machine type: {s}"
    
    let icon = function
        | Washer -> "💧"
        | Dryer -> "🔥"
```

### Value Types

```fsharp
[<Measure>] type USD
[<Measure>] type load

type Quantity = private Quantity of int<load>
type Price = private Price of decimal<USD/load>
type Money = Money of decimal<USD>

module Quantity =
    let min = 1<load>
    let max = 99<load>
    
    let create (n: int) : Result<Quantity, string> =
        let loads = n * 1<load>
        if loads < min then Error $"Quantity must be at least {min}"
        elif loads > max then Error $"Quantity cannot exceed {max}"
        else Ok (Quantity loads)
    
    let value (Quantity n) = n / 1<load>
    let loads (Quantity n) = n
    
    let increment (Quantity n) = create ((n / 1<load>) + 1)
    let decrement (Quantity n) = create ((n / 1<load>) - 1)

module Price =
    let create (amount: decimal) : Result<Price, string> =
        if amount < 0m then Error "Price cannot be negative"
        elif amount > 99.99m then Error "Price too high"
        else Ok (Price (amount * 1m<USD/load>))
    
    let value (Price p) = p / 1m<USD/load>
    let perLoad (Price p) = p

module Money =
    let create (amount: decimal) = Money (amount * 1m<USD>)
    let value (Money m) = m / 1m<USD>
    let zero = Money 0m<USD>
    let add (Money a) (Money b) = Money (a + b)
    let calculate (quantity: Quantity) (price: Price) =
        let (Quantity q) = quantity
        let (Price p) = price
        Money (decimal q * p / 1m<load>)
```

### Payment Types

```fsharp
type PaymentMethod =
    | Quarters
    | CreditCard of Last4: string
    | TruckStopPoints of AccountId: string  
    | Cash

module PaymentMethod =
    let toString = function
        | Quarters -> "quarters"
        | CreditCard last4 -> $"card ending {last4}"
        | TruckStopPoints _ -> "points"
        | Cash -> "cash"
    
    let icon = function
        | Quarters -> "🪙"
        | CreditCard _ -> "💳"
        | TruckStopPoints _ -> "⭐"
        | Cash -> "💵"
```

### Location Types

```fsharp
type Coordinates = {
    Latitude: float
    Longitude: float
}

type TruckStop = {
    Name: string
    Chain: string option  // "Love's", "Pilot", etc.
    StopNumber: string option
}

type Location = {
    Coordinates: Coordinates option
    Address: string
    TruckStop: TruckStop option
    Source: LocationSource
}

and LocationSource =
    | GPS of accuracy: float
    | Manual
    | IPGeolocation
    | Cached of age: TimeSpan

module Location =
    let create coords address = {
        Coordinates = Some coords
        Address = address
        TruckStop = TruckStop.identify address
        Source = GPS 10.0
    }
    
    let manualEntry address = {
        Coordinates = None
        Address = address  
        TruckStop = TruckStop.identify address
        Source = Manual
    }
```

## Session Types

### Active Session

```fsharp
type LaundryEntry = {
    EntryId: EntryId
    Machine: MachineType
    Quantity: Quantity
    Price: Price
    PaymentMethod: PaymentMethod
    Total: Money
    Timestamp: DateTime
}

type ActiveSession = {
    SessionId: SessionId
    Location: Location
    StartedAt: DateTime
    Entries: LaundryEntry list
    RunningTotal: Money
}

module ActiveSession =
    let create location = {
        SessionId = SessionId.create()
        Location = location
        StartedAt = DateTime.UtcNow
        Entries = []
        RunningTotal = Money.zero
    }
    
    let addEntry entry session = {
        session with
            Entries = entry :: session.Entries
            RunningTotal = Money.add session.RunningTotal entry.Total
    }
    
    let removeEntry entryId session = {
        session with
            Entries = session.Entries |> List.filter (fun e -> e.EntryId <> entryId)
            RunningTotal = session.Entries 
                          |> List.filter (fun e -> e.EntryId <> entryId)
                          |> List.sumBy (fun e -> e.Total)
    }
```

### Completed Session

```fsharp
type CompletedSession = {
    SessionId: SessionId
    Location: Location
    StartedAt: DateTime
    CompletedAt: DateTime
    Entries: NonEmptyList<LaundryEntry>
    Total: Money
    Receipt: Receipt
}

and Receipt = {
    ReceiptNumber: string
    GeneratedAt: DateTime
    IRSCompliant: bool
}

module CompletedSession =
    let complete (session: ActiveSession) : Result<CompletedSession, string> =
        match NonEmptyList.fromList session.Entries with
        | None -> Error "Cannot complete session without entries"
        | Some entries ->
            Ok {
                SessionId = session.SessionId
                Location = session.Location
                StartedAt = session.StartedAt
                CompletedAt = DateTime.UtcNow
                Entries = entries
                Total = session.RunningTotal
                Receipt = Receipt.generate session
            }
```

## User Profile Types

```fsharp
type UserPreferences = {
    DefaultLocation: Location option
    PreferredPaymentMethod: PaymentMethod option
    EnableCommunitySharing: bool
    EnableLocationServices: bool
    ExportFormat: ExportFormat
}

and ExportFormat =
    | CSV
    | PDF
    | QuickBooks
    | Excel

type UserProfile = {
    UserId: UserId
    Email: Email option
    CreatedAt: DateTime
    Preferences: UserPreferences
    Statistics: UserStatistics
}

and UserStatistics = {
    TotalSessions: int
    TotalSpent: Money
    AveragePerSession: Money
    MostFrequentLocation: Location option
    LastSessionDate: DateTime option
}
```

## Community Data Types

```fsharp
type PriceData = {
    LocationId: LocationId
    Machine: MachineType
    Prices: PricePoint list
    Average: Price
    Median: Price
    LastUpdated: DateTime
    Confidence: ConfidenceLevel
}

and PricePoint = {
    Price: Price
    ReportedAt: DateTime
    UserId: UserId  // Anonymized
}

and ConfidenceLevel =
    | Low      // < 5 data points
    | Medium   // 5-20 data points  
    | High     // > 20 data points

type CommunityData = {
    Location: Location
    WasherPrices: PriceData option
    DryerPrices: PriceData option
    TotalReports: int
    LastReport: DateTime option
}
```

## Validation Types

```fsharp
type ValidationError =
    | MissingMachineType
    | InvalidQuantity of reason: string
    | MissingPrice
    | InvalidPrice of reason: string
    | MissingPaymentMethod
    | MissingLocation
    | InvalidLocation of reason: string

type ValidationResult =
    | Valid
    | Invalid of ValidationError list

module Validation =
    let validateEntry (machine, quantity, price, payment) =
        let errors = ResizeArray<ValidationError>()
        
        if machine = None then
            errors.Add MissingMachineType
        
        match quantity with
        | None -> errors.Add (InvalidQuantity "Quantity required")
        | Some q when q < 1 -> errors.Add (InvalidQuantity "Must be positive")
        | Some q when q > 99 -> errors.Add (InvalidQuantity "Maximum 99")
        | _ -> ()
        
        match price with
        | None -> errors.Add MissingPrice
        | Some p when p < 0m -> errors.Add (InvalidPrice "Cannot be negative")
        | _ -> ()
        
        if payment = None then
            errors.Add MissingPaymentMethod
        
        if errors.Count = 0 then Valid
        else Invalid (errors |> List.ofSeq)
```

## Database Schema

### SQLite Tables

```sql
-- Sessions table
CREATE TABLE sessions (
    session_id TEXT PRIMARY KEY,
    user_id TEXT,
    location_json TEXT NOT NULL,
    started_at TEXT NOT NULL,
    completed_at TEXT,
    status TEXT NOT NULL,
    total REAL,
    receipt_json TEXT,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL
);

-- Entries table
CREATE TABLE entries (
    entry_id TEXT PRIMARY KEY,
    session_id TEXT NOT NULL,
    machine_type TEXT NOT NULL,
    quantity INTEGER NOT NULL,
    price REAL NOT NULL,
    payment_method TEXT NOT NULL,
    total REAL NOT NULL,
    timestamp TEXT NOT NULL,
    FOREIGN KEY (session_id) REFERENCES sessions(session_id)
);

-- User preferences
CREATE TABLE user_preferences (
    user_id TEXT PRIMARY KEY,
    preferences_json TEXT NOT NULL,
    updated_at TEXT NOT NULL
);

-- Community price data (cached)
CREATE TABLE community_prices (
    location_id TEXT,
    machine_type TEXT,
    price_data_json TEXT NOT NULL,
    updated_at TEXT NOT NULL,
    PRIMARY KEY (location_id, machine_type)
);

-- Indexes
CREATE INDEX idx_sessions_user ON sessions(user_id);
CREATE INDEX idx_sessions_status ON sessions(status);
CREATE INDEX idx_entries_session ON entries(session_id);
CREATE INDEX idx_entries_timestamp ON entries(timestamp);
```

## JSON Serialization

```fsharp
module Json =
    open System.Text.Json
    
    let serializeLocation (loc: Location) =
        JsonObject [
            match loc.Coordinates with
            | Some coords ->
                "latitude", JsonNumber coords.Latitude
                "longitude", JsonNumber coords.Longitude
            | None -> ()
            "address", JsonString loc.Address
            match loc.TruckStop with
            | Some ts ->
                "truckStop", JsonObject [
                    "name", JsonString ts.Name
                    match ts.Chain with
                    | Some chain -> "chain", JsonString chain
                    | None -> ()
                ]
            | None -> ()
            "source", JsonString (loc.Source.ToString())
        ]
    
    let deserializeLocation (json: string) : Location =
        let doc = JsonDocument.Parse(json)
        let root = doc.RootElement
        {
            Coordinates = 
                if root.TryGetProperty("latitude", &lat) &&
                   root.TryGetProperty("longitude", &lon) then
                    Some { Latitude = lat.GetDouble(); Longitude = lon.GetDouble() }
                else None
            Address = root.GetProperty("address").GetString()
            TruckStop = // ... deserialize truck stop
            Source = // ... deserialize source
        }
```

## Key Design Decisions

1. **Single-case unions for type safety** - Prevents ID mixups
2. **Private constructors with validation** - Ensures valid data
3. **Units of measure for calculations** - Type-safe money math
4. **NonEmptyList for completed sessions** - Can't complete without entries
5. **Event sourcing compatible** - Immutable, event-driven updates
6. **JSON for complex fields** - Flexibility without schema migrations
7. **Cached community data** - Performance with eventual consistency

---

*This data model ensures type safety, domain integrity, and supports both local-first operation and optional community features.*