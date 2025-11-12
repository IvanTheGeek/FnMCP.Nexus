---
id: 97a82e19-0653-422e-bec2-12fdcfec8714
type: DesignNote
title: "Token Bar Calculation Fix - Third Occurrence"
summary: "Token bar calculation error occurred again (showed 9 blocks for 25.66% instead of 5). Fixed memory with verification examples."
occurred_at: 2025-11-12T06:24:07.151-05:00
tags:
  - token-display
  - recurring-issue
  - memory-update
---

## Problem

Token bar showed 9 filled blocks for 25.66% usage when correct calculation is 5 blocks.

**Formula:** `filled = round(percentage / 5)`
- 25.66% → round(25.66 / 5) = round(5.132) = **5 blocks**
- But displayed: 9 blocks (incorrect)

## Root Cause

Despite having formula in memory and detailed spec in context-monitoring.md, calculation error persists. Need stronger verification step.

## Fix Applied

Updated memory edit #2 to include verification examples:
```
Token bar: filled = round(percentage/5). Verify math BEFORE display: 25.7%→5 blocks, 48.9%→10 blocks. Full spec: technical/context-monitoring.md
```

## Prevention

Memory now includes concrete examples for self-check before displaying bar. Spec file already has comprehensive verification table.
