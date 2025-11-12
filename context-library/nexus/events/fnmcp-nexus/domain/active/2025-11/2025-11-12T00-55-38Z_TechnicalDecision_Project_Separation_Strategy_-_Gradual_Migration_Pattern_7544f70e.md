---
id: 009b3ddf-fd92-426e-93fe-2f0d22a5c4a3
type: TechnicalDecision
title: "Project Separation Strategy - Gradual Migration Pattern"
summary: "Strategy for gradual project separation - keep active dev together, test with PerDiem"
occurred_at: 2025-11-11T19:55:38.709-05:00
tags:
  - project-structure
  - strategy
  - dogfooding
  - transition
technical_decision:
---

# Rationale

During initial Nexus development, keeping related work in one project enables:
- **Tight feedback loops** between framework and LaundryLog dogfooding
- **Faster iteration** on FnMCP.Nexus server development
- **Coherent learning** captured in same conversation context
- **Reduced context switching** during rapid prototyping

Once patterns stabilize, apps can graduate to their own projects.

# Strategy

**Phase 1: Current State (Now)**
```
Nexus Project (this project):
├─ Nexus framework development
├─ FnMCP.Nexus server development
└─ LaundryLog dogfooding/learning

(Heavy development, tight coupling beneficial)
```

**Phase 2: Test Pattern (Next)**
```
Nexus Project:
├─ Nexus framework
├─ FnMCP.Nexus server
└─ LaundryLog dogfooding

PerDiemLog Project (NEW):
└─ PerDiem development
(Test multi-project pattern, lighter development)
```

**Phase 3: Full Separation (Future)**
```
Each app in own project when:
- Nexus patterns stabilized
- Bootstrap process proven
- Heavy development phase complete
```

# Decision

1. **Keep in Nexus project:** FnMCP.Nexus + LaundryLog (active development)
2. **Split to test pattern:** PerDiemLog → separate project
3. **Update Nexus bootstrap:** Minimal file covering all three scopes
4. **Document transition:** Create guide for moving apps to own projects

# Benefits

- ✅ Maintains velocity on active development
- ✅ Tests multi-project pattern with low-risk app
- ✅ Documents graduation process for future apps
- ✅ Allows pattern refinement before major migration
