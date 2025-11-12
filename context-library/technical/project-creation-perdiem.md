# Creating PerDiemLog Project Guide

**Purpose:** Step-by-step instructions for creating a separate Claude Project for PerDiemLog.

**Goal:** Test the multi-project pattern by moving PerDiem development to its own project while sharing the Nexus MCP server.

**Prerequisites:**
- Nexus project updated with bootstrap file (see `technical/project-configuration-nexus.md`)
- FnMCP.Nexus server running and connected
- PerDiem documentation exists in `apps/perdiem/` in context-library

---

## Step 1: Create New Claude Project

**Action:** Create a new project in Claude for PerDiemLog.

**Steps:**

**In Claude Desktop:**
1. Look for "Projects" in the sidebar or navigation
2. Click "+ New Project" or similar button
3. Name the project: `PerDiemLog`
4. (Optional) Add description: "Per diem expense tracking for truck drivers - DOT HOS compliant"

**In Claude Web:**
1. Navigate to Projects section
2. Click "Create new project"
3. Name: `PerDiemLog`
4. Confirm creation

**Result:** Empty PerDiemLog project created, no knowledge files yet.

---

## Step 2: Create PerDiem Bootstrap File

**Action:** Create `perdiem-bootstrap.md` with PerDiem-specific configuration.

**Content:**

```markdown
# PerDiemLog Development Project

**MCP Server:** FnMCP.Nexus (connected to Claude Desktop)
**Project Scope:** PerDiem per diem expense tracking application
**Status:** Phase 1 design (manual entry focus)

## How to Start Any Conversation

### Quick Continuation
Click the MCP prompt:
- `continue-perdiem` - Continue PerDiemLog development

### Manual Continuation
1. **Check recent work:** Use `recent_chats(n=3)` to see latest discussions
2. **Load timeline:** Use `timeline_projection` filtered to "perdiem" project
3. **Load project docs:** Use Nexus MCP to read `apps/perdiem/` documentation

## Project Documentation (via MCP)

**Application documentation:**
- `apps/perdiem/overview.md` - Problem statement and solution
- `apps/perdiem/phase1-requirements.md` - Manual entry phase specs
- `apps/perdiem/event-model.md` - Commands, events, views
- `apps/perdiem/data-model.md` - Data structures and types
- `apps/perdiem/design-spec.md` - UI design specification (when created)

**Framework references:**
- `framework/overview.md` - Core philosophy and methodology
- `framework/event-modeling-approach.md` - Event Modeling patterns
- `framework/mobile-first-principles.md` - Mobile design guidance
- `framework/static-state-design.md` - Screen state patterns

**Technical references:**
- `technical/context-monitoring.md` - Token display format
- `technical/f-sharp-conventions.md` - F# coding patterns
- `technical/bolero-patterns.md` - Bolero web framework usage

## Current Focus

**Phase 1: Manual Entry**
- Track trips with start/end dates and locations
- Calculate IRS per diem rates automatically  
- Support partial days and return trips
- Generate reports for tax filing

**Not in Phase 1:**
- Automated PDF downloads from IRS (Phase 2)
- PDF parsing for rate extraction (Phase 3)

## Token Usage Display Requirement

**CRITICAL:** Every response must end with detailed token usage display.

Format: 📊 emoji, 20-char visual bar (█/░), comma-separated numbers, allocation tree with conversation breakdown, status legend.

See `technical/context-monitoring.md` for complete specification.

## Context Management

**Project Knowledge loads:** This single bootstrap file (~500 tokens)
**Everything else:** Load on-demand via Nexus MCP as needed
**Result:** Maximum conversation capacity for development work

## Event Sourcing

PerDiem work captured in events stored at:
- `events/perdiem/` - All PerDiemLog development events

Use `timeline_projection` with project filter to see PerDiem-specific history.

## Related Work

**Cheddar Ecosystem:**
- LaundryLog - Laundry expense tracking (sister app)
- CheddarBooks - Full accounting system (future)

**Shared patterns:**
- Mobile-first design
- Event sourcing architecture
- F# + Bolero technology stack
- Penpot for design source of truth

---

**That's it!** When you need details, reference the file path and Claude will fetch it via MCP.
```

