using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Curiosity.Plugin.NlParser;

namespace Curiosity.Plugin.Execution
{
    /// <summary>
    /// The single execution path for an EditIntent, regardless of whether it was resolved by
    /// LocalPatternMatcher or LlmFallbackClient. Applies changes inside a normal AutoCAD Transaction,
    /// so every NL-driven edit is a standard, undoable operation — never a special-cased "AI edit."
    ///
    /// NOTE: written against the documented AutoCAD .NET API shape from spec knowledge; has not yet
    /// been compiled or run against a real AutoCAD session. First real build/run is the next step
    /// tracked in STATUS.md.
    /// </summary>
    public static class CommandExecutor
    {
        // Named mapping table per docs/INTENT_SCHEMA.md — the single place to adjust if "medium" etc.
        // don't match classmates' real-world expectations.
        private static readonly Dictionary<string, LineWeight> LineweightMap = new()
        {
            ["LineWeight025"] = LineWeight.LineWeight025,
            ["LineWeight035"] = LineWeight.LineWeight035,
            ["LineWeight060"] = LineWeight.LineWeight060,
        };

        public static void Execute(EditIntent intent, Document doc)
        {
            if (intent.Action == IntentAction.Unrecognized)
            {
                doc.Editor.WriteMessage("\nCuriosity: didn't recognize that instruction — try rephrasing.\n");
                return;
            }

            using var tr = doc.TransactionManager.StartTransaction();
            try
            {
                switch (intent.Action)
                {
                    case IntentAction.SetProperty:
                        ExecuteSetProperty(intent, doc, tr);
                        break;
                    case IntentAction.ConstrainAngle:
                        ExecuteConstrainAngle(intent, doc, tr);
                        break;
                    case IntentAction.RunMacro:
                        Macros.MacroRegistry.Run((string)intent.Parameters["name"], doc, tr);
                        break;
                    default:
                        doc.Editor.WriteMessage($"\nCuriosity: '{intent.Action}' not implemented yet.\n");
                        return;
                }

                tr.Commit();
            }
            catch (Exception ex)
            {
                tr.Abort();
                doc.Editor.WriteMessage($"\nCuriosity: edit failed — {ex.Message}\n");
                throw;
            }
        }

        private static void ExecuteSetProperty(EditIntent intent, Document doc, Transaction tr)
        {
            var property = (string)intent.Parameters["property"];
            var value = (string)intent.Parameters["value"];

            foreach (var id in GetTargetEntityIds(intent.Target, doc, tr))
            {
                var entity = (Entity)tr.GetObject(id, OpenMode.ForWrite);

                switch (property)
                {
                    case "Lineweight":
                        entity.LineWeight = LineweightMap[value];
                        break;
                    case "Color":
                        entity.ColorIndex = int.Parse(value);
                        break;
                    case "Layer":
                        entity.Layer = value;
                        break;
                    default:
                        throw new NotSupportedException($"Property '{property}' not yet supported.");
                }
            }
        }

        private static void ExecuteConstrainAngle(EditIntent intent, Document doc, Transaction tr)
        {
            var degrees = Convert.ToDouble(intent.Parameters["degrees"]);
            var reference = (EntityReference)intent.Parameters["reference"];

            foreach (var id in GetTargetEntityIds(intent.Target, doc, tr))
            {
                if (tr.GetObject(id, OpenMode.ForWrite) is not Line targetLine)
                {
                    doc.Editor.WriteMessage("\nCuriosity: angle constraint currently only supports Line entities.\n");
                    continue;
                }

                var referenceLine = FindNamedReferenceLine(reference.Name, doc, tr);
                if (referenceLine == null)
                {
                    doc.Editor.WriteMessage($"\nCuriosity: couldn't find a reference entity matching '{reference.Name}'.\n");
                    continue;
                }

                GeometrySolver.RotateToAngleFromReference(targetLine, referenceLine, degrees);
            }
        }

        /// <summary>
        /// Resolves an EntityReference to concrete ObjectIds. "selection" uses the current AutoCAD
        /// pickfirst selection; "named" looks up an entity by a matching xdata/description tag
        /// (see GeometrySolver's reference-lookup convention, still to be finalized against how
        /// classmates actually label things like "the ground line" in real drawings).
        /// </summary>
        private static IEnumerable<ObjectId> GetTargetEntityIds(EntityReference target, Document doc, Transaction tr)
        {
            if (target.Kind == "selection")
            {
                var result = doc.Editor.SelectImplied();
                if (result.Status == PromptStatus.OK)
                {
                    foreach (var id in result.Value.GetObjectIds())
                        yield return id;
                }
            }
            // "named" / "nearest" target resolution for the primary target (as opposed to a reference
            // entity) is not yet implemented — tracked in STATUS.md.
        }

        private static Line? FindNamedReferenceLine(string? name, Document doc, Transaction tr)
        {
            // Lookup-by-name strategy (xdata tag vs. layer name vs. nearest-labeled-entity heuristic)
            // is intentionally not finalized yet — needs real classmate drawings to decide against,
            // not a guess. Tracked in STATUS.md as a Phase 1 decision.
            throw new NotImplementedException(
                "Named reference entity lookup (e.g. 'the ground line') needs a real-drawing-informed design decision before implementation.");
        }
    }
}
