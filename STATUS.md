# Status

**Current focus:** Windows AutoCAD .NET plugin — natural-language chat-edit layer.
**Scope pivot on 2026-09-16:** abandoned the from-scratch full-CAD-program concept (archived in `docs/archive-fullbuild-cad-concept/`) in favor of a plugin for real AutoCAD, since the actual audience already has genuine AutoCAD licenses through school. Windows-only, decided explicitly — AutoCAD's plugin API doesn't exist on Mac (.NET/ObjectARX unsupported there) and AutoCAD doesn't exist on Linux at all.

## What's done
- Repo restructured for the new scope; old full-build docs preserved under `docs/archive-fullbuild-cad-concept/` for reference (their pain-point research is still useful).
- `docs/ARCHITECTURE.md`, `docs/INTENT_SCHEMA.md`, `docs/PLUGIN_SETUP.md` written.
- First real code, in `src/Curiosity.Plugin/`:
  - `NlParser/IntentSchema.cs` — the structured command contract between NL parsing and execution.
  - `NlParser/LocalPatternMatcher.cs` — tier-1 offline regex matcher. **Actually tested** (via an equivalent standalone check, since no .NET SDK was reachable in this environment — see below) against both of the spec's exact acceptance-test phrases and a negative case. Caught and fixed a real bug: the first version required "line weight" to appear before the weight word, which broke on the spec's own example ("change to **medium** line weight"). Now order-independent.
  - `NlParser/LlmFallbackClient.cs` — tier-2 Claude API fallback, same output schema as tier 1. Written against the documented API shape; **not yet tested end to end** (needs a real API key + real response to confirm the parsing of Claude's JSON reply is correct).
  - `NlParser/SelectionContext.cs` — builds LLM context from the live AutoCAD selection.
  - `Execution/CommandExecutor.cs` — applies an `EditIntent` via the AutoCAD .NET API in a `Transaction`. Implements `SetProperty` (lineweight/color/layer) and `ConstrainAngle`. **Not yet compiled or run** — no AutoCAD/Windows environment was available to this session.
  - `Execution/GeometrySolver.cs` — angle-constraint math (rotate target line to intersect a reference line at a given angle). Logic written and reasoned through; not yet unit-tested against real `Line` geometry (needs the AutoCAD `Geometry` assembly, which needs a real build).
  - `Execution/Macros/MacroRegistry.cs` — macro registry with one real (rough, flagged) macro (`purge-unused`); others intentionally not stubbed in so unimplemented macros fail loudly.
  - `Ui/ChatPalette.cs`, `Commands.cs` — the AutoCAD-side panel and NETLOAD entry point.
- `src/Curiosity.Tests/` — xunit tests for the local matcher (the one part with zero AutoCAD/Windows dependency). **Not run via `dotnet test`** — no .NET SDK was installable in this sandboxed Linux environment (both the official installer and apt were blocked). The same assertions were verified with an equivalent Python regex check instead; real `dotnet test` execution is the first thing to run on a real dev machine.

## First real build: 2026-09-15/16, on the project owner's real Windows machine + real AutoCAD 2027
Confirmed real, load-bearing finding from that build: AutoCAD 2027's managed API (`AcMgd`/`AcDbMgd`/`AcCoreMgd`) targets **.NET 10**, not .NET Framework 4.8 as originally assumed. The project went net48 -> net8.0-windows -> net10.0-windows across three real build attempts before compiling clean. `Curiosity.Plugin.csproj` now targets `net10.0-windows`; the DLL lands at `src/Curiosity.Plugin/bin/Debug/net10.0-windows/Curiosity.Plugin.dll` (not `net48` — `build.bat`'s success message had a stale path, now fixed). Also fixed by that build: a missing `UseWindowsForms` reference, an `init`-vs-`set` property mismatch (CS8852), an `Application` namespace collision between AutoCAD and WinForms (CS0104), and a literal double-hyphen in an XML comment that broke MSBuild's parser (MSB4025) — all real bugs this environment could never have found without an actual compiler and an actual AutoCAD install.

**BUILD SUCCEEDED** was confirmed, and then runtime was confirmed too, same session: `NETLOAD` loaded the DLL clean, `CURIOSITY` opened the docked chat panel, and **the spec's first acceptance test passed end to end on real AutoCAD**: with a real line selected, typing "change to medium line weight" into the panel produced `Done (local, confidence 95%)` and the line's actual `Lineweight` property (verified via the AutoCAD Properties palette, Ctrl+1) changed from `ByLayer` to `0.35 mm` — the exact value `LocalPatternMatcher`/`CommandExecutor` map "medium" to. This is the whole pipeline working for real: typed English -> local regex match -> structured `EditIntent` -> AutoCAD `.NET` API transaction -> visible property change -> verified independently in AutoCAD's own UI.

Not yet tested: the second acceptance phrase ("...intersects with the ground line at a 45 degree angle") — this will currently throw, since `CommandExecutor.FindNamedReferenceLine` is an intentional `NotImplementedException` stub (see below).

## Honest gaps — what's designed but not implemented or not verified
- `CommandExecutor.FindNamedReferenceLine` — throws `NotImplementedException` on purpose. How a classmate's drawing actually identifies "the ground line" (a layer name? an xdata tag? text label proximity?) needs a real drawing to decide against, not a guess from outside AutoCAD.
- `ChatPalette` doesn't yet read an API key or construct `LlmFallbackClient` — it's referenced but never instantiated. Needs to be wired to read `ANTHROPIC_API_KEY` (or a settings UI) once basic local-matcher flow is confirmed working.
- `MacroRegistry.PurgeUnused` is a rough placeholder (the ObjectIdCollection-building loop is a no-op stub) — needs real logic once compiling against the real `Database.Purge` API is possible.
- `LlmFallbackClient.ParseModelResponse` assumes a specific Claude response shape; unverified against a real API call.

## Exact next action
1. Design and implement `CommandExecutor.FindNamedReferenceLine` so the second acceptance phrase ("...intersects with the ground line at a 45 degree angle") can work at all. Needs a real decision on how a classmate's drawing identifies "the ground line" — simplest first cut: match by an AutoCAD layer named "ground" or "ground line" (case-insensitive), since that's the lowest-friction convention a student would actually use, with a clear error message if no matching layer/entity is found. Implement that, rebuild, retest the second acceptance phrase end to end the same way the first one was just verified (via Properties/inspection, not just the "Done" message).
2. Once both acceptance phrases pass: wire up `ANTHROPIC_API_KEY` reading in `ChatPalette` and test the tier-2 LLM fallback path with a phrasing the local matcher doesn't recognize.
3. Expand `LocalPatternMatcher`'s phrasing set based on what real fallback cases actually look like once tier 2 is live, per `docs/ARCHITECTURE.md`'s "grow tier 1 from real tier-2 cases" principle.

## Ground rules
- Don't mark anything "done" here without it actually having run against real AutoCAD (or, for the pure-logic NlParser pieces, a real `dotnet test` run once an SDK is reachable).
- No autonomous scheduled sessions are running against this repo right now — paused per the project owner's "full pause" instruction. Resume manually when ready.
