# F# Error Solutions - Nexus Knowledge Base

**Generated:** 2025-11-11 03:10:06
**Total Errors Documented:** 1

## FS3373

**Occurrences:** 2 times
**Solution Success Rate:** 50%

### Solutions

## Solution

Extract the formatted value to a variable before using it in the interpolated string.

## Fixed Code (Success)

```fsharp
let timestamp = meta.LastRegenerated.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK")
content.AppendLine($"last_regenerated: {timestamp}") |> ignore
```

## Success Rate

100% - Applied 5 times, worked every time.

## Pattern

This is an instance of the `interpolated-string-variable-extraction` pattern.

**Related patterns:** interpolated-string-variable-extraction

**Last encountered:** 2025-11-11

---

