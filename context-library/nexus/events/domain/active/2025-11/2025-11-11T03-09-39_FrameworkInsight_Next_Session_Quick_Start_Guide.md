---
id: 53026c6f-5f51-4bf5-a2e5-42c0e400549f
type: FrameworkInsight
title: "Next Session Quick Start Guide"
occurred_at: 2025-11-11T03:09:39.396-05:00
tags:
  - quick-start
  - next-session
  - deployment
  - guide
---

## When Starting Next Session

### Step 1: Deploy Phase 3
```bash
cp /home/linux/RiderProjects/FnMCP.IvanTheGeek/bin/publish_single/nexus /home/linux/Nexus/nexus
```
Then restart Claude Code.

### Step 2: Verify Phase 3 Tools
Ask Claude: "List the available Nexus MCP tools"
Expect 7 tools (including record_learning, lookup_pattern, lookup_error_solution)

### Step 3: Test Learning System
- Ask: "lookup_pattern interpolated" - Should find interpolated-string-variable-extraction
- Ask: "lookup_error_solution FS3373" - Should show documented solution
- Write some F# code - Claude should reference patterns.md proactively

### Step 4: Read Knowledge Base
- Read: context-library/nexus/projections/knowledge/patterns.md
- Read: context-library/nexus/projections/knowledge/error-solutions.md
- Read: context-library/nexus/projections/.registry/registry.yaml

## Key Files to Know

**Phase 3 Binary:** `/home/linux/RiderProjects/FnMCP.IvanTheGeek/bin/publish_single/nexus` (71MB)
**Knowledge Projections:** `context-library/nexus/projections/knowledge/`
**Learning Events:** `context-library/nexus/events/learning/active/2025-11/`
**Project Source:** `/home/linux/RiderProjects/FnMCP.IvanTheGeek/src/FnMCP.IvanTheGeek/`

## Questions to Ask Next Session

1. "Read the Phase 3 completion events and summarize what was built"
2. "What F# patterns are currently in the knowledge base?"
3. "Show me the current metrics projection"
4. "What should we implement next for Nexus?"
