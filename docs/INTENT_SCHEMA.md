# Intent Schema

The single contract between "how the instruction was understood" (local matcher or LLM) and "what actually happens in the drawing" (executor). Neither parser tier is allowed to bypass this shape.

```csharp
namespace Curiosity.Plugin.NlParser;

public enum IntentAction
{
    SetProperty,      // change a property on the target (lineweight, color, layer, linetype)
    ConstrainAngle,   // rotate/recompute target geometry to satisfy an angle relative to a reference entity
    ConstrainParallel,
    ConstrainPerpendicular,
    RunMacro,         // trigger a named multi-step macro (layout setup, purge, layer standardization)
    Unrecognized      // neither tier could confidently resolve the instruction; UI shows a clarification prompt
}

public record EntityReference(
    string Kind,        // "selection" | "named" | "nearest"
    string? Name = null  // e.g. "ground line", if the instruction refers to something other than the current selection
);

public record EditIntent(
    IntentAction Action,
    EntityReference Target,
    IReadOnlyDictionary<string, object> Parameters,
    double Confidence,      // 0.0-1.0; local matcher only returns high-confidence results, else defers to LLM tier
    string ResolvedBy       // "local" | "llm" — logged, used to grow the local matcher's phrasing set over time
);
```

## Parameter conventions by action

- `SetProperty`: `{"property": "Lineweight"|"Color"|"Layer"|"Linetype", "value": <string, mapped to the real AutoCAD enum/value by the executor>}`
- `ConstrainAngle`: `{"degrees": <double>, "reference": <EntityReference>}`
- `ConstrainParallel` / `ConstrainPerpendicular`: `{"reference": <EntityReference>}`
- `RunMacro`: `{"name": <string>}` — macro names are a fixed, documented registry (see `Execution/Macros/`), never freeform.

## Lineweight word mapping (needed for acceptance test 1)

AutoCAD's `LineWeight` enum has fixed discrete values (in mm, ×100): 0, 5, 9, 13, 15, 18, 20, 25, 30, 35, 40, 50, 53, 60, 70, 80, 90, 100, 106, 120, 140, 158, 200, 211. "Medium" is not an official AutoCAD term, so the executor needs an explicit, documented mapping rather than guessing per-call. Proposed: `thin → 0.25mm (LineWeight025)`, `medium → 0.35mm (LineWeight035)`, `thick → 0.60mm (LineWeight060)`. This mapping lives in `Execution/CommandExecutor.cs` as a named constant table, not inline magic numbers, so it's a single place to adjust if it doesn't match classmates' expectations in practice.
