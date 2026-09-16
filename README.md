# Curiosity

A free, forever-free, cross-platform (Windows/macOS/Linux) CAD/3D design application built to reach full functional parity with AutoCAD 2026 — every command, every toolset, every documented feature — while resolving 100% of AutoCAD's documented user-pain-points at the architecture level, adding a natural-language chat-based editing layer, and shipping five features nothing in the AutoCAD/Autodesk line has.

This repository is the single source of truth for that build. It is designed to be resumable across sessions with zero re-explanation: `STATUS.md` always states the current phase and the exact next action.

## The 100% completion bar (non-negotiable, set by project owner)

A build is not "100% done" until **every** item below is true. This is quoted verbatim from the requirements-setting conversation and must not be edited, softened, or reinterpreted:

> Every documented AutoCAD 2026 feature capability must be present in Curiosity (not copied — independently built, researched from how AutoCAD's features actually work, including all AI integration). 1500+ AutoCAD commands/variables/capabilities researched and matched. Every AutoCAD UI/UX/layout/organization/tool complaint and every commonly-mentioned user-unfriendliness must be fully fixed/resolved/addressed — 100% of AutoCAD pain points resolved, not 10%. All 5 groundbreaking features (Living Constraint Graph, Time-Travel Drawing Diff, Real-Time Code & Standards Compliance Layer, One-Click Fabrication Reality Check, Session Replay) implemented and functional. A natural-language chat-based editing system: select an element, type a plain-English instruction ("change to medium line weight," "make this line intersect the ground line at a 45 degree angle"), and have it executed immediately and automatically — alongside full manual editing parity with AutoCAD. Any AI models the product needs are built/trained by this project, not merely wrapped. Installable on Windows, macOS, and Linux. Totally free — no subscriptions, no paid tiers, ever.

Accepted explicitly: this will take years (7+ is fine). No scope reduction on any of the above is authorized without the project owner reopening the requirement themselves.

## Source specification

The full requirements are derived from `docs/source-audit/` (the original AutoCAD 2026 feature-and-pain-point audit) plus the requirements conversation. See:

- [`docs/BUILD_PLAN.md`](docs/BUILD_PLAN.md) — Phases A–P, the sequenced execution plan
- [`docs/FEATURE_CATALOG.md`](docs/FEATURE_CATALOG.md) — every AutoCAD 2026 feature Curiosity must independently reach parity on
- [`docs/COMMAND_PARITY_CHECKLIST.md`](docs/COMMAND_PARITY_CHECKLIST.md) — every core command/alias/system variable, tracked row by row
- [`docs/PAIN_POINTS_AND_REQUIREMENTS.md`](docs/PAIN_POINTS_AND_REQUIREMENTS.md) — every pain point as a numbered, testable acceptance criterion, plus the additional requirements (NL chat layer, licensing, platform)
- [`docs/CURIOSITY_ANSWERS.md`](docs/CURIOSITY_ANSWERS.md) — the architectural answer to each pain point, and the five groundbreaking features
- [`STATUS.md`](STATUS.md) — current phase, percent complete, exact next action

## Known hard dependencies (flagged early, not blockers to starting)

- **DWG read/write at real fidelity** needs either a licensed SDK (e.g. ODA Teigha) or a long-horizon from-scratch reverse-engineered reader/writer validated against a large real-file corpus. Tracked in `docs/BUILD_PLAN.md` Phase E.
- **Custom-trained CAD-specific models** (intent parsing, Smart-Blocks-equivalent detection) need provisioned cloud GPU compute and billing, which the project owner must set up when Phase I is reached. Flagged in `docs/BUILD_PLAN.md` Phase I.

## License

Free and open, forever. See `LICENSE`.
