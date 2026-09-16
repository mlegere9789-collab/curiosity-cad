# Architectural Answers to Every Pain Point + Five Groundbreaking Features

This is the design commitment layer: for every row in `PAIN_POINTS_AND_REQUIREMENTS.md`, the specific architectural mechanism that closes it — checked off only when implemented and tested, not when merely designed.

## Pain point → architectural answer

| Pain point | Curiosity's answer | Phase |
|---|---|---|
| Brutal learning curve | Unified command palette (type or plain-language intent resolves to the right tool) + in-product guided-lesson onboarding track | F, N |
| Too many ways to draw a line | One canonical entity per primitive; legacy command names are routed aliases to the same entity type | D |
| Decades of accreted tools | Single reviewed core engine, one entity model from day one; aliases are documented, not separate code paths | D |
| Command line stuck in the 1980s | Modernized command bar: type-ahead, chainable/scriptable, live autocomplete, plain-language input, full screen-reader/dyslexia-friendly phonetic matching | F |
| Undo destroys redo history | Two fully separate append-only history stacks: Drawing History and View History | D |
| Dynamic-block authoring unintuitive | Visual, direct-manipulation authoring (drag a dimension, see states generate); definitions stored as inspectable, diffable data | F |
| Model space / paper space confusion | One continuous canvas with sheet frames pinned onto it — what you see is what prints | F |
| AutoLISP/DIESEL opaque | Modern documented Python + JS API from launch, live console/autocomplete, auto-translated AutoLISP import | I |
| Single-threaded performance ceiling | Multi-threaded, GPU-accelerated core on one shared graphics abstraction (wgpu) | G |
| 2026-style crashes/instability | Crash-corpus regression suite gating every release | M |
| DWG lock-in / subscription cost | Full native DWG read/write, no warning banner; product itself is free forever (stronger than "perpetual license option") | E, K |
| Web app trails desktop | One engine compiled to native + WASM — same codebase | B, H |
| Feature-request system is theater | Public roadmap board, exactly 4 states (Planned/Researching/Declined/Shipped) | H |
| Learning material reads like documentation | Every command's help entry ships an embedded interactive demo | N |
| Features silently disabled across versions | Explicit visible compatibility manifest on file open | E |
| Iterative for 20+ years, never rethought | One full architecture review per major version, published with notes | P (ongoing) |
| Customer support quality complaints | Free/community model — superseded by documentation quality (N) + open governance (H) | N, H |

## The five groundbreaking features

1. **Living Constraint Graph** — every dimension, layer rule, and standard in a drawing is a visible, queryable node in one graph. "What breaks if I make this wall 6 inches thicker?" highlights every downstream object, dimension, and clash before commit, across the whole sheet set. *Depends on:* Phase D entity dependency edges.
2. **Time-Travel Drawing Diff** — scrub a slider across a drawing's entire history like a video timeline, watching geometry/blocks/annotations appear/move/disappear in place; fork a new branch from any past moment without losing current state. *Depends on:* Phase D append-only Drawing History.
3. **Real-Time Code & Standards Compliance Layer** — local building-code/industry-standard rule packs (egress widths, clearance radii, electrical spacing) run continuously in the background, flagging violations inline the instant they occur — not a manual post-hoc pass, not a paid module. *Depends on:* rule-pack format + the constraint graph.
4. **One-Click Fabrication Reality Check** — any 3D solid checked instantly against a chosen manufacturing process (CNC mill, sheet-metal brake, 3D print, laser cut), flagging undercuts/minimum wall thickness/tolerance stack-up directly on the model. *Depends on:* solid-modeling kernel + a DFM rule library.
5. **Session Replay for Support & Teaching** — every session scrubbable/replayable as live, re-editable vector data (not video); a replay link lets support or a teammate pause at your exact stuck point and take over with permission. *Depends on:* the command/event log the unified command bar already produces.

## Natural-language chat-edit layer (owner-added requirement, not in the source audit)

Architecture: the NL layer is a **front end**, not a separate editing path — it parses a plain-English instruction into the exact same structured command objects the command bar and manual-editing UI already emit and execute. This guarantees:
- Every NL instruction is exactly as capable as its manual-command equivalent (no second-class "AI mode").
- Every NL instruction is inspectable/undoable through the same Drawing History as a manual edit.
- Manual editing is never degraded or hidden in favor of the chat layer — both are always-on, equal paths.

Two concrete acceptance tests (owner-specified, must pass before Phase I closes):
- Select a line, type "change to medium line weight" → lineweight property set, no dialog required.
- Select a line, type "make sure this line intersects with the ground line at a 45 degree angle" → geometry solved/constrained automatically to satisfy the angle relationship against the referenced "ground line" entity.
