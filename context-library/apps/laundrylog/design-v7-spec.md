# LaundryLog v7 Design Specification

**App:** LaundryLog  
**Version:** 7.0 (Production Ready)  
**Status:** HTML Prototype Complete  
**Updated:** 2025-01-15  

## Screen Specifications

### Dimensions and Layout
- **Width:** 375px (iPhone standard)
- **Height:** Scrollable (~1540px total)
- **Orientation:** Portrait only
- **Safe areas:** 16px padding from edges
- **Section spacing:** 20-24px between logical groups

### Color Palette

```css
/* Primary - Muted Orange */
--cheddar-orange: #FFB366;
--cheddar-light: #FFCC80;
--cheddar-pale: #FFF4E6;
--cheddar-text: #D97706;

/* Secondary */
--silver: #C0C0C0;      /* Quarter buttons */
--dark-gray: #4B5563;   /* Secondary text */
--success: #10B981;     /* Valid states */
--error: #EF4444;       /* Invalid states */
--white: #FFFFFF;       /* Backgrounds */
--border: #E5E7EB;      /* Subtle borders */
```

## Component Specifications

### Header
- **Height:** 64px
- **Background:** Gradient (#FFCC80 to #FFB366)
- **Content:** "🧺 LaundryLog / by CheddarBooks / 🧀"
- **Font:** 18px bold, white text
- **Shadow:** 0 2px 4px rgba(0,0,0,0.1)

### Location Capture
- **GPS Button:** 343px × 56px
- **States:**
  - Capturing: Pulsing animation
  - Success: Green checkmark
  - Failed: Red X with manual entry
- **Manual fallback:** Text input with autocomplete

### Machine Type Selection
- **Layout:** 2 columns, equal width
- **Button size:** 163.5px × 120px each
- **Icons:** 48px centered
- **Washer:** 💧 Water drops icon
- **Dryer:** 🔥 Heat waves icon
- **Selected state:** Orange border (3px), background #FFF4E6

### Quantity Controls
- **Circle buttons:** 108px diameter
- **Background:** #FFB366 (orange)
- **Icons:** + and - in white, 36px
- **Display:** 48px bold number between buttons
- **Label:** "QUANTITY" 13px uppercase
- **Min/Max:** 1-99 loads

### Price Controls
- **Circle buttons:** 84px diameter  
- **Quick-fills:** Three pills, 98px × 40px each
- **Labels:**
  - "Last" - Previous price in session
  - "Historical" - User's typical price here
  - "Community" - Crowd-sourced average
- **Input:** Numeric keyboard, auto-format currency

### Payment Method
- **Grid:** 2×2 layout
- **Button size:** 163.5px × 72px
- **Options:**
  - Quarters (silver coin appearance)
  - Credit Card (card icon)
  - Points (star icon)
  - Cash (bill icon)
- **Sticky:** Remembers last selection

### Entry Total
- **Display box:** 343px × 84px
- **Background:** Light gray (#F9FAFB)
- **Amount:** 32px bold
- **Formula:** Shows "2 × $3.50 = $7.00"
- **Real-time:** Updates as user adjusts

### Add Entry Button
- **Size:** 343px × 72px
- **Enabled:** Green (#10B981) when valid
- **Disabled:** Gray with 50% opacity
- **Text:** "Add Entry" 18px white
- **Validation:** Shows missing requirements

### Validation Display
- **Pills:** Inline tags showing requirements
- **Missing:** Red X icon + requirement text
- **Complete:** Green checkmark
- **Requirements:**
  - Machine type selected
  - Quantity > 0
  - Price entered
  - Payment selected

### Session Summary
- **Background:** #F3F4F6 full width
- **Padding:** 16px
- **Running total:** 24px bold
- **Update:** Real-time as entries added

### Entries List
- **Header:** "Today's Entries" with total
- **Cards:** 343px width, auto height
- **Content per card:**
  - Timestamp (top right)
  - Machine icon and type
  - Quantity × Price = Total
  - Payment method badge
- **Actions:** Swipe to delete (future)

### Complete Session Button
- **Size:** 343px × 72px
- **Color:** Orange (#FFB366)
- **Position:** Fixed bottom (with safe area)
- **Shows:** When entries exist
- **Action:** Saves and generates receipt

## Interaction Patterns

### Touch Targets
- **Minimum:** 72px × 72px
- **Primary actions:** 108px circles
- **Full-width buttons:** 343px × 72px
- **Spacing:** 8px minimum between targets

### Feedback
- **Touch:** Visual change within 100ms
- **Success:** Green flash or checkmark
- **Error:** Red shake animation
- **Loading:** Skeleton screens, not spinners

### Keyboard Handling
- **Numeric only** for quantities and prices
- **Auto-scroll** content when keyboard opens
- **Done button** prominent in keyboard
- **Dismiss** on tap outside

### Validation Flow
1. Start with all requirements shown
2. Remove pills as completed
3. Enable button when all valid
4. Show inline errors if needed
5. Prevent submission if invalid

## State Management

### Session States
```
NotStarted → LocationCaptured → EntryInProgress → HasEntries → Complete
```

### Entry States
```
Empty → MachineSelected → QuantitySet → PriceSet → PaymentSelected → Valid
```

### Data Persistence
- **Local first:** SQLite on device
- **Session recovery:** Restore incomplete sessions
- **Export ready:** CSV, PDF formats
- **Sync optional:** User controls when/if

## Responsive Behavior

### Screen Sizes
- **320px:** Reduce padding, maintain touch targets
- **375px:** Primary design (iPhone standard)
- **414px:** Increase padding proportionally
- **Tablet:** Center in 414px column

### Landscape
- Not supported in v7
- Shows rotation prompt
- Future: Two-column layout

### Accessibility
- **Touch targets:** Meet WCAG AAA (72px)
- **Color contrast:** 4.5:1 minimum
- **Focus indicators:** Visible keyboard navigation
- **Screen reader:** Semantic HTML, ARIA labels
- **Font scaling:** Supports up to 200%

## Performance Targets

### Load Times
- **Initial:** < 2 seconds on 3G
- **Subsequent:** < 500ms from cache
- **Interaction:** < 100ms response

### Data
- **Offline first:** Full functionality without network
- **Sync:** Background when connected
- **Storage:** < 10MB for 1000 sessions
- **Battery:** < 2% drain per session

## Error Handling

### GPS Failure
- Timeout after 5 seconds
- Show manual entry option
- Remember last location
- Suggest nearby truck stops

### Network Issues
- Queue actions for retry
- Show offline indicator
- Sync when reconnected
- No data loss

### Invalid Input
- Prevent invalid entry
- Show specific error
- Suggest correction
- Maintain other fields

## Success Criteria

### Usability Metrics
- **Entry time:** < 30 seconds
- **Error rate:** < 2%
- **Completion:** > 95%
- **Satisfaction:** > 4.5/5

### Technical Metrics
- **Uptime:** 99.9%
- **Sync success:** > 99%
- **Data integrity:** 100%
- **GPS capture:** > 95%

## Implementation Notes

### HTML Structure
```html
<div class="container">
  <header class="header">...</header>
  <main class="content">
    <section class="location">...</section>
    <section class="machine-type">...</section>
    <section class="quantity">...</section>
    <section class="price">...</section>
    <section class="payment">...</section>
    <section class="total">...</section>
    <button class="add-entry">...</button>
    <section class="entries">...</section>
  </main>
  <footer class="actions">...</footer>
</div>
```

### Critical CSS
```css
/* Touch targets */
.button-primary { min-height: 72px; }
.button-circle { width: 108px; height: 108px; }

/* Colors */
.machine-selected { 
  border: 3px solid #FFB366;
  background: #FFF4E6;
}

/* Disabled state */
.button:disabled {
  opacity: 0.5;
  pointer-events: none;
}
```

### JavaScript Behavior
- Progressive enhancement
- No framework dependency in prototype
- Event delegation for dynamic content
- LocalStorage for session persistence
- Service Worker for offline support

## Version History

**v7 (Current):** Production-ready design
- Muted orange palette
- Refined validation
- Complete session management
- Logged entries display

**v6:** Removed section headings
**v5:** Native keyboard
**v4:** Smart defaults added
**v3:** Thumb-friendly sizing
**v2:** Mobile-first approach
**v1:** Initial concept

---

*This specification represents the production-ready design after seven iterations of user feedback and real-world testing at truck stops.*