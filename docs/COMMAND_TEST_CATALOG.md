# Command Test Catalog

The actual "ready for testing" tracker toward the full ~1,500-command/variable target, in the same shape as the lineweight acceptance test: a phrase, an expected result, and a way to check it. Data lives in `data/command_test_catalog.csv` (not prose here, since it needs to scale to ~1,500 rows and eventually drive an automated/semi-automated test runner).

## Columns

- `command` — the real AutoCAD command name.
- `category` — rough grouping (2D Drafting, Modify, Annotation, Organization, View, Settings, 3D Modeling, Output, plus one category per specialized toolset as those are added).
- `nl_phrase` — the plain-English instruction to type into the Curiosity chat panel.
- `expected_command_string` — the literal AutoCAD command-line text `RunNativeCommand` should produce and send via `SendStringToExecute`, written the way a person would type it (options, then Enter/blank-line to accept defaults).
- `verification` — how to confirm it actually worked, mirroring how the lineweight test was verified via the Properties panel (Ctrl+1) rather than trusting the chat panel's "Done" message alone.
- `status` — `not_tested` / `passed` / `failed` / `partial` (a "partial" case is flagged in the row itself, e.g. a command that opens a Windows dialog rather than prompting at the command line — see "Known exception categories" below).

## How this gets used

1. This session (Claude) researches and populates rows, expanding category by category, toolset by toolset, toward the full count.
2. The project owner (or a classmate) runs each `nl_phrase` through the real Curiosity panel against a real AutoCAD drawing, and checks the `verification` condition.
3. Results feed back into `status` and, for anything that fails, into a bug report the same way the lineweight test's initial regex-ordering bug got caught and fixed — a real result changes real code, not just fills in a checkbox.

## Known exception categories (flagged honestly, not silently skipped)

Some commands cannot be driven purely through `SendStringToExecute` text, for reasons explained in `docs/ARCHITECTURE.md`'s "Full command-surface coverage strategy":
- **Dialog-based commands** (`PLOT`, `EXPORTPDF`, `DSETTINGS`, and others that open a Windows dialog rather than prompting at the command line) — some can be driven by setting `FILEDIA`/`CMDDIA` to 0 first to force command-line prompts instead of a dialog; others genuinely cannot. Each such row is marked `partial` with a note on what does/doesn't work, not silently dropped from the catalog.
- **Freeform interactive commands** (live grip-dragging, some mesh-sculpting workflows) — flagged as out of scope by nature, per the documented, permanent ceiling in `docs/ARCHITECTURE.md`.
- **Commands needing an unspecified point on empty canvas** — the catalog's `nl_phrase` always supplies a coordinate (e.g. "at 0,0") specifically to keep the test well-defined; real usage still requires the same information from whoever's typing.

## Current coverage

- **First batch (this pass): 70 core commands** across 2D Drafting, Modify, Annotation, Organization, View, Settings, 3D Modeling, and Output — see `data/command_test_catalog.csv`.
- **Remaining toward ~1,000 core commands**: the rest of AutoCAD's core command set beyond this first batch (see `docs/archive-fullbuild-cad-concept/COMMAND_PARITY_CHECKLIST.md` for the ~150-command aliased-core reference this expands from).
- **Remaining toward ~1,500 total**: the full command sets of each specialized toolset (Architecture, Mechanical, Electrical, MEP, Plant 3D, Map 3D, Raster Design), not yet started.
- **System variables (~900)**: not yet catalogued as test rows — these are far more uniformly text-drivable (`SETVAR name value`), so they're lower-risk and lower-priority than commands; tracked as a later pass in `STATUS.md`.

This file and the CSV are updated every time a new batch is researched — check `STATUS.md` for exactly how far the catalog currently reaches and what the next batch covers.
