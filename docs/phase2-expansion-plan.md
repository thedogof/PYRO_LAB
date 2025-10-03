# PYRO_LAB Phase 2 Expansion Plan

> **Scope Reminder**: All mechanics described below are purely abstract gameplay and visualization systems. No part of this plan references or implies real-world pyrotechnic manufacturing.

## Vision Overview
- Embrace Japanese-inspired hanabi aesthetics (e.g., peony, chrysanthemum, willow, thousand-burst, double-bloom patterns).
- Highlight the craft fantasy through abstract parameters such as outer shell layers, star arrangement, fuse timing, and effect modifiers.
- Support a light-weight economy loop where players acquire materials, assemble recipes, test fireworks in the demo scene, and sell results to progress.

## Milestone Breakdown
The second-phase roadmap is split into three feature branches. Each branch should be independently reviewable and merged in order.

### A. Workshop & Material System (`feature/workshop-materials`)
**PR Title**: `Add abstract workshop & material system`

**Objectives**
1. **Material Database**
   - Implement a ScriptableObject or JSON-driven registry of abstract materials.
   - Each material only exposes gameplay parameters:
     - `outerShellLayer` (int 1–5): thicker shells produce rounder, longer-lasting bursts.
     - `colorSet` (gradient/palette reference): used when composing `FireworkLayer` gradients.
     - `twinkleFactor` (0–1): drives `TwinkleModifier` intensity.
     - `trailFactor` (0–1): scales `TrailModifier` length.
   - Prepare editor tooling to add/remove materials and assign categories (paper, color powder, special effect powder).
2. **Workshop UI Skeleton**
   - Add a demo scene canvas with controls to choose shell layers, color palettes, and effect powders.
   - Selected options generate a `FireworkRecipe` preview asset in memory and push changes to the existing spawner.
3. **Serialization Hooks**
   - Extend the JSON import/export utilities to capture workshop selections alongside recipes for easy sharing.

**Deliverables**
- Data definitions for material entries.
- Basic workshop interface in the demo scene.
- Updated JSON schema and example presets.

### B. Assembly & Crafting Flow (`feature/assembly-flow`)
**PR Title**: `Add crafting assembly flow (visual recipe builder)`

**Objectives**
1. **Step-based Builder UI**
   - Introduce a staged interface guiding players through: outer shell selection → star arrangement → modifiers.
   - Provide visual thumbnails or text descriptors for arrangements such as sphere shell, ring, heart, willow, double bloom.
   - Link selections directly to `FireworkLayer` and `VisualModifier` instances.
2. **Live Craft Visualization**
   - Display crafting feedback in the demo scene (e.g., thicker shells yield smoother bursts, fewer layers create scattered results).
   - Optionally overlay debug widgets that describe the assembled layers and modifiers.
3. **TimingTrack Enhancements**
   - Allow scheduling of double-burst events or color shifts using interactive timeline controls.

**Deliverables**
- Scriptable definitions (e.g., `FireworkLayerTemplate`) to map assembly choices into recipes.
- UI prefabs for the stepper workflow.
- Integration with the existing timing/preview system.

### C. Economy & Growth (`feature/economy-system`)
**PR Title**: `Add abstract economy & progression system`

**Objectives**
1. **Material Pricing & Inventory**
   - Assign in-game currency costs to each material category.
   - Track player balance, owned materials, and consumption during crafting.
2. **Quality Scoring & Sales**
   - After testing a firework, compute a quality score using visual metrics (e.g., symmetry, duration, modifier usage).
   - Convert quality into sale price and update player balance.
3. **Contract Missions**
   - Author lightweight contracts (ScriptableObject or JSON) describing requested features (e.g., “willow + double bloom + gold hue”).
   - Validate crafted recipes against contract requirements to unlock payouts.
4. **Progression Curve**
   - Gate advanced materials behind milestones (e.g., total revenue, contract completions).
   - Communicate progression tiers: early (basic spheres), mid (double bloom, trails, heart shapes), late (large-scale shells).

**Deliverables**
- Currency & inventory scripts.
- Contract definition assets and validation logic.
- Progression configuration and UI feedback for unlocks.

## Demo Scene Expectations
- Provide an accessible UI flow that demonstrates buying materials, assembling components, testing fireworks, and selling the result.
- Ensure all descriptions, tooltips, and labels reiterate the purely visual, game-development intent.

## Documentation Checklist
- Update `README.md` and relevant in-project tooltips to emphasize the abstract nature of the systems.
- Include usage notes for new editors/UI elements.
- Supply JSON or ScriptableObject samples for materials, recipes, and contracts.

## Testing Guidance
- Manual playtest in the demo scene covering: material selection, recipe assembly, double-burst timing, economy transactions, and contract fulfillment.
- Validate JSON import/export across all new data models.

## Safety & Compliance
- Maintain the project-wide disclaimer: **visual simulation only—no real-world fabrication guidance**.
- Avoid any real chemical names or hazardous procedures in assets, code, or documentation.

