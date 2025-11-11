# Development Philosophy

**Framework:** FnMCP.IvanTheGeekDevFramework  
**Core Principle:** Maximum Freedom + Practical Capitalism  
**Updated:** 2025-01-15  
**Status:** Foundational Document

## Maximum Freedom + Practical Capitalism

### The Core Philosophy

Software should maximize human freedom while acknowledging economic reality. This means:
- **Open source by default** (AGPLv3 licensing)
- **No data monetization** (users own their data)
- **Optional paid services** (hosting, support, premium features)
- **Transparent business model** (no hidden agendas)

### Why AGPLv3?

The GNU Affero General Public License v3 ensures:
- **Source code remains open** - Even hosted services must share code
- **Modifications stay open** - Improvements benefit everyone
- **Network use triggers copyleft** - SaaS loophole closed
- **Patent protection included** - Defensive against trolls

This prevents companies from taking our code, hosting it as a service, and keeping improvements private.

### The Capitalism Component

We're not anti-business, we're pro-freedom:
- **Hosting services** - Charge for convenience, not access
- **Priority support** - Businesses pay for guaranteed response times
- **Custom features** - Sponsored development that benefits all
- **Training/consulting** - Expertise has value

## Build For Yourself First

### The Authenticity Principle

**If you wouldn't use it yourself, don't build it.**

This ensures:
- **Real problems get solved** - Not imaginary ones
- **Quality stays high** - You suffer from bad UX
- **Features stay relevant** - You know what matters
- **Dogfooding happens naturally** - Continuous real-world testing

### Personal Pain Points

Ivan's problems that sparked Cheddar ecosystem:
- **LaundryLog** - No receipts from truck stop laundry machines
- **PerDiemLog** - Complex IRS per diem rules for truckers
- **CheddarBooks** - QuickBooks is expensive and complex
- **FnTools** - Developer tools that respect privacy

Each app solves a problem Ivan personally experiences as a truck driver and developer.

## Privacy-First Design

### Data Principles

**Your data belongs to you:**
- **Local-first storage** - Data stays on your device
- **Optional sync** - You choose what to share
- **Export everything** - No lock-in, ever
- **Delete means delete** - No "soft delete" retention

### No Surveillance Capitalism

We will never:
- Track user behavior for advertising
- Sell or share user data
- Use dark patterns to extract data
- Make privacy settings complicated
- Require unnecessary permissions

### Community Data Sharing

**Opt-in collaborative intelligence:**
- Users can share anonymized data
- Helps establish defaults (like laundry prices)
- Community benefits from collective knowledge
- Always optional, never required
- Clear value exchange

Example: LaundryLog users sharing truck stop laundry prices helps everyone, but it's completely optional.

## Excellent UX Over Commercial Alternatives

### The Problem with Commercial Software

Most business software is terrible because:
- **Enterprise sales model** - Sell to executives, not users
- **Feature creep** - Everything for everyone means nothing works well
- **Lock-in focused** - Make it hard to leave
- **Complexity addiction** - Justify high prices with features
- **Support profit center** - Bad UX drives support revenue

### Our Approach

**Radical simplicity:**
- Do one thing excellently
- Remove features, don't add them
- Make the right thing the easy thing
- Optimize for the common case
- Handle edge cases without complicating the core

**Example:** LaundryLog does expense tracking for laundry. That's it. It does it perfectly for truck drivers. It doesn't try to be a general expense tracker.

## Community-Driven Development

### The Influence System

**Those who contribute most shape the product:**

**Ways to earn influence:**
- **Code contributions** - Features, bug fixes, documentation
- **Financial support** - Sponsorship, subscriptions, bounties
- **Community help** - Answer questions, write guides, test beta
- **Bug reports** - Quality reports with reproduction steps
- **Feature design** - Thoughtful proposals with use cases

**Influence determines:**
- Feature prioritization
- Design decisions
- Roadmap direction
- Breaking change votes

### Transparent Governance

