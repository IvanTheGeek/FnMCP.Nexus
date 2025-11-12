# PerDiemLog Event Model

## Overview

PerDiemLog follows Event Modeling methodology with Event Sourcing architecture. This document defines the commands, events, and read models for the application.

## Commands (User Actions)

### Trip Management

**CreateTrip**
```yaml
userId: uuid
startDate: date
endDate: date
startLocation: string (optional)
endLocation: string (optional)
notes: string (optional)
```

**UpdateTrip**
```yaml
tripId: uuid
userId: uuid
startDate: date (optional - if changing)
endDate: date (optional - if changing)
startLocation: string (optional)
endLocation: string (optional)
notes: string (optional)
```

**DeleteTrip**
```yaml
tripId: uuid
userId: uuid
reason: string (optional)
```

### Rate Management

**UpdatePerDiemRates**
```yaml
year: int
fullDayRate: decimal
effectiveDate: date
updatedBy: uuid (admin only)
notes: string (optional)
```

### Report Generation

**GenerateMonthlyReport**
```yaml
userId: uuid
month: int
year: int
format: enum [PDF, HTML]
```

## Events (Facts That Happened)

### Trip Events

**TripCreated**
```yaml
tripId: uuid
userId: uuid
startDate: date
endDate: date
startLocation: string
endLocation: string
notes: string
occurredAt: timestamp
```

**TripUpdated**
```yaml
tripId: uuid
userId: uuid
changes: map of changed fields
previousValues: map of old values
occurredAt: timestamp
```

**TripDeleted**
```yaml
tripId: uuid
userId: uuid
reason: string
occurredAt: timestamp
```

### Rate Events

**PerDiemRatesUpdated**
```yaml
year: int
fullDayRate: decimal
partialDayRate: decimal (calculated)
effectiveDate: date
updatedBy: uuid
previousRates: object (for audit)
occurredAt: timestamp
```

### Report Events

**MonthlyReportGenerated**
```yaml
reportId: uuid
userId: uuid
month: int
year: int
totalTrips: int
totalFullDays: int
totalPartialDays: int
totalPerDiem: decimal
format: string
occurredAt: timestamp
```

## Read Models (Views)

### Active Trips View
**Purpose:** Display user's current and recent trips

```yaml
userId: uuid
trips: array of:
  tripId: uuid
  startDate: date
  endDate: date
  startLocation: string
  endLocation: string
  durationDays: int
  status: enum [active, completed]
  createdAt: timestamp
sortedBy: startDate descending
```

### Monthly Summary View
**Purpose:** Calculate per diem totals for a specific month

```yaml
userId: uuid
month: int
year: int
trips: array of:
  tripId: uuid
  startDate: date (full trip)
  endDate: date (full trip)
  daysInMonth: int
  fullDaysInMonth: int
  partialDaysInMonth: int
  perDiemForMonth: decimal
  extendsFromPreviousMonth: bool
  extendsIntoNextMonth: bool
totalFullDays: int
totalPartialDays: int
totalPerDiem: decimal
```

### Calendar View
**Purpose:** Visual representation of trips for a month

```yaml
userId: uuid
month: int
year: int
days: array of:
  date: date
  dayType: enum [no_trip, partial_day, full_day]
  tripId: uuid (if applicable)
  tripInfo: object (start/end dates, locations)
```

### Current Rates View
**Purpose:** Display active per diem rates

```yaml
currentYear: int
fullDayRate: decimal
partialDayRate: decimal
effectiveDate: date
previousYears: array of historical rates
```

## Event Flows (Paths)

### Happy Path: Create Trip and Generate Report

1. **User Action:** CreateTrip command
2. **System:** Validate dates (>= 2 days, <= 364 days)
3. **Event:** TripCreated
4. **Update:** Active Trips View updated
5. **Update:** Monthly Summary View(s) updated (possibly multiple months)
6. **User Views:** Trip appears in list
7. **User Action:** GenerateMonthlyReport command
8. **System:** Calculate from Monthly Summary View
9. **Event:** MonthlyReportGenerated
10. **User Views:** PDF report with trip details

### Month Boundary Path: Trip Spans Months

1. **User Action:** CreateTrip (April 19 - May 19)
2. **Event:** TripCreated
3. **Update:** Monthly Summary View for April (12 days)
4. **Update:** Monthly Summary View for May (19 days)
5. **User Views:** April report shows partial trip with continuation indicator
6. **User Views:** May report shows partial trip with prior month indicator

### Error Path: Invalid Trip Duration

1. **User Action:** CreateTrip (same start/end date)
2. **System:** Validation fails
3. **Response:** Error - "Trip must be at least 2 days"
4. **No Event Created**
5. **User Views:** Error message with guidance

## Projections

### Trip Aggregates
Built from TripCreated, TripUpdated, TripDeleted events  
Projects current state of each trip

### Monthly Calculations
Built from Trip Aggregates + Current Rates  
Projects per diem totals for any month/year

### Rate History
Built from PerDiemRatesUpdated events  
Projects historical rates for recalculation

## Event Sourcing Benefits

1. **Audit Trail:** Complete history of all trip changes
2. **Recalculation:** Can replay with new rates if IRS updates
3. **Time Travel:** View state as of any past date
4. **Debugging:** Understand exactly what happened and when
5. **IRS Compliance:** Immutable record of all transactions

## Implementation Notes

- Events are immutable once stored
- Commands can be rejected (validation)
- Events represent facts (always past tense)
- Read models are projections from events
- Multiple read models can project from same events
- Events flow: Command → Validation → Event → Update Projections → View