**Save this file** to your computer for upload.

---

## Step 3: Add MCP Server to PerDiem Project

**Action:** Ensure FnMCP.Nexus server is available to the new project.

**Important:** MCP servers configured in Claude Desktop are **globally available** to all projects. You don't need to reconfigure anything!

**Verification:**
1. Open PerDiemLog project
2. Check MCP servers section in settings
3. Should see `FnMCP.Nexus` already listed

**If NOT visible:**
- MCP configuration is in Claude Desktop settings (not project-specific)
- Should already be configured from Nexus project
- If missing, check `claude_desktop_config.json` and restart Claude Desktop

**Expected:** Server automatically available without additional configuration.

---

## Step 4: Upload Bootstrap to PerDiem Project

**Action:** Add the bootstrap file as Project Knowledge for PerDiem.

**Steps:**
1. Open PerDiemLog project settings
2. Navigate to "Project knowledge" section
3. Click "Add file" or "Upload"
4. Select `perdiem-bootstrap.md` file
5. Upload and confirm
6. Verify file appears in Project Knowledge

**Result:** PerDiem project has minimal bootstrap file (~500 tokens).

---

## Step 5: Verify PerDiem Documentation Exists

**Action:** Confirm PerDiem docs are in context-library served by MCP.

**Check these files exist:**
```bash
# In your Nexus repository, verify:
apps/perdiem/overview.md
apps/perdiem/phase1-requirements.md
apps/perdiem/event-model.md
apps/perdiem/data-model.md
```

**If files missing:**
- Create them using Nexus MCP `update_documentation` tool
- Or work in Nexus project first to build out PerDiem docs
- Then return to PerDiem project once ready

**Note:** It's okay if design-spec.md doesn't exist yet - it's mentioned as "(when created)" in bootstrap.

---

## Step 6: Test New PerDiem Chat

**Action:** Start a chat in PerDiemLog project to verify setup.

**Test steps:**
1. Open PerDiemLog project
2. Click "New chat"
3. Type: "What is PerDiemLog and what's the current status?"

**Expected Claude behavior:**
```
Claude: "Let me check the PerDiem documentation..."
[Uses MCP to read apps/perdiem/overview.md]
Claude: "PerDiemLog is a per diem expense tracking app for truck drivers..."
[Explains Phase 1 requirements, current focus]
```

**Success indicators:**
- Claude accesses MCP docs automatically
- References correct PerDiem files
- Doesn't confuse with LaundryLog content
- Project Knowledge tokens low (~500)

---

## Step 7: Test Timeline Filtering

**Action:** Verify timeline shows only PerDiem events (once they exist).

**Test steps:**
1. In PerDiem project chat, ask: "Show me the PerDiem development timeline"
2. Claude should call `timeline_projection`
3. Should filter to `perdiem` project events only

**Expected:**
- If no PerDiem events yet: "No PerDiem events found"
- If events exist: Shows only PerDiem-related events
- Should NOT show Nexus framework or LaundryLog events

**To create first event:**
```
Create a SessionState event:
- project: perdiem
- title: "PerDiem Project Initialized"
- status: active
- current_task: "Setting up PerDiem project structure"
```

---

## Step 8: Test Continuation Prompt

**Action:** Verify `continue-perdiem` prompt works.

**Test steps:**
1. Create a SessionState event for PerDiem (if not done in Step 7)
2. Start a new chat in PerDiemLog project
3. Click `continue-perdiem` prompt (or type the keyword)

**Expected:**
```
[MCP prompt queries latest perdiem SessionState event]
Claude: "You're working on PerDiem development..."
[Provides continuation context]
```

**If prompt not visible:**
- Prompts are defined in MCP server code
- Check server includes perdiem in prompt list
- May need to update server to add perdiem prompt

---

## Step 9: Verify Token Efficiency

**Action:** Compare token usage between projects.

**In PerDiem project chat:**
```
📊 Context Usage: [...] X,XXX / 190,000 tokens (X.X%)

Allocation:
├─ System Prompts: ~8,000 tokens (4.2%)
├─ Project Knowledge: ~500 tokens (0.3%)  ← Minimal bootstrap only
├─ Conversation: ~X,XXX tokens
```

