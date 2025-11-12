---
id: 153876e6-3f0b-4250-8655-b36c8d2dedac
type: FrameworkInsight
title: "Bootstrap Pattern - Token Calculation Now Required Component"
summary: "Updated bootstrap pattern documentation to include token calculation formula as required component for all new project bootstrap files. Prevents recurring calculation errors."
occurred_at: 2025-11-12T06:42:44.169-05:00
tags:
  - bootstrap-pattern
  - token-display
  - documentation
  - standards
---

## What Was Done

Updated bootstrap pattern documentation to ensure token calculation formula is included in ALL bootstrap files for new projects.

## Changes Made

**1. Updated project-configuration-nexus.md:**
- Added "Bootstrap File Template" section explaining required components
- Emphasized token calculation as critical component
- Updated bootstrap example to include full calculation section
- Added verification step for token display in success checklist

**2. Updated project-creation-perdiem.md:**
- Added token calculation section to PerDiem bootstrap template
- Added verification step for token display in success checklist
- Added troubleshooting section for token calculation issues
- Updated migration checklist to include token calculation verification

**3. Bootstrap Template Standard:**
All bootstrap files now MUST include:
1. Project identification and scope
2. How to start conversations (prompts + manual)
3. Project structure and file paths
4. **Token calculation formula with verification examples**
5. Context management explanation
6. Event sourcing location

## Token Calculation Standard

Every bootstrap file includes:
```
**BAR CALCULATION (verify before displaying):**
percentage = (used / total) * 100
filled_blocks = round(percentage / 5)
empty_blocks = 20 - filled_blocks

**Quick verification:**
- 25.7% → round(25.7/5) = round(5.14) = 5 filled blocks
- 48.9% → round(48.9/5) = round(9.78) = 10 filled blocks
- 72.7% → round(72.7/5) = round(14.54) = 15 filled blocks
```

## Why This Matters

Token calculation errors occurred repeatedly in new chats because:
- Formula was in spec file (not always loaded)
- Formula was in memory (only loads in non-project chats)
- Formula was NOT in bootstrap file (which loads in EVERY project chat)

**Solution:** Add formula to bootstrap files so it's present from conversation start in ALL project chats.

## Prevention

When creating new projects:
1. Use updated bootstrap templates from project-creation guides
2. Verify token calculation section is present
3. Test token display in first message of new chat
4. Confirm calculation matches verification examples

## Coverage

- Project chats: Bootstrap file (this change)
- Non-project chats: User preferences (already updated)
- Both: Formula + examples present from start
