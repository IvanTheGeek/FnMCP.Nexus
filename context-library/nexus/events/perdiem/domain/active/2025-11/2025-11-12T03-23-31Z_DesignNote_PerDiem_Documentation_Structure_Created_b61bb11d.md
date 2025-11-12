---
id: 2fbcab16-f4f5-413e-a0f5-0f8ba04b0d9e
type: DesignNote
title: "PerDiem Documentation Structure Created"
summary: "Established comprehensive documentation in apps/perdiem/ directory with requirements, event model, data model, and design placeholder"
occurred_at: 2025-11-11T22:23:31.051-05:00
tags:
  - documentation
  - project-setup
  - perdiem
---

Created complete PerDiem documentation structure in response to user testing new project setup:

**Files created:**
1. `apps/perdiem/overview.md` - Problem statement, target users, key business rules
2. `apps/perdiem/phase1-requirements.md` - Complete Phase 1 specifications including trip management, per diem calculations, report generation, UI requirements
3. `apps/perdiem/event-model.md` - Event Sourcing architecture with commands, events, read models, and event flows
4. `apps/perdiem/data-model.md` - F# type definitions, domain primitives, aggregates, validation
5. `apps/perdiem/design-spec.md` - Placeholder for future design work (not yet started)
6. `apps/perdiem/quick-reference.md` - Quick overview for new conversations

**Content captured from previous discussions:**
- Business rules: 2-364 day trip constraints, per diem rates, month boundary handling
- Three-phase roadmap: Manual entry → PDF download → PDF parsing
- IRS audit compliance requirements
- Mobile-first design principles
- Event Modeling methodology application

**Next steps:**
- User can now test PerDiem project with proper documentation structure
- Design work (HTML mockups) can begin using LaundryLog v7 iteration process as model
- F# implementation will follow from event model and data model specifications

