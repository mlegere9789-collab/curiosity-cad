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

**BUILD SUCCEEDED** was confirmed. Not yet confirmed: that the DLL actually loads via `NETLOAD` and that `CURIOSITY` opens the panel — that's the very next step.

## Honest gaps — what's designed but not implemented or not verified
- `NETLOAD` + running the `CURIOSITY` command has not been confirmed yet — compiling and running are different things.
- `CommandExecutor.FindNamedReferenceLine` — throws `NotImplementedException` on purpose. How a classmate's drawing actually identifies "the ground line" (a layer name? an xdata tag? text label proximity?) needs a real drawing to decide against, not a guess from outside AutoCAD.
- `ChatPalette` doesn't yet read an API key or construct `LlmFallbackClient` — it's referenced but never instantiated. Needs to be wired to read `ANTHROPIC_API_KEY` (or a settings UI) once basic local-matcher flow is confirmed working.
- `MacroRegistry.PurgeUnused` is a rough placeholder (the ObjectIdCollection-building loop is a no-op stub) — needs real logic once compiling against the real `Database.Purge` API is possible.
- `LlmFallbackClient.ParseModelResponse` assumes a specific Claude response shape; unverified against a real API call.

## Exact next action
1. In AutoCAD 2027: run `NETLOAD`, browse to `src\Curiosity.Plugin\bin\Debug\net10.0-windows\Curiosity.Plugin.dll`, load it. Report whether it loads clean or throws (a common next failure at this stage: a runtime `FileNotFoundException`/`FileLoadException` if AutoCAD can't find a dependency DLL next to the plugin — if that happens, report the exact exception text).
2. Run the `CURIOSITY` command, confirm the docked chat panel actually appears.
3. Select a line in a drawing, type "change to medium line weight" into the panel, confirm the lineweight actually changes.
4. Only after 1–3 succeed: tackle the `FindNamedReferenceLine` design decision (needs a real drawing to test naming conventions against) and wire up the LLM fallback with a real API key.

## Ground rules
- Don't mark anything "done" here without it actually having run against real AutoCAD (or, for the pure-logic NlParser pieces, a real `dotnet test` run once an SDK is reachable).
- No autonomous scheduled sessions are running against this repo right now — paused per the project owner's "full pause" instruction. Resume manually when ready.
