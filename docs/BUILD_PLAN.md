# Build Plan: Phase A → Phase P

Each phase's exit criteria gate the next. This file is the authoritative sequence — `STATUS.md` always points to where we are in it. Phases are additive to the original audit's Part Five plan; the NL chat-edit layer and AI-training dependency are folded into Phase I explicitly, and Phase K is rewritten for a totally-free/no-subscription model instead of subscribe-or-buy.

## Phase A — Lock the spec (this phase)
Convert every row of the pain-point table, the tally table, and the additional requirements (NL chat editing, free/no-subscription, all platforms) into a numbered, testable acceptance criterion. Convert the Command & System-Variable Reference into the canonical parity checklist — one row per legacy command/alias, each marked `must replicate`, `must replicate + fix`, or `superseded by [new mechanism]`, with the reason recorded. No application code is written until this checklist exists and has had 3 QC passes.
**Exit criteria:** `PAIN_POINTS_AND_REQUIREMENTS.md`, `COMMAND_PARITY_CHECKLIST.md`, `FEATURE_CATALOG.md` all committed, every row populated, no `TBD` rows remaining.

## Phase B — Choose the stack
- Core: Rust (memory-safe, one cross-platform geometry/document kernel compiled once, reused everywhere).
- GPU: `wgpu` (one abstraction over Vulkan/Metal/DirectX 12 — no three separate renderers).
- UI shell: native Rust GUI toolkit (egui or Slint) bound directly to the core — no Electron/webview layer for the primary app; same core later compiles to WebAssembly for the web build so desktop and web never drift apart.
- Scripting: embedded Python (PyO3) + a JS runtime for web-facing automation — the AutoLISP-replacement layer.
- 3D kernel: evaluate Open CASCADE (mature, LGPL, B-rep/NURBS) vs. a minimal Rust-native kernel; decision recorded with tradeoffs before Phase D closes.
**Exit criteria:** `docs/architecture/STACK_DECISION.md` written and justified against every relevant pain point.

## Phase C — Stand up the monorepo
One repository (this one), one build system (Cargo workspaces): `/core` (geometry + document model), `/render` (GPU layer), `/ui` (desktop shell), `/io` (file-format readers/writers), `/script` (Python/JS bindings), `/ai` (NL command layer + trained models), `/web` (WASM target), `/installers` (per-OS packaging). CI builds and runs the full test suite on Windows, Ubuntu LTS, and macOS (both architectures) on every commit.
**Exit criteria:** CI green on all three platforms on an empty "hello triangle" render.

## Phase D — Build the document & geometry core
One entity model (points, curves, surfaces, solids, blocks, dimensions, annotations, layers, sheets) — no duplicate primitive types, closing the "eight ways to draw a line" pain point at the data-model level. Two independent, append-only history logs from day one: Drawing History and View History — the undo/redo pain point is fixed architecturally, not patched later. Every entity carries a stable ID and diffable serialization (required for Time-Travel Drawing Diff). 3D solid kernel integrated per the Phase B decision.
**Exit criteria:** round-trip create/save/load/undo/redo test suite passes for every primitive type in the entity model.

## Phase E — File I/O and interoperability
Native format first: versioned, documented, back-compatible, with an explicit "features used" manifest on open (closes the "silently disables features across versions" pain point). DWG read/write validated against a large corpus of real AutoCAD-authored files for round-trip fidelity, with automated visual-diff regression tests. PDF, DXF, STEP, IFC, OBJ/GLB import-export. No TrustedDWG-style warning banner — validated fidelity replaces the trust signal.
**Known dependency:** production DWG fidelity needs either a licensed SDK or a long-horizon from-scratch reader/writer. Decision and cost/timeline tradeoff recorded in `docs/architecture/DWG_STRATEGY.md` before this phase closes.
**Exit criteria:** DWG round-trip fidelity report published against a real test corpus; native format spec frozen at v1.

## Phase F — Command system and UI shell
Unified command bar: exact-match on every legacy alias from the parity checklist (muscle memory works day one), fuzzy/plain-language matching second, ribbon-equivalent discoverability third (never the only way to reach a tool). Dynamic-block authoring as direct-manipulation, not a hidden action graph. Dyslexia- and screen-reader-friendly input handling wired in from the start, not a later accessibility pass. One continuous canvas with sheet frames — no model-space/paper-space mode switch.
**Exit criteria:** every row in `COMMAND_PARITY_CHECKLIST.md` marked "must replicate" is reachable and functional via the command bar.

## Phase G — Rendering and performance engine
Multi-threaded scene graph, async regeneration (large files never block the UI thread). GPU-resident geometry with LOD for large assemblies. Performance budget benchmarked against AutoCAD 2026's own claimed baselines; every merged feature gated against it.
**Exit criteria:** automated benchmark suite in CI; no regression merges without a passing perf gate.

## Phase H — Collaboration and cloud layer
Real-time multi-user editing (CRDT-based, not file locking). Markup anchored to geometry IDs, not pixel coordinates. Public roadmap board with exactly four states (Planned/Researching/Declined/Shipped) as a first-class product surface.
**Exit criteria:** two simultaneous editors on one sheet with no data loss under a conflict-injection test.

## Phase I — Scripting, automation, and the AI layer
Documented Python/JS API + legacy-AutoLISP importer with auto-translation. **Natural-language chat-edit layer** (the core new requirement): select an entity, type a plain-English instruction ("change to medium line weight," "make this line intersect the ground line at a 45 degree angle"), and have it parsed into a structured edit and executed immediately, with full manual-editing parity available at all times as the fallback/complement. Built as: (1) an intent-parsing layer translating natural language into the same structured command objects the command bar already emits — reusable, testable, inspectable; (2) CAD-specific trained models for the tasks that generic language understanding can't do alone (Smart-Blocks-equivalent detection, geometry-suggestion, placement prediction).
**Hard dependency flagged here, not before:** training the CAD-specific models requires provisioned cloud GPU compute and billing, which only the project owner can set up (account creation + payment cannot be done by this session). When this phase is reached, `STATUS.md` will state exactly what's needed and pause only that sub-track — the NL command layer itself does not require this and ships first.
**Exit criteria:** the two example instructions from the requirements ("change to medium line weight," "45 degree intersection with ground line") pass as automated tests, plus a broader NL command test suite covering every command category in the parity checklist.

## Phase J — Build the five groundbreaking features
In dependency order: (1) Living Constraint Graph (needs Phase D's entity dependency edges), (2) Time-Travel Drawing Diff (needs Phase D's append-only Drawing History), (3) Real-Time Code & Standards Compliance Layer (needs a rule-pack format + the constraint graph), (4) One-Click Fabrication Reality Check (needs the solid kernel + a DFM rule library), (5) Session Replay (needs the command/event log from Phase F).
**Exit criteria:** each feature has its own acceptance test suite, all passing.

## Phase K — Licensing model: totally free, forever
No subscription tier, no paid tier, no phone-home requirement. One installer, one license (permissive open license — see `LICENSE`), full functionality unlocked with no account required to use the software locally. This directly supersedes the original audit's "subscribe-or-buy-perpetual" proposal — the actual requirement is stronger: free only, no purchase path at all.
**Exit criteria:** a clean install on each platform runs 100% of features with zero network calls required and zero payment prompts anywhere in the product.

## Phase L — Cross-platform packaging
Windows: signed MSI/MSIX, tested on Windows 10 and 11. macOS: single universal (Intel + Apple Silicon) notarized .dmg/.pkg, no Rosetta dependency. Linux: Flatpak (primary) + .deb + .rpm, tested against a current LTS distro from each major family.
**Exit criteria:** all three installers built and smoke-tested by CI on every release candidate.

## Phase M — Testing, QA, accessibility, performance
Crash-corpus regression suite (thousands of real and synthetic drawings) as a hard release gate — no release ships that newly crashes on a previously-passing file. Automated accessibility audits on every UI surface. Fuzz-testing on all file-format importers, especially DWG.
**Exit criteria:** zero open crash-class regressions; accessibility audit passes on every screen.

## Phase N — Documentation, onboarding, migration
Every command's help entry ships an embedded interactive demo before that command is "done." DWG-import fidelity report shown per file. Guided first-hour tutorial teaches the unified-canvas sheet model before anything else.
**Exit criteria:** 100% command coverage in the help system; tutorial completion test with a non-CAD-background reviewer.

## Phase O — Beta and feedback loop
Staged beta across architecture, mechanical, and civil workflows (the three verticals most represented in the pain-point research). Every beta request feeds the public roadmap board from day one.
**Exit criteria:** beta cohort completes real production drawings with no unresolved blocking bugs.

## Phase P — Launch and continuous governance
Ship 1.0 only once every row in `PAIN_POINTS_AND_REQUIREMENTS.md` and `COMMAND_PARITY_CHECKLIST.md` is marked complete. Commit to one architecture review per major version as published policy. Re-run the full pain-point audit method against Curiosity itself 12 months post-launch.
**Exit criteria:** 100%-completion bar in `README.md` fully satisfied, verified row by row, nothing marked deferred.
