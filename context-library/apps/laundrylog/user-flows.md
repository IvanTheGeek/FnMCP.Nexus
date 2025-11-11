# LaundryLog User Flows

**App:** LaundryLog  
**Format:** Step-by-step scenarios  
**Updated:** 2025-01-15  
**Status:** Implementation Ready

## Primary Flow: Complete Laundry Session

### Scenario
John, a long-haul trucker, arrives at Love's Travel Stop in Amarillo, TX at 2:47 PM on a Tuesday. He needs to do two loads of laundry.

### Steps

1. **Open App at Laundry Room**
   - John opens LaundryLog on his phone
   - App requests location permission (first time only)
   - GPS captures location: Love's Travel Stop, Amarillo TX
   - Screen shows: `SessionEntry_Add_Initial`

2. **Add First Entry (Washer)**
   - Taps Washer button (machine type selected)
   - Taps + button once (quantity = 2)
   - Price auto-fills to $3.50 (historical at this location)
   - Taps Quarters payment method
   - Entry total shows: "2 × $3.50 = $7.00"
   - Taps "Add Entry" button
   - Entry appears in list below
   - Form resets for next entry

3. **Add Second Entry (Dryer)**
   - Taps Dryer button
   - Quantity stays at 2 (sticky from last entry)
   - Price auto-fills to $3.00 (community average)
   - Payment remains Quarters (sticky)
   - Entry total shows: "2 × $3.00 = $6.00"
   - Taps "Add Entry"
   - Second entry appears in list
   - Session total shows: $13.00

4. **Complete Session**
   - Reviews entries in list
   - Taps "Complete Session" button
   - Screen shows: `Session_Complete`
   - Receipt generated with:
     - Location: Love's Travel Stop, Amarillo TX
     - Date/Time: Tuesday, Nov 12, 2024, 2:47 PM
     - Entries: 2 washers ($7.00), 2 dryers ($6.00)
     - Total: $13.00
     - IRS-compliant format

### Result
John has documented his laundry expense with location proof for tax deduction.

## Alternative Flows

### GPS Unavailable

**Scenario:** Maria is inside a metal building truck stop where GPS doesn't work.

1. **Location Capture Fails**
   - Opens app
   - GPS spinner shows for 5 seconds
   - "GPS Unavailable" message appears
   - "Enter Location Manually" button shows

2. **Manual Entry**
   - Taps manual entry button
   - Types "Pilot"
   - Autocomplete suggests "Pilot Travel Center - Oklahoma City, OK"
   - Selects correct location
   - Continues with normal flow

### Using Quick-Fill Prices

**Scenario:** Bob regularly does laundry at the same Pilot location.

1. **Historical Price Available**
   - Selects Washer
   - Price section shows three options:
     - Last: $3.75 (from earlier today)
     - Historical: $3.50 (his usual here)
     - Community: $3.45 (average from 47 reports)
   - Taps "Historical" button
   - Price fills to $3.50

### Editing an Entry

**Scenario:** Sarah accidentally entered wrong quantity.

1. **Mistake Made**
   - Added entry with 3 loads instead of 2
   - Entry shows in list

2. **Edit Entry** (Future feature)
   - Taps on entry in list
   - Edit screen opens with current values
   - Changes quantity from 3 to 2
   - Taps "Update"
   - Entry updates in list
   - Total recalculates

### Empty Session Prevention

**Scenario:** Tom tries to complete without adding entries.

1. **Attempts Completion**
   - Opens app
   - Location captured
   - Doesn't add any entries
   - "Complete Session" button is disabled
   - Validation shows: "Add at least one entry"

2. **Adds Entry**
   - Adds washer entry
   - "Complete Session" button enables
   - Can now complete

## Error Handling Flows

### Network Offline

**Scenario:** Dave has no cell signal at rural truck stop.

1. **Works Offline**
   - App works normally
   - All data saves locally
   - Sync icon shows offline state

2. **Later Sync**
   - Regains signal at next stop
   - App automatically syncs
   - Cloud backup icon shows success

### Invalid Data Entry

**Scenario:** Mike tries entering invalid quantity.

1. **Types Invalid Number**
   - Tries to enter 0 quantity
   - Minus button is disabled
   - Cannot submit with 0

2. **Maximum Exceeded**
   - Keeps tapping + button
   - Reaches 99
   - Plus button disables
   - Message: "Maximum 99 loads"

## Special Cases

### Multiple Payment Methods

**Scenario:** Linda pays with both quarters and credit card.

1. **First Entry with Quarters**
   - Adds washer entry with quarters
   - Entry shows in list

2. **Second Entry with Card**
   - Adds dryer entry
   - Selects Credit Card payment
   - Entry shows different payment icon
   - Both entries in same session

### Session Spanning Midnight

**Scenario:** Carlos starts laundry at 11:45 PM.

1. **Started Tuesday Night**
   - Creates session at 11:45 PM
   - Adds washer entries

2. **Completed Wednesday Morning**
   - Adds dryer entries at 12:30 AM
   - Completes session
   - Receipt shows:
     - Started: Tuesday 11:45 PM
     - Completed: Wednesday 12:32 AM
     - Duration: 47 minutes

### Returning User

**Scenario:** Janet uses app second time at same location.

1. **App Remembers Preferences**
   - Opens app at familiar Love's
   - Location recognized
   - Previous prices suggested
   - Last payment method pre-selected

2. **Faster Entry**
   - Only needs to tap machine type
   - Confirm quantity (usually correct)
   - Verify price (usually correct)
   - Add entry - done in 10 seconds

## Export Flows

### Email Receipt

**Scenario:** Paul needs receipt for employer reimbursement.

1. **Complete Session**
   - Finishes laundry session
   - Receipt displayed

2. **Share Receipt**
   - Taps share button
   - Chooses email
   - Receipt attached as PDF
   - Sends to self or employer

### Monthly Export

**Scenario:** Amy exports monthly expenses for taxes.

1. **Access History**
   - Opens settings/history
   - Selects "Export"
   - Chooses date range

2. **Generate Export**
   - Selects CSV format
   - File generated with all sessions
   - Saves to phone or cloud
   - Ready for tax software import

## Community Features

### Price Contribution

**Scenario:** Ken notices prices changed at his regular stop.

1. **Updates During Entry**
   - Enters new price manually
   - App notices difference from community average
   - "Share price update?" prompt appears

2. **Contributes Data**
   - Confirms price is correct
   - Price submitted anonymously
   - Community average updates
   - Helps other drivers

### Using Community Data

**Scenario:** Rachel visits new truck stop.

1. **No Historical Data**
   - First time at this location
   - No personal history available

2. **Community Helps**
   - Community average shows $3.50
   - "Based on 23 reports" indicator
   - Uses community price
   - Saves time guessing

## Accessibility Flows

### Voice Over Navigation

**Scenario:** Bill uses screen reader.

1. **Navigate by Voice**
   - Each button announced clearly
   - "Washer button, not selected"
   - "Quantity, 1 load, adjustable"
   - "Add entry button, enabled"

2. **Confirmations Spoken**
   - "Entry added"
   - "Session total: 13 dollars"
   - "Session completed successfully"

### Large Text Mode

**Scenario:** George needs larger text.

1. **System Setting Respected**
   - Phone set to largest text
   - App scales appropriately
   - Buttons remain touch-friendly
   - Layout adjusts without breaking

## Key Flow Principles

1. **Minimize steps** - Most common path is fastest
2. **Prevent errors** - Validation before, not after
3. **Smart defaults** - Reduce decision fatigue
4. **Clear feedback** - User always knows what happened
5. **Graceful degradation** - Works even when things fail
6. **Progressive disclosure** - Advanced features don't clutter

---

*These flows represent real-world usage patterns discovered through seven design iterations and user testing at actual truck stops.*