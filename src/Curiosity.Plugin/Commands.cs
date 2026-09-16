using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Curiosity.Plugin.Ui;

[assembly: CommandClass(typeof(Curiosity.Plugin.Commands))]

namespace Curiosity.Plugin
{
    /// <summary>
    /// Entry points AutoCAD discovers after NETLOAD. See docs/PLUGIN_SETUP.md for how a classmate
    /// installs and loads this.
    /// </summary>
    public class Commands
    {
        private static ChatPalette? _palette;

        [CommandMethod("CURIOSITY")]
        public void ShowPalette()
        {
            _palette ??= new ChatPalette();
            _palette.Visible = true;
        }
    }
}
