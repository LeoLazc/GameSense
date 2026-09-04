using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GameSense.Core.Services;
using Microsoft.Extensions.Configuration;

namespace GameSense.Infrastructure.Services
{
    public class HttpAiClient : IAiClient
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public HttpAiClient(HttpClient http, IConfiguration config)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _apiKey = config["Ai:ApiKey"] ?? string.Empty;
        }

        public async Task<string> GenerateReviewJsonAsync(IEnumerable<string> franchiseNames, int questionsPerFranchise, string? promptStyle = null, CancellationToken cancellationToken = default)
        {
            var prompt = BuildPrompt(franchiseNames, questionsPerFranchise, promptStyle);

            var payload = new
            {
                prompt,
                max_tokens = 1500,
                temperature = 0.2
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            if (!string.IsNullOrEmpty(_apiKey))
            {
                if (_http.DefaultRequestHeaders.Authorization == null)
                {
                    _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
                }
            }

            var response = await _http.PostAsync(string.Empty, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
            return responseText;
        }

        private string BuildPrompt(IEnumerable<string> franchiseNames, int questionsPerFranchise, string? promptStyle)
        {
            var franchises = string.Join(", ", franchiseNames);

            var schema = "{\n  \"franchises\": [\n    {\n      \"name\": \"<franchise name>\",\n      \"questions\": [ { \"text\": \"<question>\", \"answer\": \"<answer>\" } ],\n      \"review\": { \"text\": \"<review text>\", \"score\": 0 }\n    }\n  ]\n}";

            var sb = new StringBuilder();
            sb.AppendLine("You are an expert gaming critic. Produce a strict JSON object that follows the schema exactly.");
            sb.AppendLine("Schema example:");
            sb.AppendLine(schema);
            sb.AppendLine("Franchises: " + franchises);
            sb.AppendLine("For each franchise, generate " + questionsPerFranchise + " thoughtful questions and provide concise answers and a review object with a text and integer score (0-100). Do not include extra text outside the JSON.");
            if (!string.IsNullOrEmpty(promptStyle))
            {
                sb.AppendLine("Style: " + promptStyle);
            }

            return sb.ToString();
        }
    }
}
