# Status

**Current phase:** A — Lock the spec
**Overall completion: 0%** (Phase A scaffolding in progress; no application code yet)
**Last updated:** 2026-09-16

## What's done
- Repo created and structured.
- `README.md` — 100% completion bar quoted verbatim, locked.
- `docs/BUILD_PLAN.md` — Phases A–P fully specified, including the NL chat-edit layer and free/no-subscription licensing folded in.
- `docs/PAIN_POINTS_AND_REQUIREMENTS.md` — 37 numbered acceptance criteria (17 pain points + 13 cross-checked tally items + 7 owner-added requirements incl. NL layer, in-house AI, cross-platform, free-forever).
- `docs/FEATURE_CATALOG.md` — first-tranche feature checklist (AI/automation, collaboration, 2D, 3D, file compat, customization, all 7 specialized toolsets).
- `docs/COMMAND_PARITY_CHECKLIST.md` — ~150 core commands + ~40 system variables, first tranche of the full 1500+ item target.
- `docs/CURIOSITY_ANSWERS.md` — pain-point → architecture mapping, the 5 groundbreaking features, and the NL layer's acceptance tests.

## Exact next action (always keep this section accurate — this is what "resume" means)
1. Finish Phase A: expand `COMMAND_PARITY_CHECKLIST.md` toolset-by-toolset (Architecture → Mechanical → Electrical → MEP → Plant 3D → Map 3D → Raster Design) to reconcile against the full ~1500-item AutoCAD command/variable surface, not just the ~150-command aliased core.
2. Triage every row in `COMMAND_PARITY_CHECKLIST.md` as `must replicate` / `must replicate + fix` / `superseded by [mechanism]`.
3. Run the 3 required QC passes over the full Phase A checklist set (per project-owner instruction: dogmatic, repeated QC).
4. Close Phase A; open Phase B (`docs/architecture/STACK_DECISION.md` — Rust/wgpu/egui-or-Slint/PyO3 decision, justified against the parity + performance + web-parity requirements).
5. Phase C: scaffold the Cargo workspace (`/core /render /ui /io /script /ai /web /installers`) and cross-platform CI.

## Flagged dependency (not a blocker yet, will matter at Phase I and part of Phase E)
- **DWG fidelity (Phase E):** needs a licensed SDK or a long-horizon from-scratch reader/writer; decision recorded in `docs/architecture/DWG_STRATEGY.md` when Phase E opens.
- **CAD-specific model training (Phase I):** needs project-owner-provisioned cloud GPU compute + billing account. The NL command layer itself does not wait on this — only the trained-model sub-track (Smart-Blocks-equivalent detection, geometry suggestion) does.

## Ground rules for whoever/whatever resumes this project
- Never mark a row in any checklist `[x]` without a passing automated test (or documented human review where automation can't cover it).
- Never reduce scope on any of the 37 requirements without the project owner explicitly reopening that specific requirement.
- Always update this file's "exact next action" before ending a session/phase, so the next session needs zero re-explanation.
