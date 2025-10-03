using System;
using System.Collections.Generic;
using UnityEngine;

namespace PyroLab.VFXResearch
{
    /// <summary>
    /// Serializable record of a single experiment that was executed as part of the VFX Graph
    /// feasibility study. The record is intentionally lightweight so that designers can review
    /// each test directly inside the Unity inspector while exploring the demo scene.
    /// </summary>
    [Serializable]
    public class VFXResearchTest
    {
        [Tooltip("Short identifier that matches the research question bullet point.")]
        public string id = string.Empty;

        [Tooltip("Concise description of the hypothesis or expected capability under test.")]
        [TextArea(2, 4)]
        public string goal = string.Empty;

        [Tooltip("How the experiment is configured inside the demo scene.")]
        [TextArea(2, 6)]
        public string setup = string.Empty;

        [Tooltip("Primary observation gathered while running the experiment inside Play mode.")]
        [TextArea(2, 6)]
        public string observation = string.Empty;

        [Tooltip("Follow up actions or blockers that should be tracked after this research sprint.")]
        [TextArea(1, 4)]
        public string followUp = string.Empty;

        [Tooltip("High level outcome that summarises the viability of the feature under test.")]
        public VFXResearchOutcome outcome = VFXResearchOutcome.Unknown;
    }

    /// <summary>
    /// Groups related test cases under a single research question so that the inspector presents
    /// a clean hierarchy of the investigation. Each group mirrors the structure that the design
    /// brief uses (URP compatibility, performance, etc.).
    /// </summary>
    [Serializable]
    public class VFXResearchQuestionSet
    {
        [Tooltip("Descriptive title matching the investigation question from the brief.")]
        public string title = string.Empty;

        [Tooltip("Extra context or notes explaining how to review the set of experiments.")]
        [TextArea(2, 6)]
        public string notes = string.Empty;

        [SerializeField]
        private List<VFXResearchTest> tests = new();

        /// <summary>
        /// Provides read-only access to the registered test cases. Unity serialisation keeps the
        /// backing list so the inspector can still reorder entries when needed.
        /// </summary>
        public IReadOnlyList<VFXResearchTest> Tests => tests;
    }

    /// <summary>
    /// Entry point component that is attached to helper objects in the demo scene. It exposes
    /// the findings for each research question directly in the inspector which doubles as the
    /// on-site documentation for reviewers.
    /// </summary>
    public class VFXResearchNotebook : MonoBehaviour
    {
        [SerializeField]
        private List<VFXResearchQuestionSet> questionSets = new();

        /// <summary>
        /// Enumerates the structured list of question sets captured during the feasibility study.
        /// </summary>
        public IReadOnlyList<VFXResearchQuestionSet> QuestionSets => questionSets;
    }

    /// <summary>
    /// Enum representing the qualitative result of an experiment.
    /// </summary>
    public enum VFXResearchOutcome
    {
        Unknown = 0,
        Supported = 1,
        Unsupported = 2,
        NeedsWork = 3,
    }
}
