# PerDiemLog Data Model

## F# Type Definitions

### Domain Primitives

```fsharp
module PerDiemLog.Domain.Types

open System

// Value types for type safety
type TripId = TripId of Guid
type UserId = UserId of Guid
type ReportId = ReportId of Guid

type Location = Location of string option

type Money = Money of decimal

type TripDuration = {
    Days: int
} with
    member this.IsValid = this.Days >= 2 && this.Days <= 364

// Date range for trips
type DateRange = {
    Start: DateTime
    End: DateTime
} with
    member this.Duration = 
        (this.End.Date - this.Start.Date).Days + 1
    
    member this.IsValid = 
        this.End.Date >= this.Start.Date &&
        this.Duration >= 2 &&
        this.Duration <= 364
    
    member this.ContainsDate (date: DateTime) =
        date.Date >= this.Start.Date && date.Date <= this.End.Date
    
    member this.DaysInMonth (year: int) (month: int) =
        let firstOfMonth = DateTime(year, month, 1)
        let lastOfMonth = firstOfMonth.AddMonths(1).AddDays(-1.0)
        
        let effectiveStart = max this.Start.Date firstOfMonth
        let effectiveEnd = min this.End.Date lastOfMonth
        
        if effectiveStart > effectiveEnd then
            0
        else
            (effectiveEnd - effectiveStart).Days + 1

// Per diem rate structure
type PerDiemRates = {
    Year: int
    FullDayRate: Money
    EffectiveDate: DateTime
} with
    member this.PartialDayRate = 
        let (Money amount) = this.FullDayRate
        Money (amount * 0.75m)
```

### Trip Aggregate

```fsharp
type Trip = {
    Id: TripId
    UserId: UserId
    StartDate: DateTime
    EndDate: DateTime
    StartLocation: Location
    EndLocation: Location
    Notes: string option
    CreatedAt: DateTime
    UpdatedAt: DateTime
    DeletedAt: DateTime option
} with
    member this.DateRange = {
        Start = this.StartDate
        End = this.EndDate
    }
    
    member this.IsActive = this.DeletedAt.IsNone
    
    member this.TotalDays = this.DateRange.Duration
    
    member this.IsPartialMonth (year: int) (month: int) =
        let firstOfMonth = DateTime(year, month, 1)
        let lastOfMonth = firstOfMonth.AddMonths(1).AddDays(-1.0)
        this.StartDate.Date < firstOfMonth || this.EndDate.Date > lastOfMonth
```

### Per Diem Calculation Types

```fsharp
type DayType =
    | FullDay
    | PartialDay
    | NotInMonth

type TripDay = {
    Date: DateTime
    DayType: DayType
    TripId: TripId
}

type TripCalculation = {
    Trip: Trip
    FullDays: int
    PartialDays: int
    TotalPerDiem: Money
} with
    static member Calculate (trip: Trip) (rates: PerDiemRates) =
        let totalDays = trip.TotalDays
        let fullDays = max 0 (totalDays - 2)
        let partialDays = min totalDays 2
        
        let (Money fullRate) = rates.FullDayRate
        let (Money partialRate) = rates.PartialDayRate
        
        {
            Trip = trip
            FullDays = fullDays
            PartialDays = partialDays
            TotalPerDiem = Money (decimal fullDays * fullRate + decimal partialDays * partialRate)
        }

type MonthlyTripCalculation = {
    Trip: Trip
    Month: int
    Year: int
    DaysInMonth: int
    FullDaysInMonth: int
    PartialDaysInMonth: int
    PerDiemForMonth: Money
    ExtendsFromPreviousMonth: bool
    ExtendsIntoNextMonth: bool
} with
    static member Calculate (trip: Trip) (month: int) (year: int) (rates: PerDiemRates) =
        let firstOfMonth = DateTime(year, month, 1)
        let lastOfMonth = firstOfMonth.AddMonths(1).AddDays(-1.0)
        
        let daysInMonth = trip.DateRange.DaysInMonth year month
        
        if daysInMonth = 0 then
            None
        else
            let isStartDay date = date = trip.StartDate.Date
            let isEndDay date = date = trip.EndDate.Date
            
            let effectiveStart = max trip.StartDate.Date firstOfMonth
            let effectiveEnd = min trip.EndDate.Date lastOfMonth
            
            // Calculate full vs partial days for this month
            let mutable fullDays = 0
            let mutable partialDays = 0
            
            for i in 0 .. (effectiveEnd - effectiveStart).Days do
                let currentDate = effectiveStart.AddDays(float i)
                if isStartDay currentDate || isEndDay currentDate then
                    partialDays <- partialDays + 1
                else
                    fullDays <- fullDays + 1
            
            let (Money fullRate) = rates.FullDayRate
            let (Money partialRate) = rates.PartialDayRate
            
            Some {
                Trip = trip
                Month = month
                Year = year
                DaysInMonth = daysInMonth
                FullDaysInMonth = fullDays
                PartialDaysInMonth = partialDays
                PerDiemForMonth = Money (decimal fullDays * fullRate + decimal partialDays * partialRate)
                ExtendsFromPreviousMonth = trip.StartDate.Date < firstOfMonth
                ExtendsIntoNextMonth = trip.EndDate.Date > lastOfMonth
            }
```

