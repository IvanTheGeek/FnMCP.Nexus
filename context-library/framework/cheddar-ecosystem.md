# Cheddar Ecosystem

**Framework:** FnMCP.IvanTheGeekDevFramework  
**Vision:** Integrated Financial Tools for Independent Workers  
**Updated:** 2025-01-15  
**Status:** Active Development

## The Vision

The Cheddar ecosystem is a suite of integrated financial tools designed for independent workers - truck drivers, freelancers, contractors, and small business owners. Each app solves a specific problem excellently while sharing infrastructure, data, and user experience patterns.

## Core Apps

### LaundryLog
**Status:** v7 Design Complete, Implementation Starting

**Problem Solved:**
- Truck stop laundry machines don't provide receipts
- IRS requires documentation for business expenses
- Drivers lose hundreds in deductions annually

**Key Features:**
- GPS location capture for IRS compliance
- Minimal data entry (95% buttons)
- Community price database
- Session-based expense tracking

**Target Users:**
- Long-haul truck drivers
- Regional drivers
- Owner-operators
- Fleet drivers tracking reimbursables

### PerDiemLog
**Status:** Phase 1 Design in Progress

**Problem Solved:**
- IRS per diem rules are complex for truckers
- DOT Hours of Service must align with per diem
- Manual tracking is error-prone
- Missing deductions costs thousands annually

**Development Phases:**
1. **Phase 1:** Manual entry with IRS rate lookup
2. **Phase 2:** Automated PDF downloads from IRS
3. **Phase 3:** PDF parsing for rate extraction

**Integration with LaundryLog:**
- Shared location services
- Combined expense reporting
- Unified session concept

### CheddarBooks (Future)
**Status:** Conceptual Planning

**Problem Solved:**
- QuickBooks is expensive and overly complex
- Most small businesses need 20% of features
- Trucking-specific needs ignored
- Privacy concerns with cloud-only solutions

**Planned Features:**
- Income/expense tracking
- Mileage integration
- DOT compliance reports
- IFTA fuel tax calculation
- Quarterly tax estimates
- Bank reconciliation

**Differentiators:**
- Local-first data storage
- Trucking-specific workflows
- Integration with Log and PerDiem apps
- Simple enough for non-accountants

### CheddarDocs (Future)
**Status:** Conceptual

**Purpose:**
- Document storage for business records
- Scan receipts with OCR
- Organize by tax categories
- Integration with CheddarBooks

### CheddarMiles (Future)
**Status:** Conceptual

**Purpose:**
- Automatic mileage tracking
- Business vs personal classification
- Integration with CheddarBooks
- IFTA reporting support

## Integration Architecture

### Shared Infrastructure

**Data Layer:**
```fsharp
// Shared domain types across apps
module Cheddar.Core =
    type Location = {
        Latitude: float
        Longitude: float
        Address: string option
        TruckStop: TruckStop option
    }
    
    type Money = decimal<USD>
    
    type TaxCategory =
        | Meals
        | Laundry
        | Fuel
        | Maintenance
        | Supplies
        | Other of string
```

**Service Layer:**
```fsharp
// Shared services
module Cheddar.Services =
    type ILocationService =
        abstract GetCurrentLocation: unit -> Async<Location>
        abstract ResolveAddress: Coordinates -> Async<Address>
        abstract FindNearbyTruckStops: Coordinates -> Async<TruckStop list>
    
    type ICommunityDataService =
        abstract GetPriceData: Location -> ItemType -> Async<PriceData>
        abstract SubmitPriceData: PriceReport -> Async<unit>
```

### Data Sharing

**User-Controlled Sharing:**
- Each app works standalone
- Users choose what to share between apps
- Shared data improves all apps
- Privacy always respected

**Example Flow:**
```
LaundryLog captures location
    ↓ (user approves sharing)
PerDiemLog uses same location for per diem
    ↓ (user approves sharing)
CheddarBooks imports both for expense report
```

### Community Database

**Opt-in Collaborative Intelligence:**

```fsharp
type CommunityData = {
    TruckStopPrices: Map<TruckStopId, PriceHistory>
    CommonRoutes: Map<RouteId, RouteData>
    ServiceRatings: Map<ServiceId, Rating>
}

type PriceHistory = {
    Laundry: PricePoint list
    Fuel: PricePoint list
    Parking: PricePoint list
    LastUpdated: DateTime
    ConfidenceScore: float
}
```

**Privacy-Preserving Aggregation:**
- No individual user data exposed
- Aggregated anonymously
- Time-delayed to prevent tracking
- Users can opt-out anytime

## The Influence System

### Earning Influence

**Code Contributions:**
```yaml
influence_earned:
  bug_fix: 10 points
  feature_small: 25 points
  feature_large: 100 points
  documentation: 15 points
  test_coverage: 20 points
```

**Financial Support:**
```yaml
influence_earned:
  monthly_subscription: 5 points/month
  annual_subscription: 75 points/year
  one_time_donation: 1 point per $1
  bounty_sponsorship: 2 points per $1
```

**Community Contributions:**
```yaml
influence_earned:
  answer_forum_question: 2 points
  quality_bug_report: 5 points
  feature_design_doc: 15 points
  beta_testing: 10 points
  tutorial_creation: 20 points
```

### Using Influence

**Feature Prioritization:**
```fsharp
type FeatureRequest = {
    Id: Guid
    Description: string
    EstimatedEffort: int
    Votes: Map<UserId, InfluencePoints>
}

let prioritizeFeatures (requests: FeatureRequest list) =
    requests
    |> List.map (fun r -> 
        r, r.Votes |> Map.toList |> List.sumBy snd)
    |> List.sortByDescending snd
    |> List.map fst
```

