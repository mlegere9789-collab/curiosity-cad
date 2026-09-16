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
    Transform,        // general rotate/move/scale about the target's own centroid
    Select,           // build a selection from a description and make it the active selection —
                       // mouse-free selection, e.g. "select the wall layer", "select all circles"
    RunNativeCommand, // fallback for the ~1500+ command long tail: translate to AutoCAD's own
                       // command-line syntax and run it via SendStringToExecute
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
- `Transform`: `{"operation": "Rotate", "degrees": <double>}` or `{"operation": "Move", "dx": <double>, "dy": <double>}` or `{"operation": "Scale", "factor": <double>}` — all applied about the target entity's own geometric-extents centroid.
- `Select`: `{"layer": <string?>, "entityType": <string?>, "colorIndex": <int?>}` — at least one present; `layer` is wildcard/substring-matched, not exact, since a spoken description ("the wall layer") won't exactly equal a real layer name ("A-WALL") most of the time. Sets AutoCAD's implied selection so a following instruction can target it without a target of its own.
- `RunNativeCommand`: `{"commandString": <string>}` — literal AutoCAD command-line text, newline-separated exactly as a person would type it, run via `SendStringToExecute`. This is the general fallback for the ~1,500+ command surface that doesn't have a dedicated structured action — see "Full command-surface coverage strategy" below for its real limits.
- `RunMacro`: `{"name": <string>}` — macro names are a fixed, documented registry (see `Execution/Macros/`), never freeform.

## Full command-surface coverage strategy (the "1,500+ commands" and "select without the mouse" requirements)

Rather than hand-writing a structured action for each of AutoCAD's ~1,000 core commands (~1,500+ once every specialized toolset is counted), Curiosity uses `RunNativeCommand` as a general translator: the LLM turns an instruction into the literal text a person would type at AutoCAD's own command line, and `CommandExecutor` runs it via `SendStringToExecute` — the same mechanism a `.scr` script file uses. AutoCAD's own command processor does the actual work, so Curiosity doesn't need to reimplement 1,500 commands one at a time; it needs to be a good translator into AutoCAD's existing language.

**This has a real, permanent limit, not a temporary gap**: a command that needs an arbitrary point picked on an empty part of the canvas — "draw a circle" with no location given — cannot be completed this way, because the instruction contains no location information for AutoCAD to use. English has to supply a coordinate or a relative description ("centered 2 inches right of the selected point") the same way a person has to choose where to click. `RunNativeCommand` is documented (in the LLM's own system prompt) to return `Unrecognized` rather than guess when an instruction doesn't supply enough information to write a complete, unambiguous command.

`Select` is the mouse-free counterpart: it builds an AutoCAD selection filter (by layer/entity type/color) from a description and sets it as the active selection, so a chained instruction ("select the wall layer" → "change to medium line weight") never needs a mouse click. Like `RunNativeCommand`, it has a real limit — it can only select by a describable, filterable property, not a vague visual description ("that weird shape in the corner").

## Lineweight word mapping (needed for acceptance test 1)

AutoCAD's `LineWeight` enum has fixed discrete values (in mm, ×100): 0, 5, 9, 13, 15, 18, 20, 25, 30, 35, 40, 50, 53, 60, 70, 80, 90, 100, 106, 120, 140, 158, 200, 211. "Medium" is not an official AutoCAD term, so the executor needs an explicit, documented mapping rather than guessing per-call. Proposed: `thin → 0.25mm (LineWeight025)`, `medium → 0.35mm (LineWeight035)`, `thick → 0.60mm (LineWeight060)`. This mapping lives in `Execution/CommandExecutor.cs` as a named constant table, not inline magic numbers, so it's a single place to adjust if it doesn't match classmates' expectations in practice.
