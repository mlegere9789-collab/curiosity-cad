# Pain Points & Requirements — Numbered Acceptance Criteria

Status legend: `[ ]` not started · `[~]` in progress · `[x]` done and verified

## A. AutoCAD pain points — 100% must be resolved (not 10%)

1. `[ ]` **Brutal learning curve.** Fix: unified command palette with plain-language + exact-match resolution, guided-lesson onboarding shipped in-product. (Phase F, N)
2. `[ ]` **Redundant primitives** (line/polyline/spline/ray/construction line all draw "a line"). Fix: one canonical entity per primitive; legacy names are routed aliases, not separate types. (Phase D)
3. `[ ]` **Decades of accreted, never-deprecated tools.** Fix: single reviewed core engine, one entity model from day one; aliases documented, not separately maintained code paths. (Phase D)
4. `[ ]` **Command line stuck in the 1980s / dyslexia accessibility barrier.** Fix: modernized command bar — type-ahead, live autocomplete, plain-language input, phonetic matching, full screen-reader support, built in from the start. (Phase F)
5. `[ ]` **Undo also undoes your view, destroying redo history.** Fix: two fully separate append-only history stacks (Drawing History, View History) from the architecture up. (Phase D)
6. `[ ]` **Dynamic-block authoring is unintuitive even for programmers.** Fix: visual, direct-manipulation authoring with live-preview parameter binding; definitions stored as inspectable, diffable data. (Phase F)
7. `[ ]` **Model space vs. paper space confusion.** Fix: one continuous canvas with sheet frames pinned onto it — no mode switch, no separate coordinate system to learn. (Phase F)
8. `[ ]` **AutoLISP/DIESEL are opaque.** Fix: modern documented Python + JS API from launch, live in-app console/autocomplete; legacy AutoLISP auto-translated on import. (Phase I)
9. `[ ]` **Single-threaded performance ceiling.** Fix: multi-threaded, GPU-accelerated core on one shared graphics abstraction; performance scales with cores/GPU. (Phase G)
10. `[ ]` **2026-style crashes/instability.** Fix: crash-corpus regression suite gating every release; no release ships that newly crashes on a previously-passing file. (Phase M)
11. `[ ]` **DWG lock-in / no perpetual license / subscription cost.** Fix: full native DWG read/write, no warning banner, and — per the strengthened requirement — the product itself is simply free forever, no subscribe/buy choice needed. (Phase E, K)
12. `[ ]` **Web app trails desktop.** Fix: one engine compiled to native + WASM; same codebase, not a lighter reimplementation. (Phase B, H)
13. `[ ]` **Feature-request system is theater.** Fix: public roadmap board, exactly four states, nothing in unstated limbo. (Phase H)
14. `[ ]` **Learning material reads like documentation, not instruction.** Fix: every command's help entry ships an embedded interactive demo. (Phase N)
15. `[ ]` **Features silently disabled across version boundaries.** Fix: explicit visible compatibility manifest on file open. (Phase E)
16. `[ ]` **Iterative for 20+ years, never rethought.** Fix: one full architecture review per major version, published. (Phase P, ongoing)
17. `[ ]` **Customer support quality complaints.** Fix: N/A in the original commercial-support sense (Curiosity is free/community software) — superseded by requirement 24 (documentation quality) and open governance (13). Tracked as resolved via community support model, not paid support.

## B. Tallied fix requests (Part Three of the source audit) — cross-check, no gaps

18. `[ ]` Fix 2026-specific-crash-class instability before/alongside AI features → covered by #10.
19. `[ ]` Modernize the interaction model instead of only layering AI on top → covered by #4, #6, #7.
20. `[ ]` Stop undo discarding redo history → covered by #5.
21. `[ ]` Consolidate redundant commands/object types → covered by #2.
22. `[ ]` Simplify dynamic block authoring → covered by #6.
23. `[ ]` True multi-threaded performance → covered by #9.
24. `[ ]` Perpetual license or materially cheaper subscription → superseded/exceeded by #11 (fully free).
25. `[ ]` Close web/desktop gap → covered by #12.
26. `[ ]` Rework model space/paper space workflow → covered by #7.
27. `[ ]` Loosen DWG lock-in signals → covered by #11.
28. `[ ]` Actually act on community-voted feature requests → covered by #13.
29. `[ ]` Write onboarding material that teaches, not documents → covered by #14.
30. `[ ]` Stop quietly disabling features across version boundaries → covered by #15.

## C. Additional requirements set directly by the project owner (beyond the source audit)

31. `[ ]` **Feature/command parity:** every documented AutoCAD 2026 feature and capability (1500+ commands/variables/capabilities per the source audit's own count) must be present in Curiosity, independently built/researched — not copied. Tracked row-by-row in `COMMAND_PARITY_CHECKLIST.md` and `FEATURE_CATALOG.md`.
32. `[ ]` **AI integration parity:** every AI feature AutoCAD 2026 has (Autodesk Assistant, Smart Blocks x5, My Insights, Activity Insights + "What's Changed", Markup Import & Assist x2) must have a Curiosity equivalent, independently built.
33. `[ ]` **Natural-language chat-edit layer:** select an element, type a plain-English instruction, have it executed immediately. Explicit acceptance examples:
    - `[ ]` "change to medium line weight" (with a line selected) → lineweight property updated, no manual dialog needed.
    - `[ ]` "make sure this line intersects with the ground line at a 45 degree angle" (with a line selected) → geometry recalculated/constrained automatically to satisfy the stated angle relationship.
    - `[ ]` Full manual editing (direct manipulation, dialogs, command bar) remains available at all times as an equal, non-degraded path — the NL layer is additive, never a replacement requirement.
34. `[ ]` **AI built/trained in-house:** the NL intent layer and CAD-specific models (Smart-Blocks-equivalent, geometry suggestion) are built and trained by this project, not a thin wrapper around a third-party product's proprietary features. (Generic language understanding may call an existing LLM API as an implementation detail; CAD-specific models are trained on project-collected data.) See Phase I dependency note on GPU compute/billing.
35. `[ ]` **Five groundbreaking features**, tracked individually in `CURIOSITY_ANSWERS.md` / Phase J: Living Constraint Graph, Time-Travel Drawing Diff, Real-Time Code & Standards Compliance Layer, One-Click Fabrication Reality Check, Session Replay for Support & Teaching.
36. `[ ]` **Cross-platform:** installable and fully functional on Windows, macOS, and Linux.
37. `[ ]` **Totally free:** no subscriptions, no paid tiers, no in-app purchases, ever.

---

**None of rows 1–37 may be marked `[x]` without a corresponding passing automated test or, where automation genuinely can't cover it (e.g. onboarding-material quality), a documented human-reviewer verification.** `STATUS.md` tracks the live completion percentage across this table.
