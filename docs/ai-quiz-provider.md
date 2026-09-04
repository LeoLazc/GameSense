# Quiz AI provider contract

Quiz evaluation uses the `IAiQuizProvider` port in Core. The current `HttpAiQuizProvider` adapter is selected with `Ai:QuizProvider: Http` and posts to `Ai:QuizEvaluationPath` relative to `Ai:BaseUrl`.

The request JSON is:

```json
{
  "questionText": "...",
  "expectedAnswer": "...",
  "evaluationCriteria": "...",
  "userAnswer": "..."
}
```

The provider must return only this normalized JSON shape:

```json
{
  "score": 0,
  "confidence": 0,
  "evaluation": "..."
}
```

`score` is a decimal from 0 to 100, `confidence` is a decimal from 0 to 1, and `evaluation` is required. Non-success status codes, cancellation, malformed JSON, missing fields, and out-of-range values do not produce a score; they fail the operation.

To add another provider, implement `IAiQuizProvider` in Infrastructure and add its provider-name branch in `AddGameSenseServices` (or replace that boundary with a keyed/factory registration). Quiz handlers remain dependent only on `IQuizAnswerEvaluator`.

Keep `Ai:ApiKey` in user secrets or environment configuration. It is never included in logs or responses.
