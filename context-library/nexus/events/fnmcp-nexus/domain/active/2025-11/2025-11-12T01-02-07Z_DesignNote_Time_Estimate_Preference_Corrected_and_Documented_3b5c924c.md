---
id: 92bf9679-0b7f-4605-9381-142581da3923
type: DesignNote
title: "Time Estimate Preference Corrected and Documented"
summary: "Time estimate preference violation corrected - broadened to all contexts"
occurred_at: 2025-11-11T20:02:07.856-05:00
tags:
  - preference
  - correction
  - user-experience
  - documentation
---

# The Error

I included time estimates in the project configuration guides:
- "Time estimate: 15-20 minutes"
- "Time estimate: 20-30 minutes"

This violated the user's established preference about time estimates.

# The Preference

**Original wording (too narrow):**
"Do not include time estimates (hours, weeks, days) for research and planning requests."

**Issue:** Only applied to "research and planning requests" - but user wants NO time estimates in ANY context.

**Reason:** User works as opportunity allows (truck driver schedule). Time estimates don't align with workflow. Focus should be on phases and progression, not duration.

# Corrections Made

## 1. Updated Memory
**Old:** "Do not include time estimates (hours, weeks, days) for research and planning requests."

**New:** "Never include time estimates (hours, minutes, days, weeks, months) in any responses. User works as opportunity allows (truck driver). Focus on phases, steps, and progression instead of duration."

**Changes:**
- "Do not" → "Never" (stronger)
- Added "minutes" and "months" for completeness
- "for research and planning requests" → "in any responses" (broader scope)
- Clarified alternative approach

## 2. Updated Framework Documentation
Added prominent note to `framework-overview.md`:
```markdown
> **IMPORTANT:** Never include time estimates (hours, minutes, days, weeks, months) in any responses. User works as opportunity allows (truck driver). Focus on phases, steps, and progression instead of duration.
```

Placed at top of file with token display requirement for visibility.

# Future Reminders

**Instead of:** "Time estimate: 15-20 minutes"
**Say:** "This involves 10 steps" or "Phase 1 includes these tasks"

**Instead of:** "This will take 2 weeks"
**Say:** "This phase includes..." or "Next milestone"

**Instead of:** "Should take about an hour"
**Say:** "When ready to proceed..." or "At your convenience"

# Why This Matters

Time estimates create false expectations when:
- Work happens in scattered pockets of availability
- Priorities shift based on road conditions
- Development happens between driving shifts
- Real-world constraints aren't predictable

Focusing on phases/steps respects the user's actual workflow pattern.
