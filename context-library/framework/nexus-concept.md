# Nexus: The Knowledge Hub

**System:** FnMCP.IvanTheGeek MCP Server  
**Nickname:** Nexus  
**Purpose:** Central knowledge repository for framework documentation  
**Updated:** 2025-01-15

## What is Nexus?

Nexus is the nickname for the FnMCP.IvanTheGeek MCP server and its associated context-library. It serves as the central knowledge hub that makes documentation available on-demand to Claude and other LLMs.

## Architecture

### The Two-Tier System

**Tier 1: Quick Start (Always Loaded)**
- Lives in Project Knowledge
- Minimal essential context
- ~2,000 tokens total
- Provides framework awareness

**Tier 2: Nexus (On-Demand)**
- Served via MCP protocol
- Detailed documentation
- Fetched when needed
- No token cost until accessed

### File System Layout
```
/home/linux/FnMCP.IvanTheGeek/
├── context-library/        # The Nexus
│   ├── quick-start/       # Tier 1 (copy to Project Knowledge)
│   ├── framework/         # Tier 2 (MCP served)
│   ├── apps/             # Tier 2 (MCP served)
│   └── technical/        # Tier 2 (MCP served)
└── FnMCP.IvanTheGeek.dll  # MCP server binary
```

## How Nexus Works

### Read-on-Request Pattern
```fsharp
// No caching, always fresh
let getResource uri = async {
    let path = uriToPath uri
    let! content = File.ReadAllTextAsync(path)
    return { Uri = uri; Text = content; MimeType = "text/markdown" }
}
```

**Benefits:**
- Always up-to-date
- No restart needed
- Simple implementation
- Microsecond reads

### Tool Access
```yaml
Tool: FnMCP.IvanTheGeek:update_documentation
Purpose: Write documentation directly to Nexus
Parameters:
  - path: Relative path in context-library
  - content: Markdown content
  - mode: overwrite or append
```

## Usage Patterns

### For Claude/LLMs

**Starting a conversation:**
1. Quick Start loads automatically (2K tokens)
2. Claude has framework awareness
3. Detailed knowledge available on request
4. Token usage stays minimal

**Accessing detailed knowledge:**
1. User asks specific question
2. Claude identifies need for details
3. MCP fetches relevant resource
4. Information used in response
5. Resource can be released

### For Developers

**Adding documentation:**
1. Write markdown file
2. Save to appropriate folder in context-library
3. Immediately available (no restart)
4. Claude can access in next query

**Updating documentation:**
1. Edit file directly OR
2. Ask Claude to update via MCP tool
3. Changes instant
4. No deployment needed

## Why "Nexus"?

The name represents:
- **Connection point** between knowledge and LLMs
- **Central hub** for all framework documentation  
- **Neural nexus** - linking concepts together
- **Next-generation** documentation system

## Benefits Achieved

### Token Efficiency
- 88% reduction in base token usage
- From 15,000 to 2,000 tokens
- More room for actual conversation
- Details only when needed

### Maintenance
- Single source of truth
- Version controlled (git)
- Easy updates
- No synchronization issues

### Scalability
- Add unlimited documentation
- No Project Knowledge bloat
- Organized by topic
- Cross-references work

## Future Vision

### Planned Enhancements
- Semantic search across all docs
- Auto-generated indexes
- Usage analytics
- Community contributions
- Multi-project support

### Integration Points
- Penpot designs
- Event Models
- Test paths
- Community forum
- Generated code

## Best Practices

### Document Organization
1. **Focused files** - One concept per file
2. **Clear naming** - Descriptive paths
3. **Cross-references** - Link related docs
4. **Metadata headers** - Track updates
5. **Size discipline** - 4-10KB target

### Content Guidelines
1. **Start with overview** - What and why
2. **Include examples** - Concrete over abstract
3. **Show patterns** - Reusable solutions
4. **Document decisions** - Why not just what
5. **Keep current** - Update as you learn

---

*Nexus transforms documentation from a static burden into a dynamic, efficient knowledge system that scales with your framework.*