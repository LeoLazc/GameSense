# GameSense

GameSense supports gaming reviewers through a knowledge-gated review workflow. Reviewers complete quizzes, receive scored evaluations, and may proceed when they meet the product's eligibility rules. Success means preserving a reliable path from knowledge evaluation to review creation.

The evaluation itself is not a hardcoded rule: it is performed by a language model behind an interface owned by the domain. That single decision shapes most of the architecture below.

**Status:** builds and runs on `net10.0`. 72 tests, 0 failures (`dotnet test`). SQL Server via EF Core, 13 migrations. The React/TypeScript front end lives in `src/GameSense.Web` and is not part of this solution.

## What it does

A review is only worth what the reviewer is worth. GameSense puts a gate in front of publishing: you have to prove you know the subject first. Quizzes run as sessions, are evaluated in batches, and the resulting score decides whether the user becomes eligible to review.

| Area | Endpoints |
|---|---|
| Auth | `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/logout`, `GET /api/auth/me` |
| Games | `GET /api/games`, `GET /api/games/{id}` |
| Quiz | `POST /api/quiz/sessions`, `POST /api/quiz/sessions/{id}/answers`, `GET /api/quiz/sessions/{id}/result` |
| Reviews | `POST /api/games/{gameId}/reviews` (JWT + eligibility) |
| GOTY | `GET /api/goty/prediction`, `PUT /api/goty/prediction` |

## Architecture

```
GameSense.Api ──────────► GameSense.Core
  controllers               domain models
  MediatR handlers          interfaces (repositories, services)
  validators                domain exceptions
  middleware                no external dependencies
      │
      └──────────► GameSense.Infrastructure
                     EF Core + repositories
                     adapters (AI, game catalog, auth)
```

`GameSense.Core` references neither EF Core, nor ASP.NET, nor the OpenAI SDK. Everything crossing from Infrastructure upward goes through an interface declared in Core.

### Request flow

Controller → MediatR → `ValidationBehavior` (FluentValidation) → handler → domain service → repository → EF Core. Validation and domain exceptions are translated into `ProblemDetails` by one middleware, with a status code per exception type.

## Decisions (and why)

**1. The domain does not know who evaluates the quiz.**
`IAiQuizProvider` has two implementations: `OpenAiQuizProvider` (OpenAI SDK, `gpt-4o-mini`, strict JSON schema so the answer is not free-form text) and `HttpAiQuizProvider` (a self-hosted gateway over HTTP). The active one is selected by configuration (`Ai:QuizProvider`); an unregistered value fails at startup with a message pointing at the registration site. Neither the handler nor the session service mentions OpenAI.

**2. The user's answer is data, not instruction.**
Both adapters send the answer in the user message, never in the system prompt, and the system prompt explicitly instructs the model to ignore instructions embedded in the text. That is the difference between grading an exam and letting the student write the grade.

**3. Validation happens once, in the pipeline.**
FluentValidation runs in a MediatR `IPipelineBehavior`, not in each controller. An invalid request short-circuits before the handler and the middleware turns it into a `400` with `ProblemDetails`. No controller contains `if (ModelState.IsValid)`.

**4. One HTTP status per exception type, in one place.**
A global middleware maps exceptions: quiz or review conflict, duplicate prediction, ineligible user, unparsable AI response (`422`). Controllers do not decide error codes.

**5. Invalid configuration breaks startup, not the first request.**
A missing `Jwt:Key` — or one shorter than 32 bytes — stops the app from starting. `ClockSkew` is zero. I would rather fail at deploy time than at 3 a.m.

**6. Review uniqueness is enforced by the database.**
A handler-level check does not survive two concurrent requests, so there is a unique index. The migration is called `EnforceReviewUniqueness` on purpose.

**7. The game catalog is not persisted.**
Games come from RAWG behind `IGameCatalogProvider`, registered as a singleton over a typed `HttpClient` with `IMemoryCache`. They used to be stored locally; the `RemoveLocalGamePersistence` migration drops that table. A catalog that changes daily gains nothing from a stale local copy.

## Tests

72 tests across three projects: `GameSense.Core.Tests` (domain models), `GameSense.Api.Tests` (handlers, services, validators and controllers with NUnit + Moq) and `GameSense.Infrastructure.Tests` (adapters against a mocked `HttpMessageHandler`, EF Core InMemory). `InternalsVisibleTo` exposes the constructor that takes a `ChatClient` so the provider can be tested without hitting the network.

Not covered: integration tests against a real SQL Server (repository tests use InMemory, which enforces neither constraints nor indexes) and the front end.

## Running it

Requirements: .NET 10 SDK, SQL Server or LocalDB.

```bash
dotnet restore "GameSense AI.slnx"
dotnet ef database update --project src/GameSense.Infrastructure --startup-project src/GameSense.Api
dotnet run --project src/GameSense.Api
# http://localhost:5234 — Swagger at /swagger
```

Configuration through User Secrets (`dotnet user-secrets`) or environment variables: `Jwt:Key` (32 bytes or more), `Jwt:Issuer`, `Jwt:Audience`, `ConnectionStrings:DefaultConnection`.

Evaluating quizzes for real requires either `Ai:ApiKey` (OpenAI provider) or `Ai:BaseUrl` (own gateway). The repository ships no credentials and does not need them to start: only the quiz endpoints depend on that configuration.

## Known gaps (and what I would do next)

- `AutoMapper` 12.0.1 carries a known vulnerability (NU1903). It is used in exactly one handler, so the right fix is not to upgrade the package but to drop the dependency and map by hand.
- No CI. A `dotnet build` + `dotnet test` workflow on every PR is missing.
- Repository tests run on InMemory; they should move to Testcontainers with a real SQL Server so they validate what they claim to validate.
- No rate limiting on `POST /api/auth/login`.
- No OpenTelemetry: logging is ASP.NET Core's default.
