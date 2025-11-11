# Documentation Migration Strategy

**Purpose:** How to break down comprehensive docs into modular Nexus structure  
**Updated:** 2025-01-15  
**Status:** Proven Pattern

## The Challenge

Large documentation files (30-50KB) consume tokens unnecessarily when loaded into Project Knowledge. The solution: break them into focused modules served on-demand via MCP.

## Migration Process

### Step 1: Analyze Source Documents
- Identify major themes and sections
- Note file sizes (aim for 88% reduction)
- Map dependencies between concepts

### Step 2: Design Structure
```
Target Structure:
├── quick-start/     (2-3KB each, ~2K tokens total)
├── framework/       (4-10KB each, on-demand)
├── apps/           (4-10KB each, app-specific)
└── technical/      (2-10KB each, implementation)
```

### Step 3: Create Quick Start Files
**Criteria for Quick Start:**
- Essential for every conversation
- High-level overview only
- Self-contained
- Include context monitoring note

**Size targets:**
- Individual file: 2-3KB
- Total Quick Start: <10KB
- Token target: ~2,000 tokens

### Step 4: Extract Detailed Documentation
**Extraction strategy:**
- One concept per file
- Preserve all details
- Add cross-references
- Include metadata headers

### Step 5: Test Token Reduction
1. Add Quick Start files to Project Knowledge
2. Remove old comprehensive docs
3. Start new chat
4. Verify token reduction achieved
5. Test MCP resource access for details

## Results Achieved

**Before:**
- Project Knowledge: ~15,000 tokens
- Every conversation starts heavy
- Limited room for actual work

**After:**
- Project Knowledge: ~2,000 tokens
- 88% reduction achieved
- Details available on-demand
- More room for conversation

## Lessons Learned

1. **Modular is better** - Even if total KB increases
2. **Two-tier works** - Essential vs detailed
3. **MCP read-on-request** - No caching needed for markdown
4. **Quick Start discipline** - Resist adding "just one more thing"
5. **Cross-references crucial** - Files must link to each other

---

*This migration strategy transformed 3 large files into 20+ focused modules, achieving dramatic token savings while improving documentation quality.*