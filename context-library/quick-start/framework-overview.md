# FnMCP.IvanTheGeekDevFramework Overview

## Philosophy

**Maximum freedom + practical capitalism.** Build open-source software that solves real problems, licensed under AGPLv3 to ensure freedom while enabling optional paid services. Privacy-first design means no data monetization - users own their data. The guiding principle: build for yourself first, then share with others. If you wouldn't use it yourself, don't build it.

## Core Methodology

**Event Modeling drives everything.** Visual collaboration using orange commands, blue events, and green views creates shared understanding before writing code. The framework uses F# with Bolero for web applications, chosen for type safety and functional programming benefits. Penpot serves as the UI source of truth, with designs directly mapping to implementation through consistent naming conventions and component structures.

## Mobile-First Principles

Seven iterations of LaundryLog taught critical lessons: minimum 72px touch targets for reliable interaction, thumb-friendly bottom-screen placement for primary actions, button-based interaction over typing (95% of actions), and smart defaults that learn from usage patterns. Design for real-world conditions - dirty hands, bright sunlight, limited time. Every screen represents one discrete state with clear validation and visual feedback.

## Current Status

**MCP server operational.** FnMCP.IvanTheGeek successfully deployed and connected to Claude Desktop, enabling direct documentation updates to the context-library. The read-on-request pattern provides instant updates without restarts.

**LaundryLog v7 complete.** Production-ready HTML prototype with mobile-optimized design, GPS location tracking, session management, and comprehensive validation. Ready for F# + Bolero implementation.

**PerDiemLog in design.** Phase 1 focuses on manual per diem tracking for DOT HOS compliance, with future phases adding PDF automation.

**Framework architecture defined.** Event Modeling methodology documented, mobile-first patterns established, two-tier knowledge architecture implemented for efficient context management.

## Key Concepts

**Paths:** Inspired by Choose Your Own Adventure books, each path represents one complete execution trace through the system using concrete example data. Paths serve as narratives, test cases, documentation, and Penpot prototypes simultaneously.

**Static-State Design:** Each screen represents exactly one state. No hidden modes or complex state machines. Navigation is explicit, validation is visual, and users always know where they are.

**Cheddar Ecosystem:** Suite of financial tools for independent workers starting with LaundryLog and PerDiemLog, expanding to CheddarBooks (QuickBooks competitor) and beyond. Unified branding, shared infrastructure, consistent UX patterns.

**Influence System:** Contributors earn influence through code, documentation, or financial support. Influence determines feature prioritization - those who contribute most shape the product direction.

## Technical Stack

- **F#:** Type-driven development, functional paradigm
- **Bolero:** F# web framework with Elmish architecture
- **Penpot:** Open-source design tool, UI source of truth
- **MCP Protocol:** Tool and resource integration for LLMs
- **Event Modeling:** Visual specification methodology
- **SQLite:** Local-first data storage
- **Forum:** Community hub (evaluating Talkyard/Discourse)

## Next Steps

1. Populate context-library with modular documentation
2. Implement LaundryLog using established patterns
3. Complete PerDiemLog Phase 1 design
4. Select and deploy interim forum solution
5. Build EMapp prototype for Event Model visualization

---

**Note to Claude:** Always show context usage stats with visual bar at the end of responses to monitor token consumption and ensure efficient knowledge management.