### Report Types

```fsharp
type MonthlyReport = {
    ReportId: ReportId
    UserId: UserId
    Month: int
    Year: int
    GeneratedAt: DateTime
    Trips: MonthlyTripCalculation list
    TotalFullDays: int
    TotalPartialDays: int
    TotalPerDiem: Money
} with
    static member Generate (userId: UserId) (month: int) (year: int) (trips: Trip list) (rates: PerDiemRates) =
        let monthlyCalculations = 
            trips
            |> List.choose (fun trip -> 
                MonthlyTripCalculation.Calculate trip month year rates)
        
        let totalFullDays = monthlyCalculations |> List.sumBy (_.FullDaysInMonth)
        let totalPartialDays = monthlyCalculations |> List.sumBy (_.PartialDaysInMonth)
        
        let totalAmount = 
            monthlyCalculations 
            |> List.sumBy (fun calc -> 
                let (Money amount) = calc.PerDiemForMonth
                amount)
        
        {
            ReportId = ReportId (Guid.NewGuid())
            UserId = userId
            Month = month
            Year = year
            GeneratedAt = DateTime.UtcNow
            Trips = monthlyCalculations
            TotalFullDays = totalFullDays
            TotalPartialDays = totalPartialDays
            TotalPerDiem = Money totalAmount
        }

type ReportFormat =
    | PDF
    | HTML

type ReportRequest = {
    UserId: UserId
    Month: int
    Year: int
    Format: ReportFormat
}
```

### Calendar View Types

```fsharp
type CalendarDay = {
    Date: DateTime
    DayType: DayType
    TripId: TripId option
    TripInfo: Trip option
}

type CalendarMonth = {
    Month: int
    Year: int
    Days: CalendarDay list
} with
    static member Generate (month: int) (year: int) (trips: Trip list) =
        let firstOfMonth = DateTime(year, month, 1)
        let daysInMonth = DateTime.DaysInMonth(year, month)
        
        let days = 
            [0 .. daysInMonth - 1]
            |> List.map (fun dayOffset ->
                let date = firstOfMonth.AddDays(float dayOffset)
                
                // Find trip containing this date
                let tripInfo = 
                    trips 
                    |> List.tryFind (fun trip -> 
                        trip.IsActive && trip.DateRange.ContainsDate date)
                
                let dayType = 
                    match tripInfo with
                    | None -> NotInMonth
                    | Some trip -> 
                        if date = trip.StartDate.Date || date = trip.EndDate.Date then
                            PartialDay
                        else
                            FullDay
                
                {
                    Date = date
                    DayType = dayType
                    TripId = tripInfo |> Option.map (_.Id)
                    TripInfo = tripInfo
                })
        
        { Month = month; Year = year; Days = days }
```

## Commands

```fsharp
module PerDiemLog.Commands

type CreateTripCommand = {
    UserId: UserId
    StartDate: DateTime
    EndDate: DateTime
    StartLocation: string option
    EndLocation: string option
    Notes: string option
}

type UpdateTripCommand = {
    TripId: TripId
    UserId: UserId
    StartDate: DateTime option
    EndDate: DateTime option
    StartLocation: string option
    EndLocation: string option
    Notes: string option
}

type DeleteTripCommand = {
    TripId: TripId
    UserId: UserId
    Reason: string option
}

type UpdateRatesCommand = {
    Year: int
    FullDayRate: decimal
    EffectiveDate: DateTime
    UpdatedBy: UserId
}

type GenerateReportCommand = {
    UserId: UserId
    Month: int
    Year: int
    Format: ReportFormat
}
```

