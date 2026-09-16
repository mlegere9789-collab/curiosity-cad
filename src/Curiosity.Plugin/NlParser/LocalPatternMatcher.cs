using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Curiosity.Plugin.NlParser
{
    /// <summary>
    /// Tier 1 of the two-tier NL resolver: fast, offline, exact/regex matching against a deliberately
    /// narrow set of known phrasings. Only ever returns high-confidence results — anything it doesn't
    /// recognize falls through to LlmFallbackClient (tier 2), never a low-confidence guess.
    ///
    /// Grow this list from real logged tier-2 fallback cases (see docs/ARCHITECTURE.md), not speculatively.
    /// </summary>
    public static class LocalPatternMatcher
    {
        private static readonly Dictionary<string, string> LineweightWords = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["thin"] = "LineWeight025",
            ["light"] = "LineWeight025",
            ["medium"] = "LineWeight035",
            ["thick"] = "LineWeight060",
            ["heavy"] = "LineWeight060",
        };

        // Two independent checks, not one combined pattern: real phrasing puts the weight word
        // before OR after "line weight" ("change to medium line weight" vs. "set line weight to
        // thin") and a single ordered regex missed the first — the spec's own acceptance-test
        // phrase — until this was actually tested. See docs/PLUGIN_SETUP.md test notes.
        private static readonly Regex LineweightPhrase = new Regex(
            @"\bline\s*weight\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex LineweightWord = new Regex(
            @"\b(thin|light|medium|thick|heavy)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex AnglePattern = new Regex(
            @"\bintersect(?:s|ing)?\b.*\bwith\b\s+(?:the\s+)?(?<reference>[a-z0-9 ]+?)\s+\bat\b\s+(?:a\s+)?(?<degrees>\d+(?:\.\d+)?)\s*(?:degree|deg|°)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>Attempts local resolution. Returns EditIntent.NotRecognized (Confidence 0) if no rule fires.</summary>
        public static EditIntent TryResolve(string instruction)
        {
            if (string.IsNullOrWhiteSpace(instruction))
                return EditIntent.NotRecognized;

            if (LineweightPhrase.IsMatch(instruction))
            {
                var wordMatch = LineweightWord.Match(instruction);
                if (wordMatch.Success && LineweightWords.TryGetValue(wordMatch.Groups[1].Value, out var mapped))
                {
                    return new EditIntent(
                        IntentAction.SetProperty,
                        EntityReference.CurrentSelection,
                        new Dictionary<string, object>
                        {
                            ["property"] = "Lineweight",
                            ["value"] = mapped,
                        },
                        confidence: 0.95,
                        resolvedBy: "local");
                }
            }

            var angleMatch = AnglePattern.Match(instruction);
            if (angleMatch.Success)
            {
                var referenceName = angleMatch.Groups["reference"].Value.Trim();
                var degrees = double.Parse(angleMatch.Groups["degrees"].Value);

                return new EditIntent(
                    IntentAction.ConstrainAngle,
                    EntityReference.CurrentSelection,
                    new Dictionary<string, object>
                    {
                        ["degrees"] = degrees,
                        ["reference"] = new EntityReference("named", referenceName),
                    },
                    confidence: 0.9,
                    resolvedBy: "local");
            }

            return EditIntent.NotRecognized;
        }
    }
}
