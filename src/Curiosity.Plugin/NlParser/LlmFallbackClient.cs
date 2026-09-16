using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Curiosity.Plugin.NlParser
{
    /// <summary>
    /// Tier 2 of the two-tier NL resolver: called only when LocalPatternMatcher returns NotRecognized.
    /// Sends the instruction plus selected-entity context to the Claude API and expects back JSON in
    /// exactly the EditIntent shape (see docs/INTENT_SCHEMA.md), so CommandExecutor has one code path
    /// regardless of which tier resolved the instruction.
    ///
    /// Requires an API key supplied by the user (each classmate uses their own — see README "Why this
    /// scope"). Never hardcode a key in source; read from an environment variable or a local settings
    /// file that is gitignored.
    /// </summary>
    public sealed class LlmFallbackClient
    {
        private const string ApiUrl = "https://api.anthropic.com/v1/messages";
        private const string Model = "claude-sonnet-5";

        private readonly HttpClient _http;
        private readonly string _apiKey;

        public LlmFallbackClient(HttpClient http, string apiKey)
        {
            _http = http;
            _apiKey = apiKey;
        }

        public async Task<EditIntent> ResolveAsync(string instruction, EntityContext context)
        {
            var systemPrompt = BuildSystemPrompt();
            var userPrompt = BuildUserPrompt(instruction, context);

            var requestBody = new
            {
                model = Model,
                max_tokens = 512,
                system = systemPrompt,
                messages = new[]
                {
                    new { role = "user", content = userPrompt }
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("x-api-key", _apiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");

            var response = await _http.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return ParseModelResponse(responseJson);
        }

        private static string BuildSystemPrompt() =>
            "You translate a CAD user's plain-English editing instruction into a single JSON object " +
            "matching this exact schema: " +
            "{\"action\": \"SetProperty|ConstrainAngle|ConstrainParallel|ConstrainPerpendicular|Transform|Select|RunNativeCommand|RunMacro|Unrecognized\", " +
            "\"target\": {\"kind\": \"selection|named|nearest\", \"name\": string|null}, " +
            "\"parameters\": object, \"confidence\": number}. " +
            "Parameters per action - " +
            "SetProperty: {\"property\": \"Lineweight|Color|Layer\", \"value\": string} (Lineweight value must be one of " +
            "LineWeight025|LineWeight035|LineWeight060, mapped from words like thin/light/medium/thick/heavy); " +
            "ConstrainAngle: {\"degrees\": number, \"reference\": {\"kind\": \"named\", \"name\": string}}; " +
            "Transform: {\"operation\": \"Rotate\", \"degrees\": number} or {\"operation\": \"Move\", \"dx\": number, \"dy\": number} " +
            "or {\"operation\": \"Scale\", \"factor\": number} - use Transform for any rotate/move/scale instruction that " +
            "isn't specifically about matching another entity's angle (that's ConstrainAngle instead); " +
            "Select: {\"layer\": string?, \"entityType\": string?, \"colorIndex\": number?} - use this when the user wants to " +
            "select/pick something by description instead of naming an edit directly (e.g. \"select the wall layer\", " +
            "\"select all circles\", \"select everything red\"); entityType must be a real AutoCAD DXF entity name " +
            "(LINE, CIRCLE, ARC, LWPOLYLINE, TEXT, MTEXT, etc.), colorIndex is AutoCAD's 1-255 ACI index (1=red, 2=yellow, " +
            "3=green, 4=cyan, 5=blue, 6=magenta, 7=white/black); include only the criteria the instruction actually specifies; " +
            "RunNativeCommand: {\"commandString\": string} - the general fallback for AutoCAD's full ~1500-command surface " +
            "when no other action fits: the literal AutoCAD command-line text, newline-separated exactly as a person would " +
            "type it (e.g. \"FILLET\\nR\\n0.5\\n\" to fillet with a 0.5 radius), assuming it will run against whatever is " +
            "currently selected. Only use this for commands that can complete from a selection plus typed values - never " +
            "for a command that needs an arbitrary point picked on screen with no location given in the instruction; if the " +
            "instruction doesn't supply enough information to write a complete, unambiguous command string, return " +
            "Unrecognized instead of guessing; " +
            "RunMacro: {\"name\": string} (only if the instruction clearly matches a known macro, otherwise prefer Unrecognized). " +
            "Prefer the most specific action that fits (SetProperty/ConstrainAngle/Transform/Select) over RunNativeCommand " +
            "when one applies - RunNativeCommand is the fallback of last resort, not the default. " +
            "Return Unrecognized with confidence 0 if the instruction is ambiguous, unsupported, or you are not confident. " +
            "Respond with ONLY the JSON object, no prose, no markdown fencing.";

        private static string BuildUserPrompt(string instruction, EntityContext context) =>
            $"Instruction: \"{instruction}\"\n" +
            $"Selected entity type: {context.EntityType}\n" +
            $"Selected entity properties: {JsonSerializer.Serialize(context.Properties)}\n" +
            $"Nearby named/candidate reference entities: {JsonSerializer.Serialize(context.NearbyReferenceNames)}";

        private static EditIntent ParseModelResponse(string responseJson)
        {
            // NOTE: not yet validated against a real API response shape or a real AutoCAD drawing context.
            // This is the integration point flagged in STATUS.md as needing a real end-to-end test pass.
            using var doc = JsonDocument.Parse(responseJson);
            var text = doc.RootElement.GetProperty("content")[0].GetProperty("text").GetString() ?? "{}";

            using var intentDoc = JsonDocument.Parse(text);
            var root = intentDoc.RootElement;

            var action = Enum.Parse<IntentAction>(root.GetProperty("action").GetString() ?? "Unrecognized");
            var targetEl = root.GetProperty("target");
            var target = new EntityReference(
                targetEl.GetProperty("kind").GetString() ?? "selection",
                targetEl.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : null);

            var parameters = new Dictionary<string, object>();
            foreach (var prop in root.GetProperty("parameters").EnumerateObject())
            {
                // "reference" (used by ConstrainAngle) is a nested {kind, name} object, not a flat
                // value - CommandExecutor casts it to EntityReference, so it must be parsed into one
                // here rather than flattened to a JSON string like every other parameter. Missing
                // this was a real bug: it would have thrown InvalidCastException the first time
                // anyone tried an angle-constraint instruction through the LLM tier.
                if (prop.Name == "reference" && prop.Value.ValueKind == JsonValueKind.Object)
                {
                    parameters[prop.Name] = new EntityReference(
                        prop.Value.GetProperty("kind").GetString() ?? "named",
                        prop.Value.TryGetProperty("name", out var refNameEl) ? refNameEl.GetString() : null);
                }
                else
                {
                    parameters[prop.Name] = prop.Value.ToString();
                }
            }

            var confidence = root.GetProperty("confidence").GetDouble();

            return new EditIntent(action, target, parameters, confidence, resolvedBy: "llm");
        }
    }

    public sealed class EntityContext
    {
        // set, not init: SelectionContext.CaptureCurrent builds this incrementally after
        // construction (init-only properties can't be reassigned outside an object initializer,
        // caught by a real build - CS8852).
        public string EntityType { get; set; } = "";
        public Dictionary<string, object> Properties { get; init; } = new();
        public List<string> NearbyReferenceNames { get; set; } = new();
    }
}