**Compare to if using Nexus project:**
- Nexus loads: core + laundrylog + perdiem + nexus docs
- PerDiem project loads: core + perdiem only
- **Savings: ~4,000 tokens** (only loading relevant project context)

---

## Step 10: Cross-Project Verification

**Action:** Verify projects are properly isolated.

**Test:**
1. Ask PerDiem chat: "What's LaundryLog's current status?"
2. Claude should:
   - NOT have LaundryLog context automatically
   - Can access via MCP if you ask it to read LaundryLog docs
   - Naturally focuses on PerDiem work

**Expected:** Projects maintain separate conversation focus, but can access shared documentation when needed.

---

## Step 11: Document Success

**Action:** Create events documenting the new project setup.

**In PerDiem project chat:**
```
Create a FrameworkInsight event:
- Title: "PerDiem Project Successfully Separated"
- Document: 
  - New project created with minimal bootstrap
  - Token efficiency achieved
  - MCP integration working
  - Continuation prompts functional
  - Pattern validated for future app projects
```

**Then commit in Nexus repository:**
```bash
cd /home/linux/RiderProjects/FnMCP.Nexus/nexus
git add events/perdiem/
git commit -m "feat: Create PerDiem separate project with bootstrap"
```

---

## Step 12: Create Migration Guide

**Action:** Document what you learned for future app migrations.

**Create checklist for moving apps to own projects:**
1. ✅ Create new Claude Project
2. ✅ Create project-specific bootstrap file
3. ✅ Upload bootstrap as Project Knowledge
4. ✅ Verify MCP server accessible
5. ✅ Test document access via MCP
6. ✅ Create initial SessionState event
7. ✅ Test continuation prompts
8. ✅ Verify token efficiency
9. ✅ Confirm project isolation
10. ✅ Document in events

**Save this checklist** for when you graduate LaundryLog to its own project.

---

## Success Checklist

- [ ] PerDiemLog project created in Claude
- [ ] perdiem-bootstrap.md file created
- [ ] Bootstrap uploaded to PerDiem Project Knowledge
- [ ] MCP server accessible in PerDiem project
- [ ] PerDiem documentation verified in context-library
- [ ] New chat test successful
- [ ] Timeline filtering working
- [ ] Continuation prompt working  
- [ ] Token efficiency confirmed
- [ ] Cross-project isolation verified
- [ ] Success events created
- [ ] Changes committed to git
- [ ] Migration pattern documented

---

## Next Steps After Success

**Once PerDiem project working:**

1. **Use PerDiem project** for all PerDiem development work
2. **Keep Nexus project** for framework and LaundryLog work
3. **Document lessons learned** about multi-project workflow
4. **Refine bootstrap template** based on real usage
5. **Plan LaundryLog graduation** when heavy development phase complete

**When to graduate LaundryLog:**
- ✅ Nexus patterns stabilized
- ✅ v7 implementation complete and tested
- ✅ Less frequent framework changes
- ✅ Ready for focused app development

---

## Troubleshooting

### Problem: MCP server not visible in PerDiem project

**Solution:**
- MCP servers are global in Claude Desktop
- Check MCP configuration hasn't changed
- Restart Claude Desktop
- Verify `claude_desktop_config.json` correct

### Problem: Can't access PerDiem docs

**Solution:**
- Verify files exist in `apps/perdiem/` in context-library
- Check file paths in bootstrap match actual paths
- Test MCP access manually by asking Claude to read specific file
- Check MCP server logs for errors

### Problem: Timeline shows all projects' events

**Solution:**
- Filtering may not be implemented yet
- Manually filter by looking at project field in events
- Consider implementing project filter in timeline_projection tool

### Problem: Continuation prompt loads wrong project

**Solution:**
- Verify SessionState event has correct project field
- Check MCP prompts code filters by project correctly
- May need to update prompt implementation in server

---

**Pattern validated!** You now have a proven process for creating separate projects for each app while sharing Nexus infrastructure.