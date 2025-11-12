---
id: 7f912d69-cc34-495a-805a-0b2ab5594bd4
type: DesignNote
title: "Token Display Format Specification Corrected"
summary: "Token display format specification corrected - full detailed format required"
occurred_at: 2025-11-11T19:31:04.914-05:00
tags:
  - token-monitoring
  - format
  - user-preference
  - correction
---

## Correct Token Display Format

**Full specification:**

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📊 Context Usage: [███████████████░░░░░] 138,109 / 190,000 tokens (72.7%)

Allocation:
├─ System Prompts: ~25,000 tokens (13.2%)
├─ Project Knowledge: ~7,000 tokens (3.7%)
├─ Conversation: ~103,109 tokens (54.3%)
│  ├─ Your messages: ~18,000 tokens
│  ├─ My responses: ~70,900 tokens
│  └─ Tool calls: ~14,209 tokens
└─ This Response: ~3,000 tokens (1.6%)

Remaining: 51,891 tokens (27.3%) ✓ Comfortable

Status Legend: ✓ Comfortable (0-75%) | ⚠ Moderate (75-85%) | 🔴 High (85%+)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

## Key Requirements

1. **Separator:** 40 chars: `━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`
2. **Header:** `📊 Context Usage:` with bar on same line
3. **Numbers:** Use comma separators (138,109 not 138K)
4. **Percentages:** Show for all allocation items
5. **Conversation breakdown:** Sub-tree with Your/My/Tool breakdown
6. **Status:** Reference legend (Comfortable/Moderate/High)
7. **Legend:** Always show at bottom
8. **Format:** Proper tree structure with │ ├─ └─

## Incorrect Format Previously Used

Was showing abbreviated format without:
- 📊 emoji
- Comma-separated numbers
- Percentages on allocations
- Conversation sub-breakdown
- Status legend
- Proper separator length
