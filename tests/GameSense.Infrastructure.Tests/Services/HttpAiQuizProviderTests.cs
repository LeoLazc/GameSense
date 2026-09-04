using System.Net;
using System.Text.Json;
using GameSense.Core.Exceptions;
using GameSense.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace GameSense.Infrastructure.Tests.Services;

[TestFixture]
public sealed class HttpAiQuizProviderTests
{
    [Test]
    public async Task Returns_a_valid_normalized_result_and_sends_the_expected_request()
    {
        var handler = new StubHandler(HttpStatusCode.OK, "{\"score\": 88.5, \"confidence\": 0.91, \"evaluation\": \"Strong answer.\"}");
        var provider = CreateProvider(handler);

        var result = await provider.EvaluateAsync("Question", "Expected", "Criteria", "Answer");

        Assert.That(result, Is.EqualTo(new GameSense.Core.Services.QuizEvaluation(88.5m, 0.91m, "Strong answer.")));
        Assert.That(handler.Method, Is.EqualTo(HttpMethod.Post));
        var request = JsonSerializer.Deserialize<Dictionary<string, string>>(handler.Body!);
        Assert.That(request!["evaluationCriteria"], Is.EqualTo("Criteria"));
        Assert.That(request["userAnswer"], Is.EqualTo("Answer"));
    }

    [TestCase("not json")]
    [TestCase("{\"score\": 101, \"confidence\": 0.5, \"evaluation\": \"invalid\"}")]
    [TestCase("{\"score\": 50, \"confidence\": 1.1, \"evaluation\": \"invalid\"}")]
    [TestCase("{\"score\": 50, \"confidence\": 0.5}")]
    public void Rejects_malformed_or_out_of_range_responses(string body)
    {
        var provider = CreateProvider(new StubHandler(HttpStatusCode.OK, body));

        Assert.ThrowsAsync<AiResponseParseException>(() => provider.EvaluateAsync("Q", "E", "C", "A"));
    }

    [Test]
    public void Rejects_non_success_status_without_assigning_a_score()
    {
        var provider = CreateProvider(new StubHandler(HttpStatusCode.BadGateway, "{}"));

        Assert.ThrowsAsync<AiResponseParseException>(() => provider.EvaluateAsync("Q", "E", "C", "A"));
    }

    [Test]
    public void Propagates_cancellation()
    {
        var provider = CreateProvider(new CancellationHandler());
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.CatchAsync<OperationCanceledException>(() => provider.EvaluateAsync("Q", "E", "C", "A", cancellation.Token));
    }

    private static HttpAiQuizProvider CreateProvider(HttpMessageHandler handler) => new(
        new HttpClient(handler) { BaseAddress = new Uri("https://test.invalid/") },
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build());

    private sealed class StubHandler(HttpStatusCode status, string body) : HttpMessageHandler
    {
        public HttpMethod? Method { get; private set; }
        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Method = request.Method;
            Body = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(status) { Content = new StringContent(body) };
        }
    }

    private sealed class CancellationHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromCanceled<HttpResponseMessage>(cancellationToken);
        }
    }
}
