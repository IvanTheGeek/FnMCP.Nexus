# Current Focus

## Primary: LaundryLog Implementation

**Status:** Design complete (v7), ready for development.

**Immediate tasks:**
- Implement F# domain model with discriminated unions for machine types and payment methods
- Build Bolero web application with Elmish architecture
- Create GPS location service with fallback to manual entry
- Implement session management with running totals
- Add data persistence using SQLite for local-first storage
- Deploy PWA for mobile testing

**Key features to implement:**
- One-tap machine type selection (washer/dryer)
- Large quantity adjustment circles (108px touch targets)
- Smart defaults with priority: Last → Historical → Community → System
- Visual validation showing missing requirements
- Session summary with individual entries and totals
- GPS location capture with truck stop name resolution

## Secondary: PerDiemLog Design

**Status:** Phase 1 requirements defined, design in progress.

**Current work:**
- Design mobile-first UI following LaundryLog patterns
- Define data model for DOT HOS compliance
- Create entry flow for manual per diem tracking
- Plan three-phase development approach

**Phase 1 scope (manual entry):**
- Track trips with start/end dates and locations
- Calculate IRS per diem rates automatically
- Support partial days and return trips
- Generate reports for tax filing

**Future phases:**
- Phase 2: Automated PDF downloads from IRS
- Phase 3: PDF parsing for rate extraction

## Infrastructure: Nexus Knowledge System

**Status:** ✅ COMPLETE - All documentation migrated!

**Completed achievements:**
- ✅ Created 23 modular documentation files
- ✅ Established two-tier knowledge architecture
- ✅ Reduced Project Knowledge from 15K to 2K tokens (88% reduction)
- ✅ MCP server (Nexus) serving detailed docs on-demand
- ✅ All framework, LaundryLog, and technical docs ready

**Nexus structure created:**
```
context-library/
├── quick-start/        # 3 files for Project Knowledge ✅
├── framework/          # 10 files (includes new additions) ✅
├── apps/laundrylog/    # 7 files complete specification ✅
└── technical/         # 6 files (includes new additions) ✅
```

## Next Milestones

**Week 1:**
- Begin LaundryLog F# implementation
- Set up Bolero project structure
- Implement core domain types
- Create basic UI components

**Week 2:**
- GPS integration and testing
- Session management working
- Local SQLite storage operational
- Basic PWA deployment

**Month 2:**
- LaundryLog beta release to truck drivers
- PerDiemLog Phase 1 implementation
- Forum solution selected and deployed
- Community feedback integration

## Framework Evolution

**Recently completed:**
- ✅ MCP server implementation in F#
- ✅ Two-tier knowledge architecture design
- ✅ LaundryLog v7 mobile-first design
- ✅ Read-on-request pattern for documentation
- ✅ Complete Nexus documentation system

**Currently exploring:**
- Forum software evaluation (Talkyard vs Discourse)
- Penpot API for automated path extraction
- EMapp prototype for Event Model visualization
- Community database architecture for shared data

## Development Philosophy

**Active principles:**
- Ship LaundryLog quickly to validate framework patterns
- Use real-world testing to refine mobile UX
- Build modular, reusable components
- Document everything for future contributors
- Maintain focus on truck driver needs

**Not doing yet:**
- Complex authentication systems
- Cloud synchronization
- Multi-user features
- Payment processing
- Advanced analytics

Focus remains on delivering core value: **helping truck drivers track laundry expenses with minimal friction.**