## Events

```fsharp
module PerDiemLog.Events

type TripCreated = {
    TripId: TripId
    UserId: UserId
    StartDate: DateTime
    EndDate: DateTime
    StartLocation: string option
    EndLocation: string option
    Notes: string option
    OccurredAt: DateTime
}

type TripUpdated = {
    TripId: TripId
    UserId: UserId
    Changes: Map<string, obj>
    PreviousValues: Map<string, obj>
    OccurredAt: DateTime
}

type TripDeleted = {
    TripId: TripId
    UserId: UserId
    Reason: string option
    OccurredAt: DateTime
}

type RatesUpdated = {
    Year: int
    FullDayRate: decimal
    PartialDayRate: decimal
    EffectiveDate: DateTime
    UpdatedBy: UserId
    OccurredAt: DateTime
}

type ReportGenerated = {
    ReportId: ReportId
    UserId: UserId
    Month: int
    Year: int
    TotalTrips: int
    TotalFullDays: int
    TotalPartialDays: int
    TotalPerDiem: decimal
    Format: string
    OccurredAt: DateTime
}

type PerDiemEvent =
    | TripCreated of TripCreated
    | TripUpdated of TripUpdated
    | TripDeleted of TripDeleted
    | RatesUpdated of RatesUpdated
    | ReportGenerated of ReportGenerated
```

## Validation

```fsharp
module PerDiemLog.Validation

type ValidationError =
    | InvalidDateRange of string
    | TripTooShort
    | TripTooLong
    | InvalidYear
    | InvalidRate
    | TripNotFound
    | Unauthorized

type ValidationResult<'T> = Result<'T, ValidationError list>

module Trip =
    let validateDateRange (startDate: DateTime) (endDate: DateTime) : ValidationResult<DateRange> =
        let range = { Start = startDate; End = endDate }
        
        if not range.IsValid then
            if range.Duration < 2 then
                Error [TripTooShort]
            elif range.Duration > 364 then
                Error [TripTooLong]
            else
                Error [InvalidDateRange "End date must be on or after start date"]
        else
            Ok range
    
    let validateCreate (cmd: CreateTripCommand) : ValidationResult<CreateTripCommand> =
        match validateDateRange cmd.StartDate cmd.EndDate with
        | Ok _ -> Ok cmd
        | Error errors -> Error errors

module Rates =
    let validateYear (year: int) : ValidationResult<int> =
        if year < 2000 || year > 2100 then
            Error [InvalidYear]
        else
            Ok year
    
    let validateRate (rate: decimal) : ValidationResult<decimal> =
        if rate <= 0m then
            Error [InvalidRate]
        else
            Ok rate
```

## Usage Examples

```fsharp
// Creating a trip
let tripId = TripId (Guid.NewGuid())
let userId = UserId (Guid.NewGuid())

let trip = {
    Id = tripId
    UserId = userId
    StartDate = DateTime(2024, 4, 19)
    EndDate = DateTime(2024, 5, 19)
    StartLocation = Location (Some "Denver, CO")
    EndLocation = Location (Some "Denver, CO")
    Notes = Some "Cross-country haul"
    CreatedAt = DateTime.UtcNow
    UpdatedAt = DateTime.UtcNow
    DeletedAt = None
}

// Calculate per diem for April
let rates = {
    Year = 2024
    FullDayRate = Money 69m
    EffectiveDate = DateTime(2024, 1, 1)
}

let aprilCalculation = MonthlyTripCalculation.Calculate trip 4 2024 rates
// Result: 1 partial + 11 full days = $51.75 + $759 = $810.75

// Generate monthly report
let allTrips = [trip] // Would normally load from database
let report = MonthlyReport.Generate userId 4 2024 allTrips rates
```

## Notes

- All money calculations use `decimal` for precision
- Dates always use `DateTime` in UTC
- GUIDs used for all entity IDs
- Option types for nullable fields
- Validation returns `Result<'T, ValidationError list>`
- Immutable data structures throughout
