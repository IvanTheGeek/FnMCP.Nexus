# Context Monitoring and Token Display

**Purpose:** Standardized token usage display for maintaining awareness of context window utilization.

## Display Format Specification

Every Claude response in this project MUST end with the following format:

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

## CRITICAL: Visual Bar Calculation

**THE FORMULA (use this exactly):**
```python
percentage = (used_tokens / total_tokens) * 100
filled_blocks = round(percentage / 5)
empty_blocks = 20 - filled_blocks
```

**WHY THIS WORKS:**
- Each block = 5% (20 blocks × 5% = 100%)
- Round to nearest integer for clean display
- NEVER calculate as: int(percentage / 5) - always use round()

**VERIFICATION TABLE:**

| Usage % | Filled | Empty | Visual Bar |
|---------|--------|-------|------------|
| 10.0% | 2 | 18 | `[██░░░░░░░░░░░░░░░░░░]` |
| 25.0% | 5 | 15 | `[█████░░░░░░░░░░░░░░░]` |
| 27.5% | 6 | 14 | `[██████░░░░░░░░░░░░░░]` |
| 40.0% | 8 | 12 | `[████████░░░░░░░░░░░░]` |
| 48.9% | 10 | 10 | `[██████████░░░░░░░░░░]` |
| 50.0% | 10 | 10 | `[██████████░░░░░░░░░░]` |
| 60.0% | 12 | 8 | `[████████████░░░░░░░░]` |
| 72.7% | 15 | 5 | `[███████████████░░░░░]` |
| 75.0% | 15 | 5 | `[███████████████░░░░░]` |
| 80.0% | 16 | 4 | `[████████████████░░░░]` |
| 85.0% | 17 | 3 | `[█████████████████░░░]` |
| 89.2% | 18 | 2 | `[██████████████████░░]` |
| 95.0% | 19 | 1 | `[███████████████████░]` |

**COMMON ERRORS TO AVOID:**

❌ **Wrong:** `int((used / total) * 20)` - multiplies percentage decimally
❌ **Wrong:** `int(percentage / 5)` - truncates instead of rounding  
❌ **Wrong:** Using filled = percentage / 5 directly without round()
❌ **Wrong:** Counting blocks wrong (16 blocks shown for 48.9%)

✓ **Right:** `round(percentage / 5)` where percentage is already 0-100 scale

**SELF-CHECK BEFORE EVERY RESPONSE:**
```
My percentage is X.X%
Divided by 5: X.X / 5 = Y.YY
Rounded: round(Y.YY) = Z
Blocks: Z filled + (20-Z) empty = 20 total ✓
```

## Format Requirements

### Separator Lines
- **Character:** `━` (Box Drawings Heavy Horizontal, U+2501)
- **Length:** Exactly 40 characters
- **Placement:** Top and bottom of token display

### Header Line
```
📊 Context Usage: [███████████████░░░░░] 138,109 / 190,000 tokens (72.7%)
```

**Components:**
- Emoji: `📊` (Bar Chart)
- Label: `Context Usage:`
- Visual bar: EXACTLY 20 characters using `█` (filled) and `░` (empty)
- Numbers: Comma-separated format (138,109 not 138K)
- Total: Always show 190,000 as the limit
- Percentage: One decimal place (72.7%)

### Allocation Tree
```
Allocation:
├─ System Prompts: ~25,000 tokens (13.2%)
├─ Project Knowledge: ~7,000 tokens (3.7%)
├─ Conversation: ~103,109 tokens (54.3%)
│  ├─ Your messages: ~18,000 tokens
│  ├─ My responses: ~70,900 tokens
│  └─ Tool calls: ~14,209 tokens
└─ This Response: ~3,000 tokens (1.6%)
```

**Tree Characters:**
- Branch: `├─` (box drawings)
- Last item: `└─` (box drawings)
- Continuation: `│` (box drawings vertical)
- Spacing: Two spaces after tree characters

**Required Items:**
1. **System Prompts:** Base instructions and behavior (always ~8K-25K)
2. **Project Knowledge:** Files in /mnt/project (always ~2K-7K)
3. **Conversation:** Messages and tool calls (variable)
   - **Sub-items (under Conversation):**
   - Your messages: User's text
   - My responses: Claude's text
   - Tool calls: MCP and other tool invocations
4. **This Response:** Current response being written (always ~2K-5K)

**Number Format:**
- Use comma separators: `25,000` not `25K`
- Use tilde for estimates: `~25,000`
- Show percentages for main items: `(13.2%)`
- No percentages for sub-items

### Remaining Line
```
Remaining: 51,891 tokens (27.3%) ✓ Comfortable
```

