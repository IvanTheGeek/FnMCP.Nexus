# LaundryLog Overview

**App:** LaundryLog  
**Part of:** Cheddar Ecosystem  
**Status:** v7 Design Complete, Implementation Starting  
**Platform:** Mobile-first PWA  
**License:** AGPLv3  

## Problem Statement

### The Core Problem

Truck stop laundry machines don't provide receipts. This costs drivers hundreds or thousands in lost tax deductions annually because:
- The IRS requires documentation for business expense deductions
- Laundry is a legitimate business expense for truckers
- Without receipts, drivers can't prove these expenses
- Generic expense apps aren't designed for trucking workflows

### Current Workarounds Don't Work

**Manual logbooks:** Time-consuming, often forgotten, easily lost  
**Credit card statements:** Show wrong location (company HQ, not truck stop)  
**Phone photos:** No structured data, hard to organize for taxes  
**Generic expense apps:** Too much typing, not designed for truck stops

## The Solution

LaundryLog is a mobile-first expense tracker specifically designed for truck drivers doing laundry at truck stops. It prioritizes:

1. **Minimal interaction** - 95% button-based, no typing
2. **GPS location capture** - Proves presence for IRS
3. **Smart defaults** - Learns from usage patterns
4. **Session-based tracking** - Matches how laundry is actually done
5. **Community intelligence** - Shared price data helps everyone

## Key Features

### Core Functionality
- **One-tap machine selection** (Washer/Dryer buttons)
- **Large quantity adjusters** (108px circular +/- buttons)
- **Multiple payment methods** (Quarters, Card, Points, Cash)
- **GPS auto-capture** with manual fallback
- **Session management** with running totals
- **IRS-compliant receipts** generated automatically

### Smart Defaults System
Priority hierarchy for suggesting values:
1. **Last used** in current session
2. **Historical** at this location
3. **Community** average at this truck stop
4. **System** reasonable defaults

### Visual Validation
- Proactive error prevention
- Clear requirement indicators
- Disabled states when incomplete
- Visual feedback for all actions

## Target Users

### Primary: Long-Haul Truckers
- On the road for weeks at a time
- Do laundry at multiple truck stops
- Need expense documentation for taxes
- Limited time at each stop

### Secondary: Regional Drivers
- Regular routes with familiar stops
- Weekly laundry routine
- Company reimbursement needs
- Consistent locations

### Tertiary: Owner-Operators
- Every expense matters
- Detailed record-keeping required
- Tax optimization critical
- Business expense tracking

## Design Evolution

**v1-v3:** Learning mobile constraints  
**v4-v5:** Discovering smart defaults  
**v6-v7:** Perfecting the experience

Seven iterations taught us:
- Buttons must be 72px minimum
- Truck drivers have limited time
- Typing should be avoided
- Visual validation prevents errors
- Community data adds value

## Technical Architecture

### Frontend
- **Framework:** F# with Bolero
- **Architecture:** Elmish (Model-View-Update)
- **Deployment:** Progressive Web App
- **Offline:** Service worker for offline support

### Backend
- **Storage:** SQLite local-first
- **Sync:** Optional cloud backup
- **Community:** Opt-in data sharing
- **Privacy:** No tracking or analytics

### Integration Points
- **GPS Services:** For location capture
- **Camera API:** For receipt photos (future)
- **Export:** CSV, PDF, QuickBooks formats
- **Cheddar Ecosystem:** Shared location data

## Current Status

### ✅ Completed
- Mobile-first v7 design
- HTML prototype
- Event Model definition
- Data model specification
- User flow documentation
- Community feedback integration

### 🚧 In Progress
- F# domain model implementation
- Bolero component development
- GPS service integration
- SQLite persistence layer

### 📋 Upcoming
- PWA deployment
- Beta testing with drivers
- Community database setup
- Export functionality
- PerDiemLog integration

## Success Metrics

### User Success
- **Time per entry:** < 30 seconds
- **Sessions completed:** > 90%
- **Location captured:** > 95%
- **User retention:** > 80% monthly

### Technical Success
- **Offline capability:** 100%
- **Load time:** < 2 seconds
- **Touch target accuracy:** > 98%
- **Data sync success:** > 99%

### Business Success
- **Tax deductions captured:** Average $500/year per user
- **Time saved:** 5 minutes per session
- **Community participation:** > 30% opt-in
- **User satisfaction:** > 4.5 stars

## Differentiation

### vs. Generic Expense Apps
- **Designed for truck stops** not offices
- **Button-based** not keyboard-heavy
- **Session-focused** not item-by-item
- **Community prices** not manual entry

### vs. Manual Tracking
- **Automatic location** not written addresses
- **Digital receipts** not paper
- **Structured data** not free-form notes
- **Export-ready** not manual transcription

### vs. Nothing
- **Save hundreds in taxes** vs losing deductions
- **Prove expenses** vs IRS problems
- **Track spending** vs guessing
- **Professional records** vs amateur logs

## Development Principles

### User-Centered
- Built by a truck driver for truck drivers
- Tested at actual truck stops
- Feedback integrated continuously
- Real problems solved

### Privacy-First
- Local data storage
- Optional sync only
- No behavior tracking
- User owns their data

### Community-Driven
- Open source forever
- Influence system for features
- Shared knowledge benefits all
- Transparent development

## Future Enhancements

### Phase 2: Intelligence
- Price prediction by location
- Spending analytics
- Automatic categorization
- Smart notifications

### Phase 3: Integration
- PerDiemLog connection
- CheddarBooks sync
- Fleet management features
- Company reimbursement tools

### Phase 4: Expansion
- Other truck stop expenses
- Shower tracking
- Parking fees
- Maintenance records

## Get Involved

### For Users
- Beta test the app
- Provide feedback
- Share price data
- Report issues

### For Developers
- Review the code
- Submit pull requests
- Improve documentation
- Add tests

### For Supporters
- Sponsor development
- Spread the word
- Help other users
- Vote on features

---

*LaundryLog solves a real problem that costs truck drivers real money. By focusing on this specific need with excellent UX, we create genuine value for hardworking people.*