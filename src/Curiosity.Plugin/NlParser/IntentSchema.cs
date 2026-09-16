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
        Select,          // build a selection from a description ("the wall layer", "all circles")
                         // and make it the active selection — lets you chain instructions without
                         // touching the mouse
        RunNativeCommand, // general fallback: translate to AutoCAD's own command-line syntax and
                          // run it via SendStringToExecute, for the long tail of the ~1500+ command
                          // surface that doesn't have (and may never get) a dedicated structured
                          // action. Only reliable for commands that act on a selection plus typed
                          // parameters — commands needing free-form mouse point-picking with no
                          // location described in the instruction are out of scope by nature, not
                          // a bug.
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
