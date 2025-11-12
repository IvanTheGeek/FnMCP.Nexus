# PerDiemLog Quick Reference

## What is PerDiemLog?

Per diem expense tracking app for truck drivers subject to US DOT HOS regulations. Helps calculate IRS per diem deductions and generates audit-ready reports.

## Current Status

**Phase 1:** Requirements documented, design pending  
**Next:** Begin HTML mockup iteration for UI design

## Key Files

- `overview.md` - Problem statement, target users, business rules
- `phase1-requirements.md` - Complete Phase 1 feature specifications
- `event-model.md` - Commands, events, read models (Event Sourcing)
- `data-model.md` - F# type definitions and domain model
- `design-spec.md` - UI/UX specifications (placeholder - design not started)

## Core Business Rules

**Trip Duration:**
- Minimum: 2 days (can't leave/return same day)
- Maximum: 364 days (must maintain tax home)

**Per Diem Rates (2023 example):**
- Full day: $69 (100%)
- Partial day: $51.75 (75% - departure/return days)

**Month Boundaries:**
- Trips commonly span multiple months
- Reports show only days within reporting month
- Display indicates trip continuation beyond boundaries

## Phase Roadmap

**Phase 1:** Manual entry, automatic calculation, modern reports  
**Phase 2:** Automated PDF download from HOS log website  
**Phase 3:** PDF parsing to auto-populate trip data

**Never in scope:** Direct ELD device integration

## Quick Start for New Chats

```
Use MCP prompt: continue-perdiem
Or manually: recent_chats(n=3) + timeline_projection
```

## Related Documentation

**Framework:**
- `framework/overview.md` - Development philosophy
- `framework/event-modeling-approach.md` - Event Modeling methodology
- `framework/mobile-first-principles.md` - Mobile design patterns

**Technical:**
- `technical/f-sharp-conventions.md` - F# coding standards
- `technical/bolero-patterns.md` - Bolero web framework usage
