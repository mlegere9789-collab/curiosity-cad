# Feature Catalog — AutoCAD 2026 Parity Checklist

Source: AutoCAD 2026 official feature docs + What's New (2026/2026.1/2026.1.1/2026.1.2), as compiled in the source audit. Every row must reach an independently-built Curiosity equivalent (not a copy — see `PAIN_POINTS_AND_REQUIREMENTS.md` #31–32). Status: `[ ]` not started.

## AI & automation
- `[ ]` Autodesk Assistant equivalent — conversational panel answering "how do I…" without leaving the drawing. → subsumed/exceeded by Phase I's NL chat-edit layer.
- `[ ]` Smart Blocks: Search & Convert — detect repeated/near-match geometry, convert to block instances with attribute detection.
- `[ ]` Smart Blocks: Detect & Convert — proposes block conversions for ungrouped similar geometry (PDF/legacy cleanup).
- `[ ]` Smart Blocks: Placement — predicts block placement/scale from prior usage.
- `[ ]` Smart Blocks: Replacement — ML-suggested block swap.
- `[ ]` Smart Blocks: Layer 0 assignment on block conversion.
- `[ ]` My Insights — personalized tips/macros/workflow suggestions from usage patterns.
- `[ ]` Activity Insights — 35+ tracked activity types, version history, file comparison, local/cloud/third-party.
- `[ ]` "What's Changed" insight — diff summary between two saved states.
- `[ ]` Markup Import & Markup Assist — AI reads handwritten paper/PDF markup instructions (MOVE/COPY/DELETE) and applies them.
- `[ ]` Markup Import & Assist with cloud docs — JPG/PNG/PDF markups synced from cloud storage, Trace-layer actionable.

## Collaboration & cloud
- `[ ]` Connected/shared support files across a team project (fonts, plot styles, hatch patterns).
- `[ ]` Push-to-cloud-docs publishing (drawing sheets → PDF).
- `[ ]` Trace layer — review/markup without altering underlying drawing.
- `[ ]` Cross-platform anytime/anywhere — same file opens on desktop, web, mobile.
- `[ ]` Multi-user simultaneous markup via shared link, no locking/duplication.
- `[ ]` DWG Compare — overlay two drawing versions, color-coded diff.
- `[ ]` Xref Compare — version-comparison extended to externally referenced drawings.
- `[ ]` Shared Views — publish a browser-viewable link, no license required to view/comment.
- `[ ]` Sheet Set Manager — organize/batch-plot multi-drawing sets; cloud-hosted with conflict notification.
- `[ ]` Save-to-web-and-mobile with external references intact.
- `[ ]` Geospatial basemaps (satellite/street/monochrome) as drawing backdrop.
- `[ ]` Geographic location assignment + online map overlay.

## 2D drafting, drawing & annotation
- `[ ]` Arrays — rectangular, polar, path-based.
- `[ ]` Center marks & centerlines — auto-attached, layer-forceable.
- `[ ]` Data extraction — object/attribute/count data → table.
- `[ ]` Data linking — live two-way link to spreadsheet.
- `[ ]` Dimensions — hover-to-generate with live preview.
- `[ ]` Dynamic blocks — stretch/rotate/array/flip/visibility-state parameters.
- `[ ]` Fields — auto-updating text placeholders.
- `[ ]` Hatch — pattern/solid/gradient fill, including open-path fill.
- `[ ]` Layouts — paper-space sheets with title block, scaled viewports, print settings.
- `[ ]` Leaders (MLeader) — annotation lines with configurable styles, layer-forceable.
- `[ ]` Parametric constraints — geometric/dimensional consistency rules.
- `[ ]` Purge — remove unused named objects with preview.
- `[ ]` Revision clouds — layer-forceable, scale-mode-controllable.
- `[ ]` Tables — data/formula grid objects, Excel-linkable, layer-forceable.
- `[ ]` Text (Mtext) & text settings — multi/single-line, column layout, layer-forceable.
- `[ ]` Views — named, savable camera positions.
- `[ ]` Viewports (layout) — layer-forceable.
- `[ ]` Count — auto-count instances into a field/table.

## 3D modeling & visualization
- `[ ]` 3D navigation — Orbit/ViewCube/SteeringWheel equivalents.
- `[ ]` Model documentation — auto-generate 2D base/projected/section/detail views from 3D.
- `[ ]` Point clouds — laser-scan data as design reference.
- `[ ]` Rendering — lighting/materials for presentation-quality image.
- `[ ]` Cloud rendering — offload render job.
- `[ ]` Section planes — live cross-section cutting.
- `[ ]` Solid, surface & mesh modeling — all three approaches.
- `[ ]` Visual styles — wireframe/hidden/shaded/conceptual/realistic presets.
- `[ ]` Fast-shaded performance mode for large-model navigation.

## File compatibility
- `[ ]` DGN import/export/underlay (MicroStation format).
- `[ ]` PDF import (vector/text/image) + underlay/export.
- `[ ]` Federated model references / cross-application geometry import.
- `[ ]` File-trust/fidelity signal — Curiosity's version: validated fidelity report, no discouraging warning banner (see Pain Points #11, #15).

## Customization & installation
- `[ ]` Embedded scripting language + IDE (AutoLISP-equivalent — see Phase I, Python/JS).
- `[ ]` Action Recorder — record/replay command macros.
- `[ ]` Full API surface for programmatic drawing/database control (documented Python/JS/native bindings).
- `[ ]` Update/patch delivery without interrupting an open session.
- `[ ]` Floating windows — multi-monitor drawing tabs.
- `[ ]` CAD Standards Checker — naming/style drift detection.
- `[ ]` UI customization — rebuildable command bar/panel layout.
- `[ ]` Secure Load — restrict which executables/add-ons/scripts can run.
- `[ ]` Simplified/fleet installer.
- `[ ]` Start tab — recent files, templates, learning/insights hub.
- `[ ]` System Variable Monitor — drift-from-standard alerts.
- `[ ]` Extension/plugin marketplace equivalent.
- `[ ]` Native Apple Silicon support (no translation layer).
- `[ ]` Web-app feature parity with desktop (see Pain Points #12).
- `[ ]` System-printer plot compatibility enhancements.
- `[ ]` GPU-accelerated text rendering, including CJK/RTL scripts.
- `[ ]` Asynchronous raster image loading (non-blocking file open).
- `[ ]` Optional, non-intrusive usage-feedback survey.

## Specialized toolsets (must all be present, not bundled-for-pay)
- `[ ]` Architecture toolset — walls/doors/windows/roofs, auto schedules, code-driven behavior.
- `[ ]` Mechanical toolset — standards-based parts library, BOM generation, structural steel shapes.
- `[ ]` Electrical toolset — schematic symbol libraries, circuit/wiring tools, automated error checking.
- `[ ]` Map 3D toolset — GIS data handling, spatial database connectivity.
- `[ ]` MEP toolset — mechanical/electrical/plumbing objects, auto sizing/coordination.
- `[ ]` Plant 3D toolset (incl. P&ID) — piping/instrumentation diagrams, 3D plant catalogs.
- `[ ]` Raster Design toolset — scanned raster-to-vector conversion.

---
Cross-reference: `COMMAND_PARITY_CHECKLIST.md` for the command/alias/system-variable level detail underlying the 2D/3D/customization rows above.