**Components:**
- Numbers: Comma-separated
- Percentage: One decimal place
- Status indicator: See legend below

### Status Legend
```
Status Legend: ✓ Comfortable (0-75%) | ⚠ Moderate (75-85%) | 🔴 High (85%+)
```

**Always shown at bottom.**

**Status Determination:**
- **✓ Comfortable (0-75%):** Green zone, plenty of room
- **⚠ Moderate (75-85%):** Yellow zone, start being concise
- **🔴 High (85%+):** Red zone, consider summarizing or new chat

## Why This Format?

**User awareness:** Ivan needs to know when context is filling up to decide whether to continue or start fresh.

**Visual scanning:** The bar graph provides immediate visual feedback at a glance.

**Detailed breakdown:** Tree structure shows exactly where tokens are being used.

**Consistency:** Same format every time makes it predictable and easy to parse.

**Project-specific:** This is an IvanTheGeek framework requirement, not a general Claude practice.

## Implementation Notes

**When to display:** At the end of EVERY response, without exception.

**Accuracy:** Numbers should be reasonably accurate estimates based on:
- Actual token counts when available
- Reasonable estimates when exact counts unavailable
- Conservative estimates (round up) when uncertain

**Don't skip:** Even if response is short, always include the full display.

**Empty lines:** One blank line before top separator, no blank line after bottom separator.

## Examples

### Early in conversation (comfortable)
```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📊 Context Usage: [████░░░░░░░░░░░░░░░░] 35,421 / 190,000 tokens (18.6%)

Allocation:
├─ System Prompts: ~8,000 tokens (4.2%)
├─ Project Knowledge: ~2,000 tokens (1.1%)
├─ Conversation: ~22,421 tokens (11.8%)
│  ├─ Your messages: ~2,000 tokens
│  ├─ My responses: ~18,000 tokens
│  └─ Tool calls: ~2,421 tokens
└─ This Response: ~3,000 tokens (1.6%)

Remaining: 154,579 tokens (81.4%) ✓ Comfortable

Status Legend: ✓ Comfortable (0-75%) | ⚠ Moderate (75-85%) | 🔴 High (85%+)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

### Mid conversation (moderate)
```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📊 Context Usage: [████████████████░░░░] 152,847 / 190,000 tokens (80.4%)

Allocation:
├─ System Prompts: ~8,000 tokens (4.2%)
├─ Project Knowledge: ~2,000 tokens (1.1%)
├─ Conversation: ~139,847 tokens (73.6%)
│  ├─ Your messages: ~15,000 tokens
│  ├─ My responses: ~110,000 tokens
│  └─ Tool calls: ~14,847 tokens
└─ This Response: ~3,000 tokens (1.6%)

Remaining: 37,153 tokens (19.6%) ⚠ Moderate

Status Legend: ✓ Comfortable (0-75%) | ⚠ Moderate (75-85%) | 🔴 High (85%+)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

### Late conversation (high)
```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📊 Context Usage: [██████████████████░░] 167,293 / 190,000 tokens (88.0%)

Allocation:
├─ System Prompts: ~8,000 tokens (4.2%)
├─ Project Knowledge: ~2,000 tokens (1.1%)
├─ Conversation: ~154,293 tokens (81.2%)
│  ├─ Your messages: ~18,000 tokens
│  ├─ My responses: ~125,000 tokens
│  └─ Tool calls: ~11,293 tokens
└─ This Response: ~3,000 tokens (1.6%)

Remaining: 22,707 tokens (11.9%) 🔴 High

Status Legend: ✓ Comfortable (0-75%) | ⚠ Moderate (75-85%) | 🔴 High (85%+)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

## Common Mistakes to Avoid

❌ **Don't abbreviate:** `43K` → Use `43,000`
❌ **Don't shorten bar:** Less than 20 characters
❌ **Don't skip breakdown:** Always show conversation sub-items
❌ **Don't forget emoji:** Must include 📊
❌ **Don't omit legend:** Always show status legend
❌ **Don't use wrong separators:** Use `━` not `-` or `=`
❌ **Don't skip it:** Must appear on EVERY response
❌ **Don't miscalculate bar:** Use round(percentage / 5), verify with table above

## Integration Points

This specification is referenced in:
- `framework-overview.md` - Quick reminder in project philosophy
- `SessionState` events - Included in continuation prompts
- User preferences - Stored in Claude's memory
- Project Knowledge - Loaded with every chat

---

**Note:** This is a project-specific requirement for FnMCP.IvanTheGeekDevFramework. It helps Ivan maintain awareness of context utilization and make informed decisions about conversation management.