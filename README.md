# Curiosity — a natural-language plugin for AutoCAD

Not a replacement for AutoCAD. A plugin that runs **inside** real AutoCAD (Windows) and removes the tedium: type a plain-English instruction, have it executed on your selected geometry immediately, and get one-click fixes for the multi-step annoyances (layout/paper-space setup, purging, standardizing layers) instead of doing them by hand every time.

**Audience:** the project owner and classmates, who already have genuine AutoCAD licenses through school. That fact is load-bearing for the whole design — see "Why this scope" below.

## What it does

- Select an entity, type an instruction into a docked chat panel inside AutoCAD, and watch it execute immediately. Examples from spec:
  - "change to medium line weight" → sets the selected object's lineweight property.
  - "make sure this line intersects with the ground line at a 45 degree angle" → recomputes/constrains the selected line's geometry against a referenced entity to satisfy the stated angle.
- One-click macros for known multi-step chores (paper-space/layout setup, purge, layer/linetype standardization) instead of memorizing the manual sequence.
- Full manual AutoCAD editing stays exactly as-is, untouched — this is an additive layer, never a replacement UI.

## Why this scope (read before changing it)

An earlier version of this project aimed to rebuild an entire free, cross-platform, AutoCAD-parity CAD application from scratch. That is real engineering fiction at any timeline under many years and doesn't serve the actual goal, which is: **make AutoCAD itself less miserable for people who already have it.** That reframe made three things true, on purpose:

1. **We ride on real AutoCAD**, not reimplement it — so we inherit its DWG fidelity, its full toolset, and its licensing (each user needs their own AutoCAD, which this audience already has).
2. **Windows only.** AutoCAD's plugin API (.NET/ObjectARX) is Windows-only. AutoCAD for Mac does not support .NET/ObjectARX plugins (AutoLISP only, a much thinner surface). There is no AutoCAD for Linux at all — a "plugin" cannot exist for software that isn't there. This was decided explicitly, not defaulted into: see `STATUS.md` decision log.
3. **Small, shippable, testable** — weeks/months, not years.

## Architecture

- **Host:** AutoCAD (Windows), plugin loaded via `NETLOAD`, built on the AutoCAD .NET API (`AcMgd`/`AcDbMgd`/`AcCoreMgd`).
- **UI:** a docked `PaletteSet` panel with a chat-style input box, inside the AutoCAD window.
- **NL layer, two-tier:**
  1. **Local pattern matcher** — fast, offline, handles well-defined phrasings (lineweight/color/layer changes, common geometric constraints) with no network call.
  2. **LLM fallback** (Claude API) — for anything the local matcher doesn't confidently recognize. Takes the instruction + selected-entity context (type, properties, nearby geometry) and returns a structured edit command in the same schema the local matcher produces, so there is exactly one execution path regardless of which tier resolved the instruction.
- **Execution:** structured commands are applied via the AutoCAD .NET API's `Database`/`Transaction`/`Editor` objects directly — not simulated keystrokes.

See `docs/ARCHITECTURE.md` for the full design and `docs/PLUGIN_SETUP.md` for how to build/load it against a real AutoCAD install (this repo is authored outside Windows/AutoCAD, so it has never been compiled or run against the real product — building and testing it is the necessary next real step, and it must happen on an actual Windows machine with AutoCAD + the ObjectARX SDK installed).

## Status

See `STATUS.md` for exactly where the build is and what's next.

## Prior concept (archived)

The original from-scratch CAD program concept is preserved for reference in `docs/archive-fullbuild-cad-concept/` — its pain-point research is still used to prioritize what this plugin fixes first.
