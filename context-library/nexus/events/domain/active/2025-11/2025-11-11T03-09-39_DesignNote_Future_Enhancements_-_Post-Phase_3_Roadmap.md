---
id: d63a6dbf-3aaa-4af2-aa86-b4792f273fd7
type: DesignNote
title: "Future Enhancements - Post-Phase 3 Roadmap"
occurred_at: 2025-11-11T03:09:39.397-05:00
tags:
  - roadmap
  - future
  - enhancements
  - phase4
---

## Immediate Priorities (Phase 3.5)

### 1. Expose Events as MCP Resources
**Goal:** Make events and projections directly readable via MCP
**Files to modify:** `Resources.fs`
**New resource URIs:**
- `event://domain/2025-11/EventName.md`
- `event://system/2025-11/EventType.yaml`
- `event://learning/2025-11/PatternName.md`
- `projection://timeline/evolution.md`
- `projection://metrics/statistics.yaml`
- `projection://knowledge/patterns.md`

**Benefits:** Faster access, better discovery, natural browsing

### 2. Auto-Learning Hook
**Goal:** Automatically emit learning events during coding
**Implementation:**
- Before compilation: Read patterns.md
- After error: Emit ErrorEncountered event
- After fix: Emit SolutionApplied event
- After success: Emit CompilationSuccess event
**Challenge:** Requires Claude Code integration or wrapper script

### 3. Pattern Validation
**Goal:** Track when patterns are used successfully
**Implementation:**
- Emit PatternValidated events when pattern works
- Increment use_count in knowledge projections
- Update confidence scores automatically

## Phase 4 Options

### Option A: Domain-Specific Knowledge (Easiest)
- Bolero framework patterns
- Event modeling patterns
- SAFE stack patterns
- Domain modeling patterns (already in docs)

### Option B: General Coding Knowledge (Medium)
- Architecture patterns (CQRS, Event Sourcing, DDD)
- API design principles
- Testing strategies
- Performance optimization patterns

### Option C: AI Pair Programming Assistant (Most Ambitious)
- Track user's coding preferences
- Learn from code reviews
- Suggest refactorings proactively
- Context-aware pattern recommendations
- Integration with enhance_nexus workflow

## Technical Debt

1. **Resource exposure:** Not yet implemented
2. **Projection staleness:** Not automatically refreshed
3. **Pattern confidence:** Not automatically updated
4. **Error correlation:** No automatic linking of errors to patterns
5. **Learning event tags:** Could be more structured/hierarchical

## Metrics to Track

- Pattern application rate (how often patterns are used)
- Error recurrence rate (do we repeat mistakes?)
- First-try compilation rate (improving over time?)
- Knowledge base growth rate (events per session)
- Tool usage patterns (which tools most valuable?)
