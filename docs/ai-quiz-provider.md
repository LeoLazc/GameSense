# Quiz AI provider contract

Quiz evaluation uses the `IAiQuizProvider` port in Core. Two adapters implement it in Infrastructure:

- `HttpAiQuizProvider` — posts to a custom gateway contract.
- `OpenAiQuizProvider` — calls the OpenAI chat completions API directly.

Both are selected with `Ai:QuizProvider` (default `Http`) in `AddGameSenseServices`. The API key lives in `Ai:ApiKey` (user secrets or environment configuration) and is never included in logs or responses. `Ai:Model` optionally overrides the model (default `gpt-4o-mini`); it is ignored by the Http provider.

## Http provider (`Ai:QuizProvider: Http`)

Posts to `Ai:QuizEvaluationPath` relative to `Ai:BaseUrl`. The request JSON is:

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

## OpenAI provider (`Ai:QuizProvider: OpenAI`)

Calls the OpenAI chat completions API with the `OpenAI` SDK. The system prompt requires a STRICT JSON object with `score`, `confidence`, and `evaluation`:

- `score` — continuous decimal from 0 to 100.
- `confidence` — decimal from 0 to 1.
- `evaluation` — required; the feedback text MUST be written ONLY in Spanish.

`userAnswer` is placed only in the user message and is treated as untrusted answer data: the system prompt explicitly requires embedded instructions inside it to be ignored.

## Response shape

The provider must return only this normalized JSON shape:

```json
{
  "score": 0,
  "confidence": 0,
  "evaluation": "..."
}
```

`score` is a decimal from 0 to 100, `confidence` is a decimal from 0 to 1, and `evaluation` is required. Non-success status codes, cancellation, malformed JSON, empty responses, missing fields, and out-of-range values do not produce a score; they fail the operation. The OpenAI provider also fails when the model stops before completing (`finish_reason` other than `stop`).

To add another provider, implement `IAiQuizProvider` in Infrastructure and add its provider-name branch in `AddGameSenseServices` (or replace that boundary with a keyed/factory registration). Quiz handlers remain dependent only on `IQuizAnswerEvaluator`.

Keep `Ai:ApiKey` in user secrets or environment configuration. It is never included in logs or responses.
