# Penpot Integration

**Framework:** FnMCP.IvanTheGeekDevFramework  
**Tool:** Penpot (Open Source Design)  
**Updated:** 2025-01-15

## Design to Code Workflow

### Penpot Setup

**Project Structure:**
```
LaundryLog/
├── Screens/
│   ├── SessionEntry_Add_Initial
│   ├── SessionEntry_Add_Active
│   └── Session_Complete
├── Components/
│   ├── button / primary / large
│   ├── button / quantity / increment
│   └── input / numeric / currency
└── Prototypes/
    └── Happy Path Flow
```

### Naming Conventions

**Screens:** `Purpose_Action_State`
- SessionEntry_Add_Initial
- Settings_Location_Manual

**Components:** Forward slash hierarchy
- button / primary / large
- card / entry / complete

### Export Patterns

**CSS Variables from Penpot:**
```css
:root {
    --color-primary: #FFB366;
    --spacing-base: 16px;
    --radius-button: 8px;
}
```

**Component Mapping:**
```fsharp
// Penpot component → F# component
let renderButton (design: PenpotButton) =
    button [
        attr.``class`` design.ClassName
        on.click design.OnClick
    ] [
        text design.Label
    ]
```

### Prototype Extraction

**Interactive Flows:**
- Export Penpot prototype as JSON
- Parse interactions for state machine
- Generate test paths from flows

**Path Generation:**
```fsharp
let extractPath (prototype: PenpotPrototype) =
    {
        Name = prototype.Name
        Screens = prototype.Boards |> List.map toScreen
        Transitions = prototype.Interactions |> List.map toTransition
    }
```

### Asset Management

**Image Export:**
- Icons: SVG format
- Graphics: WebP with fallback
- Logos: Inline SVG in HTML

**Font Handling:**
- System fonts preferred
- Web fonts lazy loaded
- Fallback stack defined

## API Usage (Future)

### Reading Designs
```fsharp
let getDesign fileId =
    Penpot.API.getFile fileId
    |> Async.map parseDesign

let extractComponents design =
    design.Pages
    |> List.collect (fun p -> p.Objects)
    |> List.filter isComponent
```

### Generating Code
```fsharp
let generateComponent (component: PenpotComponent) =
    sprintf """
    let %s () =
        div [attr.``class`` "%s"] [
            %s
        ]
    """ component.Name component.Class (renderChildren component)
```

---

*Bridging design and development through Penpot integration patterns.*