using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using GameSense.Core.Exceptions;
using GameSense.Core.Services;
using Microsoft.Extensions.Configuration;

namespace GameSense.Infrastructure.Services;

public sealed class HttpAiQuizProvider : IAiQuizProvider
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _path;

    public HttpAiQuizProvider(HttpClient http, IConfiguration configuration)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _apiKey = configuration["Ai:ApiKey"] ?? string.Empty;
        _path = configuration["Ai:QuizEvaluationPath"] ?? "quiz/evaluate";
    }

    public async Task<QuizEvaluation> EvaluateAsync(
        string questionText,
        string expectedAnswer,
        string evaluationCriteria,
        string userAnswer,
        CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(new
        {
            questionText,
            expectedAnswer,
            evaluationCriteria,
            userAnswer
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, _path)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        if (!string.IsNullOrWhiteSpace(_apiKey))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new AiResponseParseException($"The quiz AI provider returned HTTP {(int)response.StatusCode}.");

        ProviderResponse result;
        try
        {
            result = JsonSerializer.Deserialize<ProviderResponse>(responseBody, JsonOptions)
                ?? throw new JsonException("The provider response was empty.");
        }
        catch (JsonException exception)
        {
            throw new AiResponseParseException("The quiz AI provider returned malformed JSON.", exception);
        }

        if (result.Score is null || result.Confidence is null || string.IsNullOrWhiteSpace(result.Evaluation) ||
            result.Score is < 0m or > 100m || result.Confidence is < 0m or > 1m)
            throw new AiResponseParseException("The quiz AI provider returned an invalid normalized evaluation.");

        return new QuizEvaluation(result.Score.Value, result.Confidence.Value, result.Evaluation);
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private sealed class ProviderResponse
    {
        public decimal? Score { get; set; }
        public decimal? Confidence { get; set; }
        public string? Evaluation { get; set; }
    }
}
