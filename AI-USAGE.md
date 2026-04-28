# AI Usage Report

## AI Strategy
I used AI-augmented development patterns to accelerate design and implementation. The core workflow included:
- Generating project structure ideas and identifying key files for a DDD API, infrastructure services, and frontend.
- Authoring domain models, repository interfaces, and event/caching abstractions with a focus on clear separation of concerns.
- Using tools to draft API controller behavior, error handling, and frontend interaction patterns.

## Tools Used
- GitHub Copilot (via VS Code) to generate boilerplate C# and React/TypeScript code patterns.
- Local reasoning and manual review to ensure architecture correctness and fit the assignment requirements.

## Human Audit
Accepted AI suggestions:
- A layered folder structure separating `Contracts`, `Domain`, `Infrastructure`, `WebApi`, and `UnitTests`.
- A Redis cache for inventory reads and cache invalidation after hold mutations.
- RabbitMQ event publishing for `HoldCreated`, `HoldReleased`, and `HoldExpired` events.

Rejected or adjusted AI suggestions:
- I rejected overly broad data models that combined inventory and hold state into a single document.
- I adjusted event payload shaping to include only the necessary hold context for consumers.
- I replaced simple error handling with explicit HTTP status mapping and custom error DTOs.

## Verification
- Generated unit tests with `NUnit` and `Moq` to verify business rules, validation failures, expired-hold handling, and cache usage.
- Validated the backend wiring and startup flow by reviewing `Program.cs`, `docker-compose.yml`, and DI registration logic.
- Ensured the frontend state sync strategy refreshes inventory after hold creation and release.

## Summary
AI helped accelerate the initial scaffolding and repeated patterns, while I kept the architecture aligned to the requirements and manually reviewed integration points for reliability and correctness.
