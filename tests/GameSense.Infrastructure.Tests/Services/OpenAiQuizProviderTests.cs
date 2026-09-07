using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text.Json;
using GameSense.Core.Exceptions;
using GameSense.Core.Services;
using GameSense.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using OpenAI.Chat;

namespace GameSense.Infrastructure.Tests.Services;

[TestFixture]
public sealed class OpenAiQuizProviderTests
{
    [Test]
    public void Requires_an_api_key_from_configuration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Ai:ApiKey"] = "" })
            .Build();

        Assert.Throws<InvalidOperationException>(() => new OpenAiQuizProvider(configuration));
    }

    [Test]
    public void Reads_an_optional_model_from_configuration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Ai:ApiKey"] = "test-key", ["Ai:Model"] = "gpt-4o-mini" })
            .Build();

        Assert.DoesNotThrow(() => new OpenAiQuizProvider(configuration));
    }

    [Test]
    public async Task Returns_a_valid_normalized_result()
    {
        var provider = new OpenAiQuizProvider(ClientReturning(
            """{"score": 88.5, "confidence": 0.91, "evaluation": "Respuesta muy completa."}"""));

        var result = await provider.EvaluateAsync("Question", "Expected", "Criteria", "Answer");

        Assert.That(result, Is.EqualTo(new QuizEvaluation(88.5m, 0.91m, "Respuesta muy completa.")));
    }

    [TestCase("not json")]
    [TestCase("{\"score\": 101, \"confidence\": 0.5, \"evaluation\": \"inválido\"}")]
    [TestCase("{\"score\": 50, \"confidence\": 1.1, \"evaluation\": \"inválido\"}")]
    [TestCase("{\"score\": 50, \"confidence\": 0.5}")]
    public void Rejects_malformed_or_out_of_range_responses(string body)
    {
        var provider = new OpenAiQuizProvider(ClientReturning(body));

        Assert.ThrowsAsync<AiResponseParseException>(() => provider.EvaluateAsync("Q", "E", "C", "A"));
    }

    [Test]
    public void Rejects_an_empty_response_without_assigning_a_score()
    {
        var provider = new OpenAiQuizProvider(ClientReturning(""));

        Assert.ThrowsAsync<AiResponseParseException>(() => provider.EvaluateAsync("Q", "E", "C", "A"));
    }

    [Test]
    public void Rejects_a_non_stop_finish_reason_without_assigning_a_score()
    {
        var provider = new OpenAiQuizProvider(ClientReturning(
            """{"score": 50, "confidence": 0.5, "evaluation": "ok"}""", finishReason: ChatFinishReason.Length));

        Assert.ThrowsAsync<AiResponseParseException>(() => provider.EvaluateAsync("Q", "E", "C", "A"));
    }

    [Test]
    public void Rejects_an_unsuccessful_api_response_without_assigning_a_score()
    {
        var provider = new OpenAiQuizProvider(FailingClient());

        Assert.ThrowsAsync<AiResponseParseException>(() => provider.EvaluateAsync("Q", "E", "C", "A"));
    }

    [Test]
    public void Propagates_cancellation()
    {
        var provider = new OpenAiQuizProvider(ClientReturning(
            """{"score": 50, "confidence": 0.5, "evaluation": "ok"}"""));

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.CatchAsync<OperationCanceledException>(() => provider.EvaluateAsync("Q", "E", "C", "A", cancellation.Token));
    }

    private static ChatClient ClientReturning(string content, ChatFinishReason finishReason = ChatFinishReason.Stop) =>
        new StubChatClient(JsonSerializer.Serialize(new
        {
            id = "chatcmpl-test",
            @object = "chat.completion",
            created = 1L,
            model = "gpt-4o-mini",
            choices = new[]
            {
                new
                {
                    index = 0,
                    finish_reason = finishReason.ToString(),
                    message = new { role = "assistant", content }
                }
            },
            usage = new { prompt_tokens = 1, completion_tokens = 1, total_tokens = 2 }
        }));

    private static ChatClient FailingClient() => new StubChatClient(throwError: true);

    private sealed class StubChatClient : ChatClient
    {
        private readonly string? _content;
        private readonly bool _throwError;

        public StubChatClient(string wireJson) : base("gpt-4o-mini", "test-api-key")
        {
            _content = wireJson;
        }

        public StubChatClient(bool throwError) : base("gpt-4o-mini", "test-api-key")
        {
            _throwError = throwError;
        }

        public override Task<ClientResult<ChatCompletion>> CompleteChatAsync(
            IEnumerable<ChatMessage> messages,
            ChatCompletionOptions options = null,
            CancellationToken cancellationToken = default)
        {
            if (_throwError)
                throw new ClientResultException("Unauthorized", new StubPipelineResponse(401));

            cancellationToken.ThrowIfCancellationRequested();
            var completion = ModelReaderWriter.Read<ChatCompletion>(BinaryData.FromString(_content!))!;
            return Task.FromResult(ClientResult.FromValue(completion, new StubPipelineResponse(200)));
        }
    }

    private sealed class StubPipelineResponse : PipelineResponse
    {
        private readonly int _status;

        public StubPipelineResponse(int status) => _status = status;

        public override int Status => _status;
        public override string ReasonPhrase => "OK";
        public override Stream? ContentStream { get; set; }
        public override BinaryData Content => BinaryData.FromString("{}");
        protected override PipelineResponseHeaders HeadersCore => new StubHeaders();
        public override void Dispose() { }
        public override BinaryData BufferContent(CancellationToken cancellationToken = default) => Content;
        public override ValueTask<BinaryData> BufferContentAsync(CancellationToken cancellationToken = default) => new(Content);

        private sealed class StubHeaders : PipelineResponseHeaders
        {
            public override IEnumerator<KeyValuePair<string, string>> GetEnumerator() => Enumerable.Empty<KeyValuePair<string, string>>().GetEnumerator();
            public override bool TryGetValue(string name, out string? value) { value = null; return false; }
            public override bool TryGetValues(string name, out IEnumerable<string>? values) { values = null; return false; }
        }
    }
}
