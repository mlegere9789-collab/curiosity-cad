using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
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
                    case IntentAction.Transform:
                        ExecuteTransform(intent, doc, tr);
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
        /// General rotate/move/scale, the catch-all for edits that aren't a simple property change
        /// or an angle-to-reference constraint. Parameters (per docs/INTENT_SCHEMA.md, extended):
        /// {"operation": "Rotate", "degrees": &lt;double&gt;} — rotates about the entity's own centroid
        ///   (geometric extents center), positive = counterclockwise, matching how a person says
        ///   "rotate 10 degrees" without specifying a pivot.
        /// {"operation": "Move", "dx": &lt;double&gt;, "dy": &lt;double&gt;} — translates in drawing units.
        /// {"operation": "Scale", "factor": &lt;double&gt;} — scales about the entity's own centroid.
        /// </summary>
        private static void ExecuteTransform(EditIntent intent, Document doc, Transaction tr)
        {
            var operation = (string)intent.Parameters["operation"];

            foreach (var id in GetTargetEntityIds(intent.Target, doc, tr))
            {
                var entity = (Entity)tr.GetObject(id, OpenMode.ForWrite);
                var center = entity.GeometricExtents.MinPoint +
                             (entity.GeometricExtents.MaxPoint - entity.GeometricExtents.MinPoint) * 0.5;

                Matrix3d matrix = operation switch
                {
                    "Rotate" => Matrix3d.Rotation(
                        Convert.ToDouble(intent.Parameters["degrees"]) * Math.PI / 180.0,
                        Vector3d.ZAxis,
                        center),
                    "Move" => Matrix3d.Displacement(new Vector3d(
                        Convert.ToDouble(intent.Parameters["dx"]),
                        Convert.ToDouble(intent.Parameters["dy"]),
                        0)),
                    "Scale" => Matrix3d.Scaling(Convert.ToDouble(intent.Parameters["factor"]), center),
                    _ => throw new NotSupportedException($"Transform operation '{operation}' not supported.")
                };

                entity.TransformBy(matrix);
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

        /// <summary>
        /// First-cut lookup strategy, chosen for lowest friction: a "named" reference like "the
        /// ground line" matches any Line entity whose layer name contains one of the reference
        /// name's significant words (case-insensitive, ignoring "the"/"line"/etc.). This assumes
        /// classmates put reference geometry like a ground line on a sensibly-named layer (e.g.
        /// "Ground" or "Ground Line") rather than relying on drawing-order or xdata tags, which
        /// nobody sets up manually. Revisit once real classmate drawings show this assumption wrong.
        /// </summary>
        private static Line? FindNamedReferenceLine(string? name, Document doc, Transaction tr)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var stopWords = new HashSet<string> { "the", "a", "an", "line", "this" };
            var keywords = name
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.Trim().ToLowerInvariant())
                .Where(w => !stopWords.Contains(w))
                .ToList();

            if (keywords.Count == 0)
                return null;

            var blockTable = (BlockTable)tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);
            var modelSpace = (BlockTableRecord)tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead);

            foreach (ObjectId id in modelSpace)
            {
                if (tr.GetObject(id, OpenMode.ForRead) is not Line line)
                    continue;

                var layerRecord = (LayerTableRecord)tr.GetObject(line.LayerId, OpenMode.ForRead);
                var layerName = layerRecord.Name.ToLowerInvariant();

                if (keywords.Any(k => layerName.Contains(k)))
                    return line;
            }

            return null;
        }
    }
}
