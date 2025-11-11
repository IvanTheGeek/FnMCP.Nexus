# F# Coding Conventions

**Framework:** FnMCP.IvanTheGeekDevFramework  
**Language:** F# 8.0  
**Updated:** 2025-01-15  
**Status:** Team Standard

## File Organization

### File Ordering in Project
```xml
<!-- .fsproj file order matters! -->
<ItemGroup>
  <!-- 1. Type definitions first -->
  <Compile Include="Types.fs" />
  <Compile Include="Domain.fs" />
  
  <!-- 2. Business logic -->
  <Compile Include="Validation.fs" />
  <Compile Include="Commands.fs" />
  <Compile Include="Events.fs" />
  
  <!-- 3. Services -->
  <Compile Include="Services/GPS.fs" />
  <Compile Include="Services/Storage.fs" />
  
  <!-- 4. UI Components -->
  <Compile Include="Components/Entry.fs" />
  <Compile Include="Components/Session.fs" />
  
  <!-- 5. Main entry point last -->
  <Compile Include="Program.fs" />
</ItemGroup>
```

### Module Structure
```fsharp
namespace LaundryLog.Domain

// 1. Opens at top
open System
open FsToolkit.ErrorHandling

// 2. Module declaration
module Session =
    
    // 3. Type definitions
    type SessionState =
        | Active
        | Completed
    
    // 4. Private helpers
    let private validateEntries entries =
        // ...
    
    // 5. Public functions
    let create location =
        // ...
    
    // 6. Active patterns (if any)
    let (|HasEntries|Empty|) session =
        // ...
```

## Naming Conventions

### Types and Modules
```fsharp
// Types: PascalCase
type SessionId = SessionId of Guid
type LaundryEntry = { ... }

// Modules: PascalCase
module Session =
module Validation =

// Generic type parameters: 'a, 'b or 'T for clarity
type Result<'T, 'Error> = 
type Container<'a> =
```

### Functions and Values
```fsharp
// Functions: camelCase
let calculateTotal quantity price =
let validateEntry entry =

// Values: camelCase
let defaultQuantity = 1
let maxEntries = 99

// Constants: PascalCase or SCREAMING_SNAKE
let [<Literal>] DefaultPort = 8080
let [<Literal>] MAX_RETRIES = 3
```

### Active Patterns
```fsharp
// Single case: PascalCase
let (|EmailAddress|) (s: string) =
    // ...

// Multiple cases: PascalCase
let (|Child|Teen|Adult|Senior|) age =
    // ...

// Partial: PascalCase with underscore
let (|ParsedInt|_|) str =
    // ...
```

## Type Design

### Discriminated Unions
```fsharp
// Prefer unions over class hierarchies
type PaymentMethod =
    | Cash
    | CreditCard of last4: string  // Named fields
    | Points of accountId: string

// Use single-case for domain modeling
type EmailAddress = private EmailAddress of string
type CustomerId = CustomerId of Guid
```

### Record Types
```fsharp
// Prefer records over classes
type Session = {
    Id: SessionId
    Location: Location  
    Entries: Entry list
    CreatedAt: DateTime
}

// Use 'with' for updates
let updated = { session with Entries = newEntries }
```

### Option and Result
```fsharp
// Use Option for optional values
type User = {
    Name: string
    Email: EmailAddress option  // Not null!
}

// Use Result for operations that can fail
let parseEmail (s: string) : Result<EmailAddress, string> =
    if String.IsNullOrWhiteSpace s then
        Error "Email cannot be empty"
    else
        Ok (EmailAddress s)
```

## Function Design

### Parameter Order
```fsharp
// Data parameter last for piping
let map f list =         // ✅ Good
let map list f =         // ❌ Bad

// Partial application friendly
let add x y = x + y
let addFive = add 5      // Natural partial application
```

### Function Composition
```fsharp
// Use composition operators
let processEntry =
    validateEntry
    >> Result.bind enrichWithLocation  
    >> Result.map calculateTotal

// Pipeline for data flow
let result =
    input
    |> validate
    |> Result.bind process
    |> Result.map format
```

### Async Patterns
```fsharp
// Async for I/O operations
let fetchData url = async {
    let! response = Http.get url
    return response
}

// AsyncResult for async operations that can fail  
let saveSession session = asyncResult {
    do! validateSession session
    let! savedId = Database.save session
    return savedId
}
```

## Error Handling

### Domain Errors
```fsharp
type ValidationError =
    | MissingField of field: string
    | InvalidValue of field: string * reason: string
    | BusinessRule of rule: string

type DomainError =
    | Validation of ValidationError list
    | NotFound of id: string
    | Conflict of reason: string
```

### Result Usage
```fsharp
// Prefer Result over exceptions
let divide x y =
    if y = 0 then 
        Error "Division by zero"
    else 
        Ok (x / y)

// Use computation expression
let workflow = result {
    let! x = parseInt "42"
    let! y = parseInt "7"  
    let! result = divide x y
    return result
}
```

