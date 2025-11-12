# Nexus Development Project

**MCP Server:** FnMCP.Nexus (connected to Claude Desktop)
**Project Scopes:** Nexus framework + FnMCP.Nexus server + LaundryLog dogfooding

## How to Start Any Conversation

### Quick Continuation
Click one of the MCP prompts:
- `continue-core` - Continue Nexus/framework work
- `continue-laundrylog` - Continue LaundryLog development
- `continue-nexus` - Continue FnMCP.Nexus server work

### Manual Continuation
1. **Check recent work:** Use `recent_chats(n=3)` to see latest discussions
2. **Load timeline:** Use `timeline_projection` to see recent events
3. **Load project docs:** Use Nexus MCP to read relevant documentation

## Project Structure (via MCP)

**Framework documentation:**
- `framework/overview.md` - Philosophy and methodology
- `framework/event-modeling-approach.md` - Core methodology
- `framework/mobile-first-principles.md` - Mobile design patterns
- `framework/paths-and-gwt.md` - Testing patterns

**Current applications:**
- `apps/laundrylog/` - Expense tracking (v7 design complete)
- `apps/perdiem/` - Per diem tracking (Phase 1 design)

**Technical references:**
- `technical/context-monitoring.md` - Token display format
- `technical/f-sharp-conventions.md` - F# patterns
- `technical/mcp-implementation.md` - MCP server details

**Quick Start files** (summarized versions):
- `quick-start/framework-overview.md`
- `quick-start/current-focus.md`
- `quick-start/naming-conventions.md`

## Token Usage Display Requirement

**CRITICAL:** Every response must end with detailed token usage display.

**BAR CALCULATION (verify before displaying):**
```
percentage = (used / total) * 100
filled_blocks = round(percentage / 5)
empty_blocks = 20 - filled_blocks
```

**Quick verification:**
- 25.7% → round(25.7/5) = round(5.14) = 5 filled blocks
- 48.9% → round(48.9/5) = round(9.78) = 10 filled blocks
- 72.7% → round(72.7/5) = round(14.54) = 15 filled blocks

**Format:** 📊 emoji, 20-char visual bar (█/░), comma-separated numbers, allocation tree with conversation breakdown, status legend.

See `technical/context-monitoring.md` for complete specification.

## Context Management

**Project Knowledge loads:** This single bootstrap file (~500 tokens)
**Everything else:** Load on-demand via Nexus MCP as needed
**Result:** ~95% token savings vs loading all docs

## Event Sourcing

All work is captured in events stored at:
- `events/core/` - Nexus framework events
- `events/laundrylog/` - LaundryLog app events
- `events/fnmcp-nexus/` - MCP server events

Use `timeline_projection` to see chronological event history.

---

**That's it!** When you need details, reference the file path and Claude will fetch it via MCP.
