using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace Curiosity.Plugin.NlParser
{
    /// <summary>
    /// Builds the EntityContext the LLM fallback tier needs (selected entity type/properties +
    /// candidate reference-entity names) from AutoCAD's live selection state.
    /// </summary>
    public static class SelectionContext
    {
        public static EntityContext CaptureCurrent(Document doc)
        {
            var context = new EntityContext();

            var result = doc.Editor.SelectImplied();
            if (result.Status != PromptStatus.OK || result.Value.Count == 0)
                return context;

            using var tr = doc.TransactionManager.StartTransaction();
            var entity = (Entity)tr.GetObject(result.Value[0].ObjectId, OpenMode.ForRead);

            context.EntityType = entity.GetType().Name;
            context.Properties["Layer"] = entity.Layer;
            context.Properties["ColorIndex"] = entity.ColorIndex;
            context.Properties["LineWeight"] = entity.LineWeight.ToString();

            // Candidate reference-entity names (e.g. things a user might call "the ground line") —
            // real lookup strategy (layer name? xdata tag? text label proximity?) still undecided;
            // tracked in STATUS.md alongside CommandExecutor.FindNamedReferenceLine.
            context.NearbyReferenceNames = new List<string>();

            tr.Commit();
            return context;
        }
    }
}
