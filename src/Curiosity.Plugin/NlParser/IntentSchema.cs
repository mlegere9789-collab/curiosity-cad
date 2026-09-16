using System.Collections.Generic;

namespace Curiosity.Plugin.NlParser
{
    /// <summary>
    /// The single contract between "how an instruction was understood" (either parser tier)
    /// and "what happens in the drawing" (CommandExecutor). See docs/INTENT_SCHEMA.md.
    /// </summary>
    public enum IntentAction
    {
        SetProperty,
        ConstrainAngle,
        ConstrainParallel,
        ConstrainPerpendicular,
        Transform,   // general rotate/move/scale — the "make any edit" catch-all until a more
                     // specific action earns its own case, per the project owner's ask that this
                     // not be limited to lineweight-style property edits
        RunMacro,
        Unrecognized
    }

    public sealed class EntityReference
    {
        /// <summary>"selection" | "named" | "nearest"</summary>
        public string Kind { get; }
        public string? Name { get; }

        public EntityReference(string kind, string? name = null)
        {
            Kind = kind;
            Name = name;
        }

        public static readonly EntityReference CurrentSelection = new EntityReference("selection");
    }

    public sealed class EditIntent
    {
        public IntentAction Action { get; }
        public EntityReference Target { get; }
        public IReadOnlyDictionary<string, object> Parameters { get; }
        public double Confidence { get; }

        /// <summary>"local" | "llm" — logged so the local matcher's phrasing set can grow from real fallback cases.</summary>
        public string ResolvedBy { get; }

        public EditIntent(
            IntentAction action,
            EntityReference target,
            IReadOnlyDictionary<string, object> parameters,
            double confidence,
            string resolvedBy)
        {
            Action = action;
            Target = target;
            Parameters = parameters;
            Confidence = confidence;
            ResolvedBy = resolvedBy;
        }

        public static readonly EditIntent NotRecognized = new EditIntent(
            IntentAction.Unrecognized,
            EntityReference.CurrentSelection,
            new Dictionary<string, object>(),
            0.0,
            "none");
    }
}