**Governance Decisions:**
- Breaking changes require influence vote
- Design direction discussions
- Resource allocation
- Community guidelines

### Influence Transparency

**Public Ledger:**
```yaml
user: ivan_the_geek
total_influence: 1,847 points
earned_from:
  - code_contributions: 1,200 points
  - documentation: 180 points
  - community_help: 267 points
  - financial_support: 200 points
currently_allocated:
  - feature_request_42: 100 points
  - design_decision_7: 50 points
available: 1,697 points
```

## Revenue Model

### Individual Users

**Free Forever:**
- Self-hosted version
- All source code
- Community support
- Basic features

**Premium ($5-10/month):**
- Hosted service
- Automatic backups
- Priority support
- Beta features
- Additional influence

### Business Users

**Small Business ($50/month):**
- Hosted service for team
- Email support
- Custom reports
- API access
- Training materials

**Enterprise ($500/month):**
- Self-hosted support
- SLA guarantees
- Custom features
- Dedicated support
- Compliance reports

### Revenue Allocation

```yaml
revenue_distribution:
  development: 40%
  infrastructure: 20%
  support: 20%
  contributor_pool: 10%
  reserve_fund: 10%
```

## Technical Standards

### Shared Patterns

**UI Components:**
```fsharp
// Shared Cheddar UI components
module Cheddar.UI =
    type ButtonSize = Small | Medium | Large
    type ButtonStyle = Primary | Secondary | Danger
    
    type CheddarButton = {
        Text: string
        Size: ButtonSize
        Style: ButtonStyle
        OnClick: unit -> unit
    }
```

**Data Persistence:**
```fsharp
// Shared storage pattern
module Cheddar.Storage =
    type IStorage<'T> =
        abstract Save: 'T -> Async<unit>
        abstract Load: unit -> Async<'T option>
        abstract Delete: unit -> Async<unit>
        abstract Export: Format -> Async<byte[]>
```

**Synchronization:**
```fsharp
// Optional sync service
module Cheddar.Sync =
    type SyncStrategy =
        | LocalOnly
        | ManualSync
        | AutoSync of TimeSpan
    
    type ISyncService =
        abstract Sync: SyncStrategy -> Async<SyncResult>
        abstract ResolveConflicts: ConflictStrategy -> Async<unit>
```

### Cross-App Communication

**Message Bus:**
```fsharp
type CheddarMessage =
    | LocationCaptured of Location
    | ExpenseAdded of Expense
    | SessionCompleted of SessionType * SessionData
    | DataExportRequested of ExportFormat
    | SettingsChanged of Settings

type IMessageBus =
    abstract Publish: CheddarMessage -> unit
    abstract Subscribe: (CheddarMessage -> unit) -> IDisposable
```

**Deep Linking:**
```
cheddar://laundrylog/session/new?location=35.1234,-101.5678
cheddar://perdiemlog/trip/new?start=2024-11-08
cheddar://books/expense/import?source=laundrylog&id=abc123
```

## Brand Identity

### Visual Identity

**Logo:** 🧀 (Cheese emoji as primary identifier)

**Color Palette:**
```css
--cheddar-orange: #FFB366;
--cheddar-yellow: #FFD93D;
--cheddar-cream: #FFF4E6;
--cheddar-brown: #8B4513;
--cheddar-text: #2C3E50;
```

**Typography:**
- Headers: Bold, friendly sans-serif
- Body: Readable system fonts
- Code: Monospace for data entry

### Voice and Tone

**Personality Traits:**
- Helpful but not condescending
- Professional but approachable
- Clear but not oversimplified
- Honest about limitations

**Example Messages:**
```
❌ Bad: "Oops! Something went wrong! 😱"
✅ Good: "GPS signal lost. Enter location manually?"

❌ Bad: "You're doing great! ⭐"
✅ Good: "Entry saved"

❌ Bad: "Invalid input, dummy!"
✅ Good: "Quantity must be 1-99"
```

## Community Building

### The Forum

Central hub for ecosystem:
- App documentation
- Feature discussions
- Community support
- Influence voting
- Path refinement
- Success stories

### Open Development

All development is public:
- Roadmap in forum
- Design discussions open
- Code reviews public
- Decision rationale documented

### Contributor Recognition

Ways to recognize contributors:
- Influence points (primary)
- Contributor badge in apps
- Credits in release notes
- Forum flair
- Annual contributor summit

## Growth Strategy

### Phase 1: Foundation (Current)
- Launch LaundryLog
- Build community
- Establish patterns
- Prove ecosystem concept

### Phase 2: Expansion (Months 3-6)
- Launch PerDiemLog
- Integrate apps
- Community database live
- Influence system active

### Phase 3: Maturation (Year 2)
- CheddarBooks beta
- Enterprise features
- API ecosystem
- Partner integrations

### Phase 4: Ecosystem (Year 3+)
- Full suite available
- Third-party apps
- Marketplace
- Certification program

## Success Metrics

### User Success
- Problems solved effectively
- Time saved daily
- Money saved annually
- Stress reduced

### Ecosystem Health
- Active contributors
- App integrations working
- Community engagement
- Influence participation

### Financial Sustainability
- Monthly recurring revenue
- Cost per user
- Contribution margin
- Reserve fund months

## Competitive Advantages

1. **Trucking-First Design** - Built by and for truckers
2. **Privacy Respected** - Local-first, no surveillance
3. **True Open Source** - AGPLv3, forever free
4. **Community Driven** - Users shape direction
5. **Integrated Suite** - Apps work better together
6. **Fair Business Model** - Transparent, ethical pricing

---

*The Cheddar ecosystem represents a new model for software: community-owned, privacy-respecting, and sustainably funded. By solving real problems for real people, we build software that matters.*