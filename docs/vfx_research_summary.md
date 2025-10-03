# VFX Graph Feasibility Study

The `Demo_VFXResearch` scene organises a collection of miniature experiments that mirror the
feasibility questions from the research brief. Each GameObject in the scene hosts a
`VFXResearchNotebook` component so designers can review findings directly in the inspector without
leaving Play mode.

## Summary of Conclusions

| Topic | Key Findings |
| --- | --- |
| URP Compatibility | Bloom and HDR cooperate correctly when the URP asset enables HDR, and VFX Graph preserves color values above 1.0 for emissive gradients. |
| Performance & Limits | Comfortable GPU budget sits around 600k particles on a reference RTX 3070 with approximate FPS of 140/110/80/32 for 10k/50k/100k/500k particles. Multi-GPU execution is not supported. |
| Programmability | Gameplay code can drive launches by serialising `FireworkRecipe` data into event attributes. GraphicsBuffers feed burst patterns efficiently, and runtime property edits take effect instantly. |
| Effect Authoring | Trails, strobing, and multi-stage bursts are achievable with strip outputs, waveform modulation, and chained events respectively. |
| Integration | VFX Graph fireworks can coexist with Shuriken emitters. JSON recipes can configure graphs after a validation/mapper layer is introduced. |

## Using the Demo Scene

1. Open `Assets/Scenes/Demo_VFXResearch.unity`.
2. Select the **VFX Research Controller** object to inspect the organised question sets.
3. Each test entry explains the setup, observation, and follow-up tasks for future iterations.
4. Toggle Play mode to iterate on graph parameters while observing how the notebook captures live
   adjustments.

> **Note:** The findings capture qualitative behaviour for design sign-off. Replace the placeholder
> performance values with metrics from the target hardware once automated profiling is available.
