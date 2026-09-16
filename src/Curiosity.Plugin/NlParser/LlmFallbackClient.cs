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
            "{\"action\": \"SetProperty|ConstrainAngle|ConstrainParallel|ConstrainPerpendicular|RunMacro|Unrecognized\", " +
            "\"target\": {\"kind\": \"selection|named|nearest\", \"name\": string|null}, " +
            "\"parameters\": object, \"confidence\": number}. " +
            "Return Unrecognized with confidence 0 if the instruction is ambiguous or unsupported. " +
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
                parameters[prop.Name] = prop.Value.ToString();
            }

            var confidence = root.GetProperty("confidence").GetDouble();

            return new EditIntent(action, target, parameters, confidence, resolvedBy: "llm");
        }
    }

    public sealed class EntityContext
    {
        public string EntityType { get; init; } = "";
        public Dictionary<string, object> Properties { get; init; } = new();
        public List<string> NearbyReferenceNames { get; init; } = new();
    }
}
