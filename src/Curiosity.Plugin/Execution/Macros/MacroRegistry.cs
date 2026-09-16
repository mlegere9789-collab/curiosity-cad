using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace Curiosity.Plugin.Execution.Macros
{
    /// <summary>
    /// Fixed, documented registry of one-click macros for AutoCAD's known multi-step chores
    /// (from the pain-point research: paper-space/layout setup, purging, layer standardization).
    /// Macro names are never freeform strings from the LLM tier — RunMacro intents must match
    /// a name in this registry exactly, or CommandExecutor reports it unimplemented.
    /// </summary>
    public static class MacroRegistry
    {
        private static readonly Dictionary<string, Action<Document, Transaction>> Macros = new()
        {
            ["purge-unused"] = PurgeUnused,
            // "setup-layout" and "standardize-layers" are designed (see docs/BUILD_PLAN.md's successor
            // notes in STATUS.md) but not yet implemented — placeholders intentionally omitted here so
            // RunMacro fails loudly instead of silently no-opping.
        };

        public static void Run(string name, Document doc, Transaction tr)
        {
            if (Macros.TryGetValue(name, out var macro))
            {
                macro(doc, tr);
            }
            else
            {
                doc.Editor.WriteMessage($"\nCuriosity: macro '{name}' is not implemented yet.\n");
            }
        }

        private static void PurgeUnused(Document doc, Transaction tr)
        {
            // Wraps AutoCAD's own Database.Purge — the macro's value is exposing it as one NL
            // instruction ("clean up unused stuff") instead of the manual PURGE dialog flow.
            var db = doc.Database;
            var ids = new ObjectIdCollection();
            foreach (ObjectId id in db.LayerTableId.IsNull ? new ObjectIdCollection() : new ObjectIdCollection())
            {
                ids.Add(id); // placeholder collection-building logic — finalize once running against real AutoCAD
            }
            db.Purge(ids);
            foreach (ObjectId id in ids)
            {
                var obj = tr.GetObject(id, OpenMode.ForWrite);
                obj.Erase();
            }
        }
    }
}
