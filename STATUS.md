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

Real usage immediately surfaced the next real requirement: the project owner tried "rotatw ccw 10 degrees" (a phrasing the narrow local matcher correctly didn't recognize) and got the honest "no LLM API key configured yet" message — confirming the two-tier fallback behaves exactly as designed, but that tier 2 wasn't actually wired up. Fixed same session, not yet retested on real AutoCAD:
- `FindNamedReferenceLine` implemented: matches a "named" reference (e.g. "the ground line") to any `Line` whose layer name contains one of the reference's significant words, case-insensitive. First-cut convention, not yet validated against a real drawing.
- `IntentAction.Transform` added (Rotate/Move/Scale about an entity's own centroid) so the executor can act on instructions beyond property changes and angle constraints — this is what makes "rotate 10 degrees," "move it over," "scale it up" possible in principle, not just lineweight-style edits.
- `ChatPalette` now actually constructs `LlmFallbackClient`, reading `ANTHROPIC_API_KEY` from the environment at startup (each user supplies their own key — see `docs/PLUGIN_SETUP.md`).
- Fixed a latent bug in `LlmFallbackClient.ParseModelResponse`: the `reference` parameter (used by `ConstrainAngle`) was being flattened to a JSON string instead of parsed into the `EntityReference` object `CommandExecutor` actually casts it to — would have thrown `InvalidCastException` the first time anyone tried an angle instruction through the LLM tier. Never hit yet since tier 2 wasn't wired up until now.

None of this round has been compiled or run yet — that's the very next step.

## Honest gaps — what's designed but not implemented or not verified
- This entire round (Transform action, FindNamedReferenceLine, LLM wiring, the reference-parsing fix) has not been compiled or run yet.
- `FindNamedReferenceLine`'s layer-name-matching convention is a first guess, not validated against a real drawing — a classmate's actual layer naming habits may need a different strategy.
- `MacroRegistry.PurgeUnused` is still a rough placeholder (the ObjectIdCollection-building loop is a no-op stub).
- The LLM tier (API key wiring, response parsing, and the new Transform action in the system prompt) has never been exercised against a real Claude API call.

## Exact next action
1. Rebuild (`build.bat`) and confirm this round compiles clean.
2. Retest acceptance phrase 1 ("change to medium line weight") still works after the changes — regression check.
3. Test acceptance phrase 2 ("...intersects with the ground line at a 45 degree angle") for the first time — needs a second line on a layer with "ground" in its name in the test drawing first.
4. Set `ANTHROPIC_API_KEY` (see `docs/PLUGIN_SETUP.md`), fully restart AutoCAD, and test a genuinely novel phrasing like "rotate this 10 degrees counterclockwise" to confirm the LLM tier and the new `Transform` action work end to end.
5. Expand `LocalPatternMatcher`'s phrasing set based on what real tier-2 cases look like once step 4 is live, per `docs/ARCHITECTURE.md`'s "grow tier 1 from real tier-2 cases" principle.

## Ground rules
- Don't mark anything "done" here without it actually having run against real AutoCAD (or, for the pure-logic NlParser pieces, a real `dotnet test` run once an SDK is reachable).
- No autonomous scheduled sessions are running against this repo right now — paused per the project owner's "full pause" instruction. Resume manually when ready.
