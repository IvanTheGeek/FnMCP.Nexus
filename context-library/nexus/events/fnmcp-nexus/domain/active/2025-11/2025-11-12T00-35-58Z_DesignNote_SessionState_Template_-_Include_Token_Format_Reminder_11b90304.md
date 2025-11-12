---
id: 9778f2c7-d3c9-43e7-b872-5fc3727ef10a
type: DesignNote
title: "SessionState Template - Include Token Format Reminder"
summary: "Pattern for including token format reminders in SessionState events"
occurred_at: 2025-11-11T19:35:58.617-05:00
tags:
  - session-state
  - token-monitoring
  - continuation
  - pattern
---

## SessionState Event Template Pattern

When creating SessionState events (used for session continuation prompts), include a reminder about token display format in the event body.

**Recommended section to include:**

```markdown
## Token Display Format Reminder

All responses in continuation sessions must use the standardized token display format. See `technical/context-monitoring.md` for complete specification.

**Quick reference:**
- 📊 emoji at start
- 20-character visual bar (█/░)
- Comma-separated numbers (not K abbreviations)
- Full allocation tree with conversation breakdown
- Status legend at bottom
```

**Why this matters:**

When users click continuation prompts (continue-core, continue-laundrylog, etc.), the SessionState event loads as context. Including the token format reminder ensures Claude knows the requirement even in new chat sessions started via prompts.

**Implementation:**

Add this section to SessionState events going forward. For consistency, can be a standard footer section in the event template.

**Benefit:**

Ensures every entry point into the project (new chats, continued chats, direct documentation access) reinforces the token display requirement.
