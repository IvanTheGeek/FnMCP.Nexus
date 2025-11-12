---
id: 0c332dfc-6f22-4c74-8348-b425dc868148
type: DesignNote
title: "Token Bar Calculation - Comprehensive Fix via Dual Coverage"
summary: "Comprehensive fix for recurring token bar calculation errors: added formula to both bootstrap file (project chats) and user preferences (all chats) for automatic loading."
occurred_at: 2025-11-12T06:34:00.053-05:00
tags:
  - token-display
  - recurring-issue
  - comprehensive-fix
  - bootstrap
  - user-preferences
---

## Problem Analysis

Token bar calculation has failed repeatedly (more than 3 occurrences) across multiple new chats. Pattern:
- Spec file existed: Still failed
- Memory edit added: Still failed  
- Verification examples added: Still failed

**Root cause:** Information was available but not in the right places to load automatically in every new chat.

## Solution: Dual Coverage Strategy

**For project chats:**
- Updated `nexus-bootstrap.md` with calculation formula + verification examples
- Bootstrap loads automatically in every project chat (~600 tokens total)

**For non-project chats:**
- Updated user preferences (memory edit #2) with calculation formula + verification examples
- User preferences load automatically in every chat

## Implementation

**Bootstrap addition:**
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

**Memory edit:**
```
EVERY response ends with token bar. Calculate: percentage=(used/total)*100, filled=round(percentage/5). Verify: 25.7%→5, 48.9%→10, 72.7%→15. Format: 📊 [20 chars █/░], numbers, tree, legend.
```

## Why This Works

- Bootstrap file loads in EVERY project chat (already happening)
- User preferences load in EVERY chat (project or not)
- Formula + examples present from conversation start
- No additional token cost (bootstrap already loads)
- Self-contained verification examples eliminate need to reference spec

## Prevention

This should finally solve the recurring issue by ensuring the calculation formula is always present in the initial context, regardless of chat type.
