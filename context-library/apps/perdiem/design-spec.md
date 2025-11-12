# PerDiemLog Design Specification

## Status

**Phase 1 design not yet started.**

This document will contain detailed UI/UX specifications once design work begins, following the same iterative HTML mockup process used for LaundryLog v7.

## Design Approach

When design begins, follow these principles:

### Mobile-First
- Thumb-zone navigation (bottom 1/3 of screen)
- Large touch targets (minimum 44×44pt)
- Single-hand operation when possible
- Offline-capable for data entry on road

### Static-State Design
- Each screen = discrete state
- Clear navigation between states
- No ambiguous "in-between" states
- Predictable back navigation

### Modern & Polished
- LaundryLog-quality visual design
- Smooth transitions
- Clear visual hierarchy
- Professional appearance for tax documents

## Planned Screens

### 1. Dashboard/Home
- Current month summary card
- Quick "Add Trip" action
- Recent trips list (last 5-10)
- "Generate Report" action
- Month selector

### 2. Add/Edit Trip
- Date range picker (start/end)
- Location inputs (start/end)
- Notes field
- Real-time per diem preview
- Save/Cancel actions

### 3. Trip List
- Chronological list view
- Filter by date range
- Search by location
- Tap: View/edit trip
- Swipe: Delete with confirmation

### 4. Monthly Report View
- Month/year selector
- Trip list for selected month
- Toggle: List view / Calendar view
- Summary section (totals)
- Export/Share actions

### 5. Calendar View
- Full month grid
- Color-coded days (full/partial/none)
- Tap day: Show trip details
- Visual indicators for month-spanning trips

### 6. Settings
- User profile (name for reports)
- Current rates display
- Data management (backup/restore)
- About/help

## Design Deliverables (Future)

When design phase begins:
1. **HTML mockups** (v1, v2, v3... until satisfied)
2. **Screen flow diagrams** (path through states)
3. **Component specifications** (reusable elements)
4. **Interaction patterns** (gestures, transitions)
5. **Responsive breakpoints** (mobile, tablet, desktop)

## References

See LaundryLog design evolution for process examples:
- `apps/laundrylog/design-v7.md` - Final polished design
- `apps/laundrylog/design-iterations.md` - Evolution notes

## Next Steps

1. Complete Phase 1 requirements finalization
2. Begin HTML mockup iteration process
3. User testing with target drivers
4. Refinement based on feedback
5. Final specification for F# implementation

---

**Note:** This file will be substantially expanded once design work begins.
