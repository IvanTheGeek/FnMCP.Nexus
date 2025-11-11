# Context Monitoring Pattern

**Framework:** FnMCP.IvanTheGeekDevFramework  
**Purpose:** Track token usage in Claude conversations  
**Updated:** 2025-01-15  
**Status:** Active Pattern

## The Pattern

Display token usage statistics at the end of every response to monitor context consumption and ensure efficient knowledge management.

## Implementation

### Visual Display Format
```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📊 Context Usage: [████████░░░░░░░░░░░░] 85,000 / 190,000 tokens (44.7%)

Allocation:
├─ System Prompts:     ~5,000 tokens  (2.6%)
├─ Project Knowledge:  ~2,000 tokens  (1.1%)  ← REDUCED from 15K
├─ Conversation:      ~75,000 tokens (39.5%)
└─ This Response:      ~3,000 tokens  (1.6%)

Remaining: 105,000 tokens (55.3%) ✓ Comfortable
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

### Visual Bar Calculation
```python
# 20-character bar representing percentage
filled = int(percentage / 5)  # Each block = 5%
empty = 20 - filled
bar = "█" * filled + "░" * empty
```

### Status Indicators
- **✓ Excellent headroom** (>60% remaining)
- **✓ Good headroom** (40-60% remaining)  
- **✓ Comfortable** (25-40% remaining)
- **⚠️ Getting tight** (10-25% remaining)
- **⚠️ Critical** (<10% remaining)

## Benefits

1. **Immediate awareness** of token consumption
2. **Proactive management** before hitting limits
3. **Optimization feedback** when testing changes
4. **Debugging aid** for unexpected token usage

## Integration

### Add to Quick Start
Include this note in framework overview:
```markdown
**Note to Claude:** Always show context usage stats with visual bar at the end of responses to monitor token consumption and ensure efficient knowledge management.
```

### Track Improvements
Document token savings when optimizing:
- Before: X tokens
- After: Y tokens  
- Savings: Z% reduction

## Example Scenarios

### Successful Optimization
```
Before Quick Start implementation:
├─ Project Knowledge: ~15,000 tokens (7.9%)

After Quick Start implementation:
├─ Project Knowledge:  ~2,000 tokens (1.1%)  ← 87% reduction!
```

### When to Worry
If Project Knowledge exceeds 10,000 tokens, investigate:
- Redundant files included?
- Old documentation not removed?
- Files too detailed for Quick Start?

---

*This pattern ensures continuous awareness of context usage, enabling proactive optimization and preventing context exhaustion.*