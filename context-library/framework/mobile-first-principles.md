# Mobile-First Design Principles

**Framework:** FnMCP.IvanTheGeekDevFramework  
**Source:** LaundryLog v1-v7 iterations  
**Updated:** 2025-01-15  
**Status:** Core Framework Pattern

## Overview

Seven iterations of LaundryLog taught us that mobile-first isn't just "responsive design" - it's designing for the reality of how mobile users actually interact with applications in challenging environments. These principles apply to all Cheddar ecosystem apps.

## Touch Target Fundamentals

### Minimum Sizes
- **Absolute minimum:** 72px × 72px for ANY interactive element
- **Primary actions:** 108px diameter circles (quantity adjustments, main CTAs)
- **Secondary actions:** 84px diameter circles (price adjustments, options)
- **Full-width buttons:** 343px × 72px (submission, completion, navigation)
- **Icon buttons:** 72px × 72px minimum, 84px × 84px preferred

### Why These Sizes Matter
Real-world conditions truck drivers face:
- **Dirty/wet hands** from weather, grease, or laundry
- **Gloves** in cold weather
- **Bright sunlight** causing screen glare
- **Time pressure** between loads or during mandatory breaks
- **Fatigue** after long driving shifts
- **One-handed operation** while holding laundry or phone

### Button Spacing Rules
- **Minimum gap:** 8px between any interactive targets
- **Typical gap:** 12-16px for comfortable distinction
- **Section gaps:** 20-24px between logical groups
- **Edge padding:** 16px from screen edges minimum

## Thumb Zone Optimization

### Screen Zones (Portrait)
```
Top 20%    - Stretch zone (avoid critical actions)
Middle 40% - Comfortable reach (secondary actions)
Bottom 40% - Easy reach (primary actions, navigation)
```

### Placement Strategy
- **Bottom zone:** Submit buttons, primary CTAs, tab navigation
- **Middle zone:** Input fields, selection options, adjustments
- **Top zone:** Information display, logos, status indicators
- **Never at top:** Anything requiring immediate action

## Interaction Patterns

### Button-First Design
**95% of interactions should be buttons**, not text input:
- Machine type: Washer/Dryer buttons (not dropdown)
- Quantity: +/- circular buttons (not number input)
- Price: Quick-select pills (not keyboard)
- Payment: Method buttons (not select list)

### When Keyboard Is Necessary
- **Auto-scroll** content up when keyboard appears
- **Numeric-only** keyboard for numbers
- **Done button** prominently displayed
- **Validation** happens on blur, not during typing
- **Default values** pre-filled when possible

## Smart Defaults System

### Priority Hierarchy
1. **Last used** - Current session value
2. **Historical** - User's common value at this location
3. **Community** - Crowd-sourced typical value
4. **System** - Reasonable fallback default

### Implementation
```fsharp
let getDefault (field: FieldType) (context: Context) =
    match field with
    | Quantity ->
        context.LastUsed
        |> Option.orElse context.Historical
        |> Option.orElse context.Community
        |> Option.defaultValue SystemDefaults.quantity
```

### Sticky Preferences
Within a session, remember:
- Payment method (rarely changes)
- Location (if manually entered)
- Price per unit (at same location)
- Typical quantities

## Visual Validation

### Proactive vs Reactive
**Proactive:** Prevent errors before they happen
- Disable invalid options
- Show requirements upfront
- Guide toward valid input

**Reactive:** Report errors after the fact
- Confusing for users
- Requires correction
- Increases friction

### Validation States
```
Empty     → Gray outline, no indicator
Partial   → Orange outline, missing items listed
Valid     → Green checkmark, submit enabled
Invalid   → Red X, specific error message
```

### Visual Indicators
- **Pills/Tags:** Show missing requirements as removable tags
- **Disabled states:** 50% opacity with gray overlay
- **Progress indication:** Fill progress as requirements met
- **Color + Icon:** Never rely on color alone

## Color Psychology

### Muted Palettes
Bright colors cause eye strain during extended use:
- **Muted orange** (#FFB366) instead of bright (#FF9800)
- **Soft backgrounds** (#FFF4E6) instead of white (#FFFFFF)
- **Gentle contrasts** for long viewing sessions

### Accessibility Standards
- **Text contrast:** 4.5:1 minimum (WCAG AA)
- **Button contrast:** 3:1 minimum for large text
- **Error states:** Red + icon (not just red)
- **Success states:** Green + checkmark

## Realistic Controls

### Skeuomorphic Elements
Some real-world metaphors improve usability:
- **Quarter buttons:** Look like silver coins
- **Machine icons:** Washer has water drops, dryer has heat waves
- **Credit card:** Shows chip/tap symbols
- **Cash:** Shows bill denomination

### Progressive Disclosure
Don't show everything at once:
1. Start with most common options
2. Reveal advanced options on request
3. Hide rarely-used features
4. Keep primary flow uncluttered

## Performance Considerations

### Instant Feedback
Mobile users expect immediate response:
- **Touch feedback:** Visual change within 100ms
- **Optimistic updates:** Show success immediately, sync later
- **Loading states:** Skeleton screens, not spinners
- **Offline-first:** Work without connection, sync when available

### Battery and Data
Respect user resources:
- **Minimal animations:** Subtle, not battery-draining
- **Image optimization:** Compress, use WebP
- **Local storage:** Reduce network requests
- **Background sync:** Don't drain battery

## Testing Methodology

### Real-World Testing
Test in actual conditions:
- Bright sunlight (outdoor visibility)
- One-handed operation
- While walking/moving
- With dirty/wet screen
- In landscape and portrait
- With accessibility tools enabled

### User Feedback Integration
From LaundryLog iterations:
- **v1-v2:** "Buttons too small" → Increased to 72px minimum
- **v3-v4:** "Too much typing" → Added quick-select options
- **v5-v6:** "Can't see in sunlight" → Increased contrast
- **v7:** "Perfect for truck stop use" → Validated approach

## Framework Application

These principles apply across all Cheddar apps:
- **LaundryLog:** Proven implementation
- **PerDiemLog:** Following same patterns
- **CheddarBooks:** Extending to desktop while maintaining mobile excellence
- **Future apps:** Start with these principles

## Key Takeaways

1. **Assume challenging conditions** - Design for worst case
2. **Minimize typing** - Buttons over keyboards
3. **Respect thumb reach** - Bottom-screen primary actions
4. **Smart defaults save time** - Learn from usage
5. **Visual feedback prevents errors** - Proactive validation
6. **Test in real conditions** - Not just at a desk

---

*These principles emerged from real truck drivers using LaundryLog at actual truck stops. They represent hard-won knowledge about mobile UX in challenging environments.*