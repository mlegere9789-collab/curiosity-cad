# Architecture

## Project layout

```
/src
  /Curiosity.Plugin        # AutoCAD .NET plugin (C#, class library, NETLOAD-able)
    Commands.cs             # [CommandMethod] entry points (e.g. CURIOSITY to open the panel)
    ChatPalette.cs           # PaletteSet + WinForms/WPF chat UI
    NlParser/
      LocalPatternMatcher.cs # tier 1: regex/keyword-based intent parser, no network
      LlmFallbackClient.cs   # tier 2: Claude API call, same output schema as tier 1
      IntentSchema.cs        # shared structured-command types both tiers produce
    Execution/
      CommandExecutor.cs     # applies an IntentSchema command via AutoCAD .NET API
      GeometrySolver.cs      # angle/intersection constraint math for geometric intents
    Curiosity.Plugin.csproj
  /Curiosity.Tests          # unit tests for the parser + executor (the parts testable without AutoCAD running)
docs/
  ARCHITECTURE.md            # this file
  PLUGIN_SETUP.md             # build/NETLOAD instructions for a real Windows+AutoCAD machine
  INTENT_SCHEMA.md            # the structured command format both parser tiers must produce
```

## The structured intent schema (the contract between NL parsing and execution)

Both the local matcher and the LLM fallback must emit the same shape, so `CommandExecutor` has exactly one code path regardless of which tier resolved the instruction. See `docs/INTENT_SCHEMA.md` for the full type definitions. Sketch:

```csharp
record EditIntent(
    string Action,              // e.g. "SetProperty", "ConstrainAngle", "RunMacro"
    string TargetSelector,      // "selection" (default) or an explicit entity reference like "ground line"
    Dictionary<string, object> Parameters // action-specific: {"property": "Lineweight", "value": "Medium"} etc.
);
```

## Two-tier resolution flow

1. User types into the chat panel with an AutoCAD selection active.
2. `LocalPatternMatcher` attempts to resolve the text against a fixed set of known phrasings (lineweight/color/layer changes, common angle/intersection constraints, macro triggers). If confident, returns an `EditIntent` immediately — no network round-trip, works offline.
3. If the local matcher doesn't recognize the phrasing (or its confidence is low), `LlmFallbackClient` sends the instruction plus selected-entity context (type, current properties, IDs/positions of nearby candidate reference entities like "the ground line") to the Claude API, which returns an `EditIntent` in the same schema.
4. `CommandExecutor` takes the `EditIntent` (from either tier) and applies it via the AutoCAD .NET API inside a `Transaction`, so it's a normal, undoable AutoCAD edit — no special-cased "AI edit" that behaves differently from a manual one.

## The two owner-specified acceptance tests (must both work end to end)

1. Select a line → type "change to medium line weight" → `LocalPatternMatcher` resolves `Action=SetProperty, Parameters={property: Lineweight, value: Medium}` → `CommandExecutor` sets `Entity.LineWeight = LineWeight.LineWeight035` (AutoCAD's "medium" mapping) inside a transaction.
2. Select a line → type "make sure this line intersects with the ground line at a 45 degree angle" → parser identifies the target line + a reference entity named/tagged "ground line" in the drawing → `GeometrySolver` computes the endpoint/rotation needed to satisfy a 45° angle against the reference line's direction → `CommandExecutor` applies the recomputed geometry.

## Local-vs-LLM boundary (kept deliberately conservative at first)

Start the local matcher's known-phrasing set narrow and exact (property changes: lineweight, color, linetype, layer; simple angle/parallel/perpendicular constraints against a named reference) and expand it only from real logged fallback cases — i.e. grow tier 1 based on what tier 2 actually sees in practice, not speculatively.

## Getting as close as possible to full command coverage and mouse-free selection

The project owner's target: everything AutoCAD's own command line can do (~1,000+ core commands, ~1,500+ once every specialized toolset is counted) done in typed English, plus selecting specific geometry through Curiosity instead of the mouse. Two actions carry this, both in `docs/INTENT_SCHEMA.md`:

- **`RunNativeCommand`** is the general-purpose answer to command-surface coverage: instead of Curiosity reimplementing each of AutoCAD's ~1,500 commands as its own structured action (an enormous, never-finished undertaking), the LLM translates an instruction into AutoCAD's own command-line syntax and `CommandExecutor` runs it via `SendStringToExecute` — the same mechanism `.scr` script files use. This means Curiosity inherits AutoCAD's full command surface automatically, as new AutoCAD versions add commands, without new Curiosity code for each one. The system prompt instructs the model to prefer a specific structured action (`SetProperty`/`Transform`/etc.) when one exists, and to fall back to `RunNativeCommand` only for what isn't otherwise covered — and to return `Unrecognized` rather than emit an incomplete/ambiguous command string.
- **`Select`** is the mouse-free answer: builds an AutoCAD selection filter (layer/entity type/color, wildcard-matched against layer names so a spoken description doesn't need to exactly match a real layer name) from a description, and sets it as AutoCAD's implied selection — so a chained instruction acts on it without a click.

**The honest, permanent ceiling on both**, stated plainly so it's never a surprise later: a command needing an arbitrary point picked on an empty canvas has no location to work from unless the instruction supplies one (a coordinate, or a relative description like "2 inches right of the selected point") — that's not a solvable gap, it's the same information a person has to supply by clicking. Likewise, `Select` can only select by a describable, filterable property (layer, type, color) — a vague visual description ("that weird shape in the corner") isn't something a selection filter can express. Within those real limits, this is designed to get as close to 100% of the command surface and as close to fully mouse-free selection as the underlying instruction actually contains the information needed to act.
