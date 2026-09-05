# Game catalog

GameSense uses RAWG as a backend-only catalog provider. The catalog integration stores game metadata and never stores Metacritic or any other external review score; user review ratings remain GameSense data.

## Setup

1. Create a RAWG account, request an API key, and review RAWG's API terms and attribution requirements.
2. Configure `GameCatalog:ApiKey` with user secrets or deployment secret storage. Do not expose it to a browser, log it, or commit it.
3. Leave `GameCatalog:ApiBaseUrl` at `https://api.rawg.io/api/` unless using a compatible proxy.

The API exposes `GET /api/games/search?query=...` and `GET /api/games/current-year`. Both endpoints fetch from RAWG, upsert the results by provider and external ID, and return local game IDs for reviews and GOTY predictions.

`GET /api/games/current-year` includes games released from November 15 of the previous UTC year through December 31 of the current UTC year, inclusive. RAWG receives one continuous date-range request, and results are deduplicated by provider and external ID before upsert.

RAWG requests are made by the backend so the API key stays private. RAWG applies rate limits and its terms may change; callers should avoid polling these endpoints, cache results where appropriate, and follow RAWG attribution and usage requirements.

RAWG list responses provide a background image but no cover, summary, website, franchise, or external review score in this integration. Images are not downloaded or copied into GameSense storage. No external rating is returned or persisted.