## Collections

### List vs Array vs Seq
```fsharp
// List: Default immutable collection
let process items =
    items
    |> List.map transform
    |> List.filter isValid
    |> List.fold aggregate initial

// Array: When performance matters
let processLarge (items: int[]) =
    items
    |> Array.Parallel.map heavyComputation
    
// Seq: For lazy evaluation
let infinite = 
    Seq.initInfinite id
    |> Seq.take 100
```

### Collection Patterns
```fsharp
// Pattern match on lists
let rec sum = function
    | [] -> 0
    | head :: tail -> head + sum tail

// Use collection modules
let result =
    items
    |> List.choose tryParse
    |> List.groupBy category
    |> List.sortByDescending snd
```

## Testing Conventions

### Test Naming
```fsharp
module SessionTests =
    
    [<Test>]
    let ``create should return active session with location`` () =
        // Descriptive test names with backticks
        
    [<Property>]
    let ``adding entries maintains total integrity`` (entries: Entry list) =
        // Property-based test naming
```

### Test Organization
```fsharp
// Group related tests
[<Tests>]
let sessionTests =
    testList "Session" [
        test "create returns active session" {
            // Arrange
            let location = createTestLocation()
            
            // Act
            let session = Session.create location
            
            // Assert
            Expect.equal session.Status Active "Should be active"
        }
        
        testProperty "total equals sum of entries" <| fun entries ->
            // Property test
    ]
```

## Documentation

### XML Documentation
```fsharp
/// <summary>
/// Calculates the total cost for a laundry session
/// </summary>
/// <param name="entries">List of laundry entries</param>
/// <returns>Total cost in USD</returns>
let calculateTotal (entries: Entry list) : Money =
    // ...
```

### Inline Comments
```fsharp
let processEntry entry =
    // Validate first to ensure data integrity
    validate entry
    |> Result.bind (fun valid ->
        // GPS might fail, so handle gracefully
        enrichWithLocation valid
        |> Result.defaultValue valid
    )
```

## Performance Patterns

### Tail Recursion
```fsharp
// Use accumulator for tail recursion
let sum items =
    let rec loop acc = function
        | [] -> acc
        | h :: t -> loop (acc + h) t  // Tail position
    loop 0 items
```

### Memoization
```fsharp
// Cache expensive computations
let memoize f =
    let cache = Dictionary<_, _>()
    fun x ->
        match cache.TryGetValue x with
        | true, v -> v
        | _ ->
            let v = f x
            cache.[x] <- v
            v
```

## Interop

### JavaScript Interop (Fable/Bolero)
```fsharp
// Import JavaScript functions
[<Import("localStorage", from="Browser.WebStorage")>]
let localStorage: ILocalStorage = jsNative

// Emit raw JavaScript when needed
[<Emit("console.log($0)")>]
let consoleLog (msg: string) : unit = jsNative
```

### C# Interop
```fsharp
// Design F# APIs for C# consumption
[<CompiledName("CreateSession")>]
let createSession location =
    // Use CompiledName for C#-friendly names
    
// Avoid F#-specific types in public APIs
type ISessionService =
    abstract CreateSession: Location -> Task<Session>
```

## Code Smells to Avoid

### Avoid Mutation
```fsharp
// ❌ Bad: Mutable
let mutable total = 0
for item in items do
    total <- total + item.Price

// ✅ Good: Immutable
let total = 
    items 
    |> List.sumBy (fun item -> item.Price)
```

### Avoid Null
```fsharp
// ❌ Bad: Null checking
let getName (user: User) =
    if user = null then "" 
    else user.Name

// ✅ Good: Option type
let getName (user: User option) =
    user |> Option.map (fun u -> u.Name) |> Option.defaultValue ""
```

### Avoid Deep Nesting
```fsharp
// ❌ Bad: Nested matching
match x with
| Some y ->
    match y with
    | Some z ->
        match z with
        | Some w -> w
        | None -> default
    | None -> default
| None -> default

// ✅ Good: Computation expression
option {
    let! y = x
    let! z = y  
    let! w = z
    return w
} |> Option.defaultValue default
```

## Style Guide

### Indentation
- Use 4 spaces (never tabs)
- Align pattern match cases
- Align record fields

### Line Length
- Prefer 80-100 characters max
- Break long pipelines into multiple lines
- One parameter per line for many parameters

### Formatting
```fsharp
// Pipeline formatting
let result =
    input
    |> step1
    |> step2
    |> step3

// Record formatting  
let session = {
    Id = SessionId.create()
    Location = location
    Entries = []
    CreatedAt = DateTime.UtcNow
}

// Pattern match formatting
match result with
| Ok value -> 
    processValue value
| Error msg -> 
    handleError msg
```

---

*These conventions ensure consistent, idiomatic F# code across the framework. The goal is readable, maintainable, and performant functional code.*