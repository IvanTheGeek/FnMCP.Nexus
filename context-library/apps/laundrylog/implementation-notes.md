# LaundryLog Implementation Notes

**App:** LaundryLog  
**Stack:** F# + Bolero  
**Updated:** 2025-01-15  
**Status:** Implementation Starting

## Project Setup

### Solution Structure
```
LaundryLog/
├── LaundryLog.sln
├── src/
│   ├── LaundryLog.Domain/
│   │   ├── Types.fs
│   │   ├── Events.fs
│   │   ├── Commands.fs
│   │   └── Validation.fs
│   ├── LaundryLog.Web/
│   │   ├── Program.fs
│   │   ├── Main.fs
│   │   ├── Components/
│   │   │   ├── Entry.fs
│   │   │   ├── Session.fs
│   │   │   └── Location.fs
│   │   └── wwwroot/
│   │       ├── css/main.css
│   │       └── index.html
│   ├── LaundryLog.Storage/
│   │   ├── SQLite.fs
│   │   ├── EventStore.fs
│   │   └── Migrations.fs
│   └── LaundryLog.Services/
│       ├── GPS.fs
│       ├── Community.fs
│       └── Export.fs
├── tests/
│   ├── LaundryLog.Domain.Tests/
│   ├── LaundryLog.Web.Tests/
│   └── LaundryLog.Integration.Tests/
└── paket.dependencies
```

### Dependencies (Paket)
```
source https://api.nuget.org/v3/index.json

nuget Bolero
nuget Bolero.HotReload
nuget Microsoft.AspNetCore.Components.WebAssembly.DevServer
nuget FSharp.Core
nuget SQLite.Core
nuget FsToolkit.ErrorHandling
nuget Thoth.Json
nuget FsCheck
nuget Expecto
```

## Domain Implementation

### Core Types First
```fsharp
// Types.fs - Start here
namespace LaundryLog.Domain

module Types =
    // Start with simple types
    type SessionId = SessionId of Guid
    type EntryId = EntryId of Guid
    
    // Build up to complex types
    type MachineType = Washer | Dryer
    type PaymentMethod = 
        | Quarters 
        | CreditCard of last4: string
        
    // Use single-case unions for validation
    type Quantity = private Quantity of int
    module Quantity =
        let create n =
            if n < 1 then Error "Must be positive"
            elif n > 99 then Error "Max 99"
            else Ok (Quantity n)
```

### Event Sourcing Pattern
```fsharp
// Events.fs
module Events =
    type SessionEvent =
        | SessionStarted of SessionStartedData
        | EntryAdded of EntryAddedData
        | SessionCompleted of SessionCompletedData
    
    // Apply events to build state
    let apply state event =
        match event with
        | SessionStarted data ->
            { state with 
                Id = data.SessionId
                Location = data.Location }
        | EntryAdded data ->
            { state with
                Entries = data :: state.Entries }
```

## Bolero Components

### Main Application
```fsharp
// Main.fs
module LaundryLog.Web.Main

open Elmish
open Bolero
open Bolero.Html

type Model = {
    Session: Session option
    Location: Location option
    CurrentEntry: EntryDraft
    ValidationErrors: ValidationError list
}

type Message =
    | StartSession
    | LocationReceived of Location
    | SelectMachine of MachineType
    | SetQuantity of int
    | SetPrice of decimal
    | SelectPayment of PaymentMethod
    | AddEntry
    | CompleteSession

let update message model =
    match message with
    | StartSession ->
        model, Cmd.ofAsync GPS.getLocation () LocationReceived Error
    
    | LocationReceived location ->
        { model with 
            Location = Some location
            Session = Some (Session.create location) }, Cmd.none
            
    | SelectMachine machine ->
        { model with 
            CurrentEntry = { model.CurrentEntry with Machine = Some machine }
        }, Cmd.none

let view model dispatch =
    div [] [
        comp<Header> [] []
        
        cond model.Session <| function
        | None -> 
            comp<LocationCapture> [
                "OnLocation" => (LocationReceived >> dispatch)
            ] []
        | Some session ->
            comp<SessionEntry> [
                "Session" => session
                "Entry" => model.CurrentEntry
                "OnAddEntry" => (fun () -> dispatch AddEntry)
            ] []
    ]
```

### Component Pattern
```fsharp
// Components/Entry.fs
module LaundryLog.Web.Components.Entry

open Bolero
open Bolero.Html

type EntryComponent() =
    inherit Component()
    
    [<Parameter>]
    member val Session = Unchecked.defaultof<Session> with get, set
    
    [<Parameter>]
    member val Entry = Unchecked.defaultof<EntryDraft> with get, set
    
    [<Parameter>]
    member val OnAddEntry = Unchecked.defaultof<unit -> unit> with get, set
    
    override this.Render() =
        div [ attr.``class`` "entry-form" ] [
            // Machine selection
            div [ attr.``class`` "machine-selection" ] [
                button [
                    attr.``class`` (if this.Entry.Machine = Some Washer then "selected" else "")
                    on.click (fun _ -> this.SelectMachine Washer)
                ] [
                    span [ attr.``class`` "icon" ] [ text "💧" ]
                    text "Washer"
                ]
            ]
            
            // Quantity controls
            comp<QuantitySelector> [
                "Value" => this.Entry.Quantity
                "OnChange" => this.SetQuantity
            ] []
        ]
```

## Service Implementation

### GPS Service
```fsharp
// Services/GPS.fs
module LaundryLog.Services.GPS

open Browser.Navigator
open Browser.Types

let getLocation () : Async<Result<Location, string>> =
    async {
        // Use Browser.Navigator for GPS
        let! result = 
            Async.FromContinuations(fun (cont, econt, _) ->
                navigator.geolocation.getCurrentPosition(
                    (fun pos -> 
                        cont (Ok {
                            Latitude = pos.coords.latitude
                            Longitude = pos.coords.longitude
                        })),
                    (fun err -> 
                        cont (Error err.message))
                )
            )
        
        return 
            match result with
            | Ok coords -> 
                Ok (Location.fromCoordinates coords)
            | Error msg -> 
                Error msg
    }
```

### Local Storage
```fsharp
// Storage/LocalStorage.fs
module LaundryLog.Storage.LocalStorage

open Browser.WebStorage
open Thoth.Json

let private key = "laundrylog_session"

let saveSession (session: Session) =
    let json = Encode.Auto.toString(4, session)
    localStorage.setItem(key, json)

let loadSession () : Session option =
    localStorage.getItem(key)
    |> Option.ofObj
    |> Option.bind (fun json ->
        match Decode.Auto.fromString<Session>(json) with
        | Ok session -> Some session
        | Error _ -> None
    )

let clearSession () =
    localStorage.removeItem(key)
```

## CSS Implementation

### Mobile-First Styles
```css
/* wwwroot/css/main.css */

:root {
    --cheddar-orange: #FFB366;
    --cheddar-light: #FFCC80;
    --cheddar-pale: #FFF4E6;
    --text-dark: #2C3E50;
    --button-height: 72px;
    --circle-size: 108px;
}

/* Container */
.container {
    max-width: 375px;
    margin: 0 auto;
    padding: 0;
}

/* Touch targets */
.button-primary {
    min-height: var(--button-height);
    width: 100%;
    font-size: 18px;
    font-weight: 600;
}

/* Quantity circles */
.quantity-button {
    width: var(--circle-size);
    height: var(--circle-size);
    border-radius: 50%;
    background: var(--cheddar-orange);
    color: white;
    font-size: 36px;
}

/* Machine selection */
.machine-button {
    min-height: 120px;
    flex: 1;
}

.machine-button.selected {
    border: 3px solid var(--cheddar-orange);
    background: var(--cheddar-pale);
}
```

## PWA Configuration

### Manifest
```json
{
    "name": "LaundryLog",
    "short_name": "LaundryLog",
    "description": "Expense tracking for truck stop laundry",
    "start_url": "/",
    "display": "standalone",
    "background_color": "#FFFFFF",
    "theme_color": "#FFB366",
    "icons": [
        {
            "src": "/icon-192.png",
            "sizes": "192x192",
            "type": "image/png"
        }
    ]
}
```

### Service Worker
```javascript
// wwwroot/sw.js
const CACHE_NAME = 'laundrylog-v1';
const urlsToCache = [
    '/',
    '/css/main.css',
    '/_framework/blazor.webassembly.js',
    '/_framework/dotnet.wasm'
];

self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => cache.addAll(urlsToCache))
    );
});

self.addEventListener('fetch', event => {
    event.respondWith(
        caches.match(event.request)
            .then(response => response || fetch(event.request))
    );
});
```

## Testing Strategy

### Domain Tests
```fsharp
// Tests/Domain.Tests.fs
module LaundryLog.Domain.Tests

open Expecto
open FsCheck

[<Tests>]
let tests =
    testList "Domain" [
        testProperty "Quantity within bounds" <| fun n ->
            let result = Quantity.create n
            match result with
            | Ok _ -> n >= 1 && n <= 99
            | Error _ -> n < 1 || n > 99
            
        test "Cannot complete empty session" {
            let session = Session.create testLocation
            let result = Session.complete session
            Expect.isError result "Should not complete without entries"
        }
    ]
```

### Component Tests
```fsharp
// Tests/Component.Tests.fs
module LaundryLog.Web.Tests

open Bolero.Test

[<Tests>]
let tests =
    testList "Components" [
        test "Entry component renders" {
            let comp = EntryComponent()
            comp.Session <- testSession
            comp.Entry <- EntryDraft.empty
            
            let html = comp.Render()
            Expect.isNotNull html "Component should render"
        }
    ]
```

## Deployment

### Build Script
```bash
#!/bin/bash
# build.sh

# Clean
dotnet clean

# Restore
dotnet tool restore
dotnet paket restore

# Build
dotnet build -c Release

# Test
dotnet test

# Publish
dotnet publish -c Release -o ./dist
```

### GitHub Actions
```yaml
name: Build and Deploy

on:
  push:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    - uses: actions/setup-dotnet@v1
      with:
        dotnet-version: '8.0.x'
    - run: ./build.sh
    - uses: actions/upload-artifact@v2
      with:
        name: dist
        path: ./dist
```

## Performance Optimization

### Lazy Loading
```fsharp
// Load community data only when needed
let getCommunityPrices = 
    lazy (
        async {
            let! data = API.fetchCommunityData()
            return data
        }
    )
```

### Memoization
```fsharp
// Cache expensive calculations
let memoize f =
    let cache = System.Collections.Generic.Dictionary<_,_>()
    fun x ->
        match cache.TryGetValue(x) with
        | true, v -> v
        | false, _ ->
            let v = f x
            cache.[x] <- v
            v

let calculateTotal = memoize (fun (q, p) ->
    Money.calculate q p
)
```

## Common Pitfalls

### Avoid These Mistakes

1. **Don't use mutable state** - Embrace immutability
2. **Don't ignore error cases** - Use Result everywhere
3. **Don't skip validation** - Validate at boundaries
4. **Don't forget accessibility** - ARIA labels matter
5. **Don't assume online** - Offline-first always

### Do These Instead

1. **Use discriminated unions** - Make illegal states impossible
2. **Handle all pattern matches** - Compiler is your friend
3. **Test with real devices** - Simulators lie
4. **Profile on slow devices** - Not everyone has flagship phones
5. **Listen to users** - They know what they need

## Next Steps

1. **Implement core domain** - Types, then logic
2. **Build basic UI** - One screen at a time
3. **Add GPS service** - Handle all failure modes
4. **Implement storage** - Local first, sync later
5. **Deploy PWA** - Test with real users
6. **Iterate based on feedback** - Ship early, improve often

---

*These implementation notes provide a practical starting point. The key is to start simple, validate with users, and iterate quickly.*