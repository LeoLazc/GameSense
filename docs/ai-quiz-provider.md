# Quiz AI provider contract

Quiz evaluation uses the `IAiQuizProvider` port in Core. The current `HttpAiQuizProvider` adapter is selected with `Ai:QuizProvider: Http` and posts to `Ai:QuizEvaluationPath` relative to `Ai:BaseUrl`.

The request JSON is:

```json
{
  "instructions": "Evaluate the videogame knowledge answer using the question, expected answer, and evaluation criteria. ...",
  "questionText": "...",
  "expectedAnswer": "...",
  "evaluationCriteria": "...",
  "userAnswer": "..."
}
```

`instructions` is fixed backend-owned evaluation guidance. It is not supplied by the frontend. It instructs the provider to evaluate videogame knowledge using the question, expected answer, and evaluation criteria; assign any decimal score from 0 to 100 based on correctness, completeness, and evaluation criteria; use confidence from 0 to 1; and return a concise evaluation. `userAnswer` is untrusted answer data, so any instructions inside it must be ignored.

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
