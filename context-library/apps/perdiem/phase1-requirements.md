# PerDiemLog Phase 1 Requirements

## Phase Overview

**Goal:** Manual trip entry with automated calculations and modern report generation

**Not in scope for Phase 1:**
- Automated PDF downloads (Phase 2)
- Log parsing/automation (Phase 3)
- Direct ELD device integration (never in scope)

## Core Features

### 1. Trip Management

**Create Trip**
- Start date (required)
- End date (required)
- Start location (optional but recommended)
- End location (optional but recommended)
- Trip notes/memo (optional)

**Validation Rules:**
- End date must be >= start date
- Trip must be minimum 2 days
- Trip must be maximum 364 days
- Warn if trip duration seems unusual (e.g., >30 days)

**Edit Trip**
- Modify any trip field
- Recalculate per diem automatically
- Preserve trip ID for audit trail

**Delete Trip**
- Soft delete with confirmation
- Maintain deletion audit trail

### 2. Per Diem Calculation

**Rate Structure (2023 rates as example):**
- Full day rate: $69 (100%)
- Partial day rate: $51.75 (75% of full day)
- Rates change annually - system must support rate updates

**Day Type Calculation:**
- **Departure day:** Partial (75%)
- **Return day:** Partial (75%)
- **Full days away:** Full (100%)
- **2-day trip:** 2 partial days
- **3+ day trip:** 2 partial + (n-2) full days

**Month Boundary Logic:**
- Trip spans multiple months: Common occurrence
- Calculate days for each month separately
- Report shows only days within that month
- Display indicates trip continuation beyond boundaries

**Example calculations:**

*Trip: April 19 - May 19 (31 days total)*  
For April report:
- Days in April: 12 (April 19-30)
- Breakdown: 1 partial (19th departure) + 11 full (20-30)
- April total: $51.75 + ($69 × 11) = $810.75

For May report:
- Days in May: 19 (May 1-19)
- Breakdown: 18 full (1-18) + 1 partial (19th return)
- May total: ($69 × 18) + $51.75 = $1,293.75

### 3. Report Generation

**Monthly Report Format:**

**Header Section:**
- Driver name (user profile)
- Month/Year
- Report generation date
- Tax year

**Trip List View:**
- Full trip date range (even if extends beyond month)
- Total trip duration
- Start/end locations
- Days in current month only
- Breakdown: Partial/Full days (for current month)
- Per diem total for days in month
- Visual indicators for trips extending into prev/next month

**Calendar View:**
- Current month only
- Color-coded days:
  - Full days: Distinct color/style
  - Partial days: Different color/style
  - Days outside trips: Neutral
- Trips extending beyond month: Visual indication at boundaries
- Tap day: Show trip details

**Summary Section:**
- Total full days (in month)
- Total partial days (in month)
- Full day subtotal: (count × $69)
- Partial day subtotal: (count × $51.75)
- **Grand total per diem for month**

**Footer:**
- Disclaimer: "This report is for tax filing purposes. Maintain DOT HOS logs as supporting documentation for IRS audits."
- Report generated timestamp

**Export Options:**
- PDF (primary format for tax filing)
- Print-friendly format
- Email/share capability

### 4. Rate Management

**Current Rates:**
- Display current per diem rates
- Show effective year
- Full day rate
- Partial day calculation (75%)

**Rate Updates:**
- Admin function to update rates when IRS announces changes
- Historical rates maintained for past trip calculations
- Warning when generating report using old rates

**Rate History:**
- View past year rates
- Used for recalculating old trips if needed

## UI Requirements

### Mobile-First Design
- Thumb-friendly controls
- Large touch targets (minimum 44×44pt)
- Bottom navigation for primary actions
- Top-sheet patterns for complex inputs
- Offline capability for data entry

### Screen States (Static-State Design)

**Dashboard/Home:**
- Current month summary
- Quick trip entry button
- Recent trips list
- Generate report action

**Add/Edit Trip:**
- Date pickers (start/end)
- Location input (autocomplete if possible)
- Notes field
- Save/cancel actions
- Real-time per diem calculation preview

**Trip List:**
- Chronological list
- Filter by month/date range
- Search by location
- Tap trip: View details / Edit
- Swipe: Quick delete

**Report View:**
- Month selector
- Trip list for month
- Calendar view toggle
- Summary section
- Export/share actions

**Settings:**
- User profile (name for reports)
- Current per diem rates display
- Rate update function (admin)
- Data backup/restore
- About/help

## Data Requirements

### Trip Record
```yaml
id: uuid
userId: uuid
startDate: date
endDate: date
startLocation: string (optional)
endLocation: string (optional)
notes: string (optional)
createdAt: timestamp
updatedAt: timestamp
deletedAt: timestamp (soft delete)
```

### Per Diem Rates
```yaml
year: int
fullDayRate: decimal
partialDayRate: decimal (calculated as fullDay * 0.75)
effectiveDate: date
notes: string (optional)
```

### Calculated Trip Summary (derived)
```yaml
tripId: uuid
totalDays: int
fullDays: int
partialDays: int
totalPerDiem: decimal
```

### Monthly Report (derived per month)
```yaml
month: int
year: int
trips: array of trip summaries (for days in month)
totalFullDays: int
totalPartialDays: int
totalPerDiem: decimal
```

## Technical Requirements

### Validation
- Client-side validation for immediate feedback
- Server-side validation for data integrity
- Clear error messages in plain language

### Data Integrity
- Immutable trip records (edit creates new version)
- Audit trail for changes
- Calculation results cached but recalculable

### Performance
- Trip list: Load last 90 days by default
- Pagination for older trips
- Report generation: <2 seconds for typical month

### Security
- User authentication required
- Data encryption at rest
- HTTPS for all communications
- User owns their data (GDPR/privacy compliant)

## Future Enhancements (Post-Phase 1)

**Phase 2:**
- Automated PDF download from HOS log website
- Local/cloud storage of log PDFs
- Bulk import trips from external source

**Phase 3:**
- PDF parsing to auto-populate trip data
- Machine learning for location recognition
- Smart trip suggestions based on patterns

## Success Criteria

**Phase 1 Complete when:**
1. User can manually enter trips
2. System calculates per diem accurately for all scenarios
3. Reports generate correctly including month-boundary trips
4. Reports are polished, modern, IRS-audit-ready
5. Mobile UX is smooth and intuitive
6. Data is secure and backed up
