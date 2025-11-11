# Bolero Patterns

**Framework:** FnMCP.IvanTheGeekDevFramework  
**Library:** Bolero (F# Blazor)  
**Updated:** 2025-01-15

## Core Patterns

### Elmish Architecture
```fsharp
type Model = { Count: int }
type Message = Increment | Decrement

let update message model =
    match message with
    | Increment -> { model with Count = model.Count + 1 }, Cmd.none
    | Decrement -> { model with Count = model.Count - 1 }, Cmd.none

let view model dispatch =
    div [] [
        button [on.click (fun _ -> dispatch Decrement)] [text "-"]
        span [] [text (string model.Count)]
        button [on.click (fun _ -> dispatch Increment)] [text "+"]
    ]
```

### Component Pattern
```fsharp
type MyComponent() =
    inherit Component()
    
    [<Parameter>]
    member val Value = 0 with get, set
    
    [<Parameter>]
    member val OnChange = Unchecked.defaultof<int -> unit> with get, set
    
    override this.Render() =
        div [] [
            input [
                attr.value (string this.Value)
                on.change (fun e -> this.OnChange(int e.Value))
            ]
        ]
```

### Routing
```fsharp
type Page =
    | [<EndPoint "/">] Home
    | [<EndPoint "/session/{id}">] Session of id: string
    | [<EndPoint "/settings">] Settings

let router = Router.infer SetPage (fun m -> m.Page)
```

### Remote Services
```fsharp
type IDataService =
    { GetSession: string -> Async<Session option> }
    
let service = 
    Remoting.createApi()
    |> Remoting.withRouteBuilder routeBuilder
    |> Remoting.buildProxy<IDataService>()
```

### HTML Templating
```fsharp
type Templates = Template<"wwwroot/templates.html">

let view model dispatch =
    Templates.Main()
        .Title("LaundryLog")
        .Body(viewBody model dispatch)
        .Elt()
```

### State Management
```fsharp
// Use Elmish subscriptions for external state
let subscription model =
    let sub dispatch =
        GPS.locationUpdates
        |> Observable.subscribe (LocationUpdated >> dispatch)
    Cmd.ofSub sub
```

### CSS Integration
```fsharp
// Use CSS modules or inline styles
let styles = [
    style.backgroundColor "#FFB366"
    style.padding (length.px 16)
    style.borderRadius (length.px 8)
]

div [attr.style styles] [text "Styled content"]
```

---

*Key patterns for building efficient Bolero applications with F# and WebAssembly.*