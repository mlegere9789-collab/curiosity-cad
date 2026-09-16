using System;
using System.Net.Http;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Windows;
using Curiosity.Plugin.Execution;
using Curiosity.Plugin.NlParser;
// Both Autodesk.AutoCAD.ApplicationServices and System.Windows.Forms define an "Application" type;
// a real build caught the ambiguity (CS0104). Alias the AutoCAD one explicitly rather than "using"
// the whole ApplicationServices namespace, since Document (used elsewhere in this file) doesn't
// collide and can stay a plain using.
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.AutoCAD.ApplicationServices;

namespace Curiosity.Plugin.Ui
{
    /// <summary>
    /// Docked chat panel inside AutoCAD (PaletteSet host). Minimal WinForms control: a text input +
    /// a scrolling log. Not yet visually polished — functional-first, per STATUS.md's priority order
    /// (get the two acceptance-test instructions working end to end before investing in UI design).
    /// </summary>
    public sealed class ChatPalette
    {
        private readonly PaletteSet _paletteSet;
        private readonly TextBox _input;
        private readonly TextBox _log;

        private readonly LlmFallbackClient? _llmClient;

        public ChatPalette()
        {
            // Each user supplies their own key (see README "Why this scope" - no shared/paid backend).
            // Never hardcode a key here or commit one; ANTHROPIC_API_KEY is read from the environment
            // AutoCAD itself was launched with.
            var apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
            _llmClient = string.IsNullOrWhiteSpace(apiKey)
                ? null
                : new LlmFallbackClient(new HttpClient(), apiKey);

            _paletteSet = new PaletteSet("Curiosity")
            {
                Size = new System.Drawing.Size(340, 500),
                DockEnabled = DockSides.Left | DockSides.Right,
            };

            var panel = new Panel { Dock = DockStyle.Fill };

            _log = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
            };

            _input = new TextBox { Dock = DockStyle.Bottom };
            _input.KeyDown += async (_, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                await HandleInstructionAsync(_input.Text);
                _input.Clear();
            };

            panel.Controls.Add(_log);
            panel.Controls.Add(_input);
            _paletteSet.Add("Chat", panel);
        }

        public bool Visible
        {
            get => _paletteSet.Visible;
            set => _paletteSet.Visible = value;
        }

        private async System.Threading.Tasks.Task HandleInstructionAsync(string instruction)
        {
            var doc = AcApplication.DocumentManager.MdiActiveDocument;
            Log($"> {instruction}");

            var intent = LocalPatternMatcher.TryResolve(instruction);

            if (intent.Action == IntentAction.Unrecognized)
            {
                if (_llmClient == null)
                {
                    Log("(no local match, and no LLM API key configured yet — see docs/PLUGIN_SETUP.md)");
                    return;
                }

                var context = SelectionContext.CaptureCurrent(doc);
                intent = await _llmClient.ResolveAsync(instruction, context);
            }

            if (intent.Action == IntentAction.Unrecognized)
            {
                Log("Didn't understand that — try rephrasing.");
                return;
            }

            try
            {
                CommandExecutor.Execute(intent, doc);
                Log($"Done ({intent.ResolvedBy}, confidence {intent.Confidence:P0}).");
            }
            catch (Exception ex)
            {
                Log($"Failed: {ex.Message}");
            }
        }

        private void Log(string line) => _log.AppendText(line + Environment.NewLine);
    }
}
