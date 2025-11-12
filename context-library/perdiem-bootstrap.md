# PerDiemLog Development Project

**MCP Server:** FnMCP.Nexus (connected to Claude Desktop)
**Project Scope:** PerDiem per diem expense tracking application
**Status:** Phase 1 design (manual entry focus)

## How to Start Any Conversation

### Quick Continuation
Click the MCP prompt:
- `continue-perdiem` - Continue PerDiemLog development

### Manual Continuation
1. **Check recent work:** Use `recent_chats(n=3)` to see latest discussions
2. **Load timeline:** Use `timeline_projection` filtered to "perdiem" project
3. **Load project docs:** Use Nexus MCP to read `apps/perdiem/` documentation

## Project Documentation (via MCP)

**Application documentation:**
- `apps/perdiem/overview.md` - Problem statement and solution
- `apps/perdiem/phase1-requirements.md` - Manual entry phase specs
- `apps/perdiem/event-model.md` - Commands, events, views
- `apps/perdiem/data-model.md` - Data structures and types
- `apps/perdiem/design-spec.md` - UI design specification (when created)

**Framework references:**
- `framework/overview.md` - Core philosophy and methodology
- `framework/event-modeling-approach.md` - Event Modeling patterns
- `framework/mobile-first-principles.md` - Mobile design guidance
- `framework/static-state-design.md` - Screen state patterns

**Technical references:**
- `technical/context-monitoring.md` - Token display format
- `technical/f-sharp-conventions.md` - F# coding patterns
- `technical/bolero-patterns.md` - Bolero web framework usage

## Current Focus

**Phase 1: Manual Entry**
- Track trips with start/end dates and locations
- Calculate IRS per diem rates automatically  
- Support partial days and return trips
- Generate reports for tax filing

**Not in Phase 1:**
- Automated PDF downloads from IRS (Phase 2)
- PDF parsing for rate extraction (Phase 3)

## Token Usage Display Requirement

**CRITICAL:** Every response must end with detailed token usage display.

**BAR CALCULATION (verify before displaying):**
```
percentage = (used / total) * 100
filled_blocks = round(percentage / 5)
empty_blocks = 20 - filled_blocks
```

**Quick verification:**
- 25.7% → round(25.7/5) = round(5.14) = 5 filled blocks
- 48.9% → round(48.9/5) = round(9.78) = 10 filled blocks
- 72.7% → round(72.7/5) = round(14.54) = 15 filled blocks

**Format:** 📊 emoji, 20-char visual bar (█/░), comma-separated numbers, allocation tree with conversation breakdown, status legend.

See `technical/context-monitoring.md` for complete specification.

## Context Management

**Project Knowledge loads:** This single bootstrap file (~500 tokens)
**Everything else:** Load on-demand via Nexus MCP as needed
**Result:** Maximum conversation capacity for development work

## Event Sourcing

PerDiem work captured in events stored at:
- `events/perdiem/` - All PerDiemLog development events

Use `timeline_projection` with project filter to see PerDiem-specific history.

## Related Work

**Cheddar Ecosystem:**
- LaundryLog - Laundry expense tracking (sister app)
- CheddarBooks - Full accounting system (future)

**Shared patterns:**
- Mobile-first design
- Event sourcing architecture
- F# + Bolero technology stack
- Penpot for design source of truth

---

**That's it!** When you need details, reference the file path and Claude will fetch it via MCP.
