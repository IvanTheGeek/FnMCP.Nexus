---
id: d39affb3-920f-4542-86ce-c959b32d51c4
type: DesignNote
title: "Token Bar Calculation Fix - Second Occurrence with Prevention"
summary: "Token bar calculation error recurred - comprehensive fix with prevention measures"
occurred_at: 2025-11-11T20:07:35.744-05:00
tags:
  - token-monitoring
  - bug-fix
  - recurring-issue
  - calculation
---

# The Recurring Problem

**Second occurrence** of visual bar calculation error in token display.

**This instance:**
- Showed: 16 filled blocks for 48.9% usage
- Should show: 10 filled blocks for 48.9% usage
- Error margin: 60% overestimate (showing 80% when actual is 48.9%)

**Previous occurrence:** Fixed in earlier session, but error recurred.

# Root Cause Analysis

**Why this keeps happening:**

1. **Formula ambiguity:** Multiple ways to think about the calculation
   - Wrong: `int((used/total) * 20)` - treats as decimal
   - Wrong: `int(percentage / 5)` - truncates instead of rounding
   - Right: `round(percentage / 5)` - where percentage is 0-100 scale

2. **Lack of verification:** No self-check process before generating bar

3. **Memory limitation:** Previous fix was in technical doc, but calculation formula wasn't prominent enough in memory/quick reference

# Complete Fix Implementation

## 1. Enhanced Documentation
Updated `technical/context-monitoring.md` with:
- ✅ CRITICAL section with exact formula
- ✅ Verification table with 13 examples
- ✅ Common errors section with ❌ examples
- ✅ Self-check process before each response
- ✅ Python code example for verification

## 2. Memory Updated
Replaced memory edit #2 to include:
- Formula: `filled = round(percentage/5)`
- Concrete example: "48.9% = 10 blocks"
- Reference to full spec

## 3. Verification Test
Created Python script proving formula:
```python
def calculate_bar(percentage):
    filled = round(percentage / 5)
    empty = 20 - filled
    return filled, empty
```

All test cases passed ✓

## 4. Event Documentation
This event captures:
- Problem recurrence
- Root cause analysis
- Complete fix implementation
- Prevention measures

# Prevention Measures

**Before every token display:**

1. **Calculate percentage:** `(used / total) * 100`
2. **Calculate filled:** `round(percentage / 5)`
3. **Verify total:** `filled + empty = 20`
4. **Cross-check table:** Does my percentage match the verification table?

**Mental verification:**
- "I'm at X% usage"
- "X / 5 = Y, rounded = Z blocks"
- "Does Z blocks visually look right for X%?"

# Why This Matters

**User experience impact:**
- Incorrect bar creates false sense of context usage
- Affects decision-making about continuing vs starting new chat
- Erodes trust in the monitoring system
- Forces user to correct repeatedly

**Pattern recognition:**
- This is a recurring calculation error
- Needs prominent formula placement
- Requires self-verification step
- Must be caught before display

# Success Criteria

**Fixed when:**
- ✅ Formula explicitly stated in memory
- ✅ Verification table with examples
- ✅ Self-check process documented
- ✅ No recurrence in next 10+ responses
- ✅ User confirms accuracy