All decisions are public:
- Roadmap discussions in forum
- Vote results published
- Influence calculations transparent
- No backroom deals
- Community can fork if they disagree

## Technical Excellence

### Type-Driven Development

**F# and strong typing because:**
- **Make illegal states unrepresentable** - Can't exist if it can't compile
- **Domain modeling in types** - Code documents itself
- **Fewer runtime errors** - Caught at compile time
- **Refactoring confidence** - Compiler has your back
- **Less testing needed** - Types eliminate whole categories of bugs

### Functional Programming

**Pure functions and immutability because:**
- **Predictable behavior** - Same input, same output
- **Easy testing** - No hidden dependencies
- **Safe concurrency** - No race conditions
- **Time-travel debugging** - Replay event streams
- **Composition** - Build complex behavior from simple parts

### Event Sourcing

**Events as source of truth because:**
- **Complete audit trail** - Everything that ever happened
- **Time travel** - Replay to any point
- **Debugging superpowers** - See exact sequence
- **GDPR compliance** - Selective event removal
- **Read model flexibility** - Multiple projections

## Sustainable Development

### The Long Game

Building for decades, not quarters:
- **Technical debt avoided** - Do it right the first time
- **Documentation prioritized** - Future developers matter
- **Backward compatibility** - Don't break user's workflows
- **Migration paths** - When breaking changes necessary
- **Knowledge preservation** - Decisions documented

### Sustainable Pace

**Marathon, not sprint:**
- No crunch time or death marches
- Regular refactoring and cleanup
- Time for learning and experimentation
- Work-life balance respected
- Burnout actively prevented

### Financial Sustainability

**Multiple revenue streams:**
- **Individual subscriptions** - $5-10/month for hosting
- **Business subscriptions** - $50-100/month with support
- **Enterprise contracts** - Custom development and deployment
- **Training and certification** - Educational revenue
- **Consulting services** - Implementation help

All optional - self-hosted remains free forever.

## Framework Principles

### Start Small, Stay Focused

Each app should:
- Solve one problem well
- Ship quickly with core features
- Iterate based on real usage
- Resist feature creep
- Maintain conceptual integrity

### Mobile-First, Desktop-Capable

Design for constraints:
- Mobile forces simplicity
- Touch-first prevents complexity
- Battery/bandwidth consciousness
- Desktop becomes easy after mobile
- Progressive enhancement approach

### Documentation as First-Class

Documentation is code:
- Written alongside implementation
- Reviewed in pull requests
- Tests verify documentation accuracy
- Examples that actually run
- Versioned with code

## Cultural Values

### Inclusive Community

Everyone welcome who shares our values:
- Respectful disagreement encouraged
- Diverse perspectives valued
- Mentorship and learning supported
- No gatekeeping or elitism
- Mistakes are learning opportunities

### Pragmatic Idealism

Balance principles with reality:
- Perfect is enemy of good
- Ship iteratively
- Learn from users
- Adjust based on feedback
- Stay flexible on tactics, firm on values

### Joy in Craft

Software development should be enjoyable:
- Beautiful code matters
- Elegant solutions preferred
- Learning new things encouraged
- Celebrate wins together
- Have fun while building

## The Cheddar Promise

To our users:
1. **Your data remains yours** - Always
2. **Software stays open** - Forever
3. **No dark patterns** - Ever
4. **Community drives direction** - Through influence
5. **Quality over features** - Always

To our contributors:
1. **Your contributions valued** - Through influence system
2. **Your time respected** - No waste on politics
3. **Your growth supported** - Learn and teach
4. **Your ideas heard** - Open governance
5. **Your work preserved** - Proper attribution

## Success Metrics

We measure success by:
- **Users helped** - Real problems solved
- **Contributors engaged** - Active community
- **Code quality** - Maintainable and elegant
- **Financial sustainability** - Not growth at all costs
- **Freedom preserved** - No compromise on principles

---

*This philosophy guides every decision in the FnMCP.IvanTheGeekDevFramework. When in doubt, we choose freedom, simplicity, and user empowerment.*