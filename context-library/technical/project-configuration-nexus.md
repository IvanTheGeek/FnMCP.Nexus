# Nexus Project Configuration Guide

**Purpose:** Step-by-step instructions for updating the Nexus Project with minimal bootstrap file.

**Current State:** Nexus project with framework-overview.md, current-focus.md, naming-conventions.md in Project Knowledge (~6KB).

**Goal:** Replace with single minimal bootstrap file (~500 tokens) that teaches Claude to use Nexus MCP.

---

## Step 1: Create the Bootstrap File

**Action:** Create `nexus-bootstrap.md` with the following content:

```markdown
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

Format: 📊 emoji, 20-char visual bar (█/░), comma-separated numbers, allocation tree with conversation breakdown, status legend.

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
```

**Save this file** to your computer for the next step.

---

## Step 2: Access Claude Project Settings

**Action:** Open Project Settings for the Nexus project.

**How to access:**
1. In Claude Desktop or web interface, look for the current project name at the top
2. Click on the project name or settings icon
3. Select "Project settings" or similar option
4. Navigate to "Project knowledge" section

**What you'll see:** List of current knowledge files:
- framework-overview.md
- current-focus.md  
- naming-conventions.md

---

## Step 3: Remove Current Knowledge Files

**Action:** Delete all existing Project Knowledge files.

**Steps:**
1. In Project knowledge section, find each file
2. Click the delete/remove button for each file:
   - Remove `framework-overview.md`
   - Remove `current-focus.md`
   - Remove `naming-conventions.md`
3. Confirm removal

**Why:** We're replacing ~6KB of static docs with ~500 token bootstrap file.

**Important:** The actual documentation files are NOT deleted - they're still in the context-library served by MCP. We're only removing them from Project Knowledge.

---

## Step 4: Upload Bootstrap File

**Action:** Add the new bootstrap file to Project Knowledge.

**Steps:**
1. In Project knowledge section, click "Add file" or "Upload" button
2. Select the `nexus-bootstrap.md` file you created in Step 1
3. Upload and confirm
4. Verify file appears in Project Knowledge list

**Result:** Project Knowledge now contains only the single bootstrap file.

---

## Step 5: Verify MCP Connection

**Action:** Ensure FnMCP.Nexus server is still connected.

**Verification:**
1. In Claude Desktop, check MCP servers section (usually in settings)
2. Confirm `FnMCP.Nexus` appears in the list
3. Check status shows "Connected" or similar indicator

**If not connected:**
1. Restart Claude Desktop
2. Check MCP server configuration in `claude_desktop_config.json`
3. Verify binary is at correct location

**Expected:** MCP server should remain connected; Project Knowledge changes don't affect MCP.

---

## Step 6: Test in New Chat

**Action:** Start a new chat in the Nexus project to test the bootstrap.

**Test steps:**
1. Click "New chat" within Nexus project
2. Type: "What am I working on?"
3. Observe Claude's behavior:
   - Should call `timeline_projection` or `recent_chats`
   - Should reference files via MCP
   - Should provide context about current work

**Expected behavior:**
```
Claude: "Let me check your recent work..."
[Calls timeline_projection]
Claude: "You're working on Phase 4 of Nexus development..."
```

---

## Step 7: Verify Token Savings

**Action:** Check token usage display in the test chat.

**Look for:**
```
📊 Context Usage: [███░░░░░░░░░░░░░░░░░] X,XXX / 190,000 tokens (X.X%)

Allocation:
├─ System Prompts: ~8,000 tokens (4.2%)
├─ Project Knowledge: ~500 tokens (0.3%) ← Should be ~500, not ~6,000!
├─ Conversation: ~X,XXX tokens
```

**Success criteria:**
- Project Knowledge: **~500 tokens** (was ~6,000)
- Total savings: **~5,500 tokens** (92% reduction)
- More conversation space available

---

## Step 8: Test MCP Document Access

**Action:** Verify Claude can still access detailed documentation.

**Test steps:**
1. In the new test chat, ask: "Explain the Event Modeling approach in detail"
2. Claude should:
   - Reference `framework/event-modeling-approach.md`
   - Use Nexus MCP to fetch the file
   - Provide detailed explanation

**Expected:** Claude accesses documentation on-demand without it being in Project Knowledge.

---

## Step 9: Test Continuation Prompts

**Action:** Verify MCP prompts still work.

**Test steps:**
1. Start another new chat
2. Click the `continue-core` prompt (or type the keyword)
3. Verify continuation context loads

**Expected:** Prompts work normally; they query events, not Project Knowledge.

---

## Step 10: Document and Commit

**Action:** Create event documenting the successful transition.

**In your current working chat (not test chat), run:**
```
Create a FrameworkInsight event documenting:
- Nexus project updated to use bootstrap file
- Token savings achieved (6,000 → 500)
- All functionality verified working
- Pattern ready for testing with PerDiem
```

**Then commit:**
```bash
git add -A
git commit -m "docs: Update Nexus Project to use minimal bootstrap file"
```

---

## Troubleshooting

### Problem: Claude doesn't access MCP docs

**Solution:**
- Verify MCP server is connected
- Check file paths in bootstrap match actual paths
- Restart Claude Desktop
- Check server logs for errors

### Problem: Token usage still high

**Solution:**
- Verify old knowledge files were actually removed
- Check bootstrap file size (should be <2KB)
- Start fresh chat to see clean baseline

### Problem: Continuation prompts don't work

**Solution:**
- Prompts don't depend on Project Knowledge
- Check SessionState events exist
- Verify MCP server prompts capability working
- May need to restart Claude Desktop

---

## Success Checklist

- [ ] Bootstrap file created and saved
- [ ] Old Project Knowledge files removed
- [ ] Bootstrap file uploaded to Project Knowledge
- [ ] MCP connection verified
- [ ] New chat test successful
- [ ] Token savings confirmed (~5,500 tokens)
- [ ] MCP document access working
- [ ] Continuation prompts working
- [ ] Event documented
- [ ] Changes committed to git

---

**Once complete:** You're ready to test the same pattern with PerDiemLog in a separate project!