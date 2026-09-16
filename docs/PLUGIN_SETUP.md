# Building and loading the plugin (do this on a real Windows + AutoCAD machine)

This repo was authored outside Windows and outside AutoCAD — there is no AutoCAD SDK or Windows Forms/GDI runtime available in the environment that wrote this code. The `NlParser` logic (the regex matcher, the intent schema) was sanity-checked with an equivalent standalone check and is believed correct; everything touching `AcMgd`/`AcDbMgd`/`AcCoreMgd` or `System.Windows.Forms` (`Execution/`, `Ui/`) has **never been compiled**, only written carefully against the documented API shape. Building it for the first time, on a real machine, is the actual next step — do not treat this code as verified until that's happened.

## Prerequisites

1. AutoCAD (2024+) installed, with a real license (school license is fine).
2. Visual Studio 2022 (Community is free) with the ".NET desktop development" workload.
3. The AutoCAD .NET managed assemblies — these ship inside your AutoCAD install, typically at:
   `C:\Program Files\Autodesk\AutoCAD 2026\AcMgd.dll`, `AcDbMgd.dll`, `AcCoreMgd.dll`
   (adjust the version folder to whatever you have installed).

## Build

1. Clone this repo.
2. Set the `AUTOCAD_INSTALL_DIR` environment variable to your AutoCAD install folder (the one containing the three DLLs above), or create a `src/Curiosity.Plugin/Curiosity.Plugin.csproj.user` file (gitignored) overriding the `HintPath`s directly.
3. Open `src/Curiosity.Plugin/Curiosity.Plugin.csproj` in Visual Studio, or build from the CLI:
   ```
   dotnet build src/Curiosity.Plugin/Curiosity.Plugin.csproj
   ```
4. This should produce `Curiosity.Plugin.dll` in `src/Curiosity.Plugin/bin/Debug/net48/`.

## Load into AutoCAD

1. Launch AutoCAD.
2. Run the `NETLOAD` command, browse to `Curiosity.Plugin.dll`, load it.
3. Run the `CURIOSITY` command — the chat panel should dock and appear.
4. Select a line in the drawing, type `change to medium line weight` into the panel, press Enter.

## Configuring the LLM fallback (optional, needed only for instructions the local matcher doesn't recognize)

Set your own Claude API key as an environment variable (`ANTHROPIC_API_KEY`) before launching AutoCAD — never commit a key to this repo. `ChatPalette` currently does not yet wire this up automatically (see `STATUS.md` — this is the next real gap after the first successful build).

## What to actually verify first (in order)

1. Does the project compile at all against a real AutoCAD install? (Most likely source of first-build errors: exact assembly version/path, or a namespace that differs slightly between AutoCAD versions.)
2. Does `NETLOAD` + `CURIOSITY` show the panel?
3. Does typing the spec's own two example instructions, with a line selected, actually do the right thing in the drawing?
4. Only after 1–3 pass: worry about UI polish, more macros, more local-matcher phrasings.

Report back what breaks at each step — that's real information this repo can't get any other way from outside Windows/AutoCAD.
