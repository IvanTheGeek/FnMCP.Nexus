# F# Coding Patterns - Nexus Knowledge Base

**Generated:** 2025-11-11 03:10:06
**Total Patterns:** 2

## Syntax

### Fixed FS3373 by extracting timestamp variable
**Confidence:** ⭐⭐⭐⭐⭐ (100% success, 8 uses)
**Last validated:** 2025-11-11

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

**Related errors:** FS3373

---

### Percent Sign Escaping in Interpolated Strings
**Confidence:** ⭐⭐⭐⭐⭐ (100% success, 2 uses)
**Last validated:** 2025-11-11

## Pattern

When using the `%` character in F# interpolated strings, it must be escaped as `%%`.

## Example

```fsharp
// ✗ Fails - F# interprets % as format specifier
$"Success rate: {pct}%"

// ✓ Works
$"Success rate: {pct}%%"
```

## Reason

F# uses `%` for format specifiers (printf-style), so literal percent signs must be escaped.

## Confidence

High - Used 2 times during Phase 3, 100% success rate.

---

