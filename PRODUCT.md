# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

Gaming reviewers who need to demonstrate relevant videogame knowledge before using the review workflow.

## Product Purpose

GameSense supports gaming reviewers through a knowledge-gated review workflow. Reviewers complete quizzes, receive scored evaluations, and may proceed when they meet the product's eligibility rules. Success means preserving a reliable path from knowledge evaluation to review creation.

## Positioning

GameSense makes reviewer eligibility part of the review workflow by using quiz performance as a knowledge gate, rather than treating review creation as an unrestricted form submission.

## Operating Context

GameSense is currently implemented as an ASP.NET Core Web API backed by SQL Server. The API exposes review, quiz, user, and game-catalog operations for a web client. External AI quiz evaluation and RAWG game-catalog requests run through backend adapters so credentials remain private.

## Capabilities and Constraints

- Reviewers can use the existing review workflow.
- Quiz scoring and eligibility determine whether a reviewer may proceed.
- Quiz evaluation uses a backend AI-provider boundary and normalized scores/evaluations.
- Game metadata is sourced through a backend RAWG catalog integration.
- AI and RAWG API keys must remain in secrets or environment configuration and must not be exposed in browser responses or logs.
- User review ratings remain GameSense data; external review scores are not stored or returned by the catalog integration.
- The frontend technology and deployment target are not established in this product record.

## Evidence on Hand

- `docs/ai-quiz-provider.md` documents the AI quiz evaluation contract and secret-handling requirements.
- `docs/game-catalog.md` documents the RAWG catalog integration, attribution/rate-limit constraints, and data boundaries.
- `src/GameSense.Api` contains the ASP.NET Core API entry point, controllers, authentication, validation, and Swagger configuration.
- Automated tests exist under `tests/` for core, API, and infrastructure behavior.
- No customer testimonials, market benchmarks, or externally validated product claims are recorded; future work must not fabricate them.

## Product Principles

- Knowledge eligibility is part of the review experience, not an unrelated gate.
- Preserve the existing review workflow while evolving quiz capabilities.
- Keep third-party credentials and provider-specific details behind backend boundaries.
- Treat scores and eligibility as explicit, testable product behavior.
