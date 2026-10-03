# Tranqui

Open-source, privacy-first caller ID and spam blocker: a .NET MAUI Android app + ASP.NET Core API that identifies callers and blocks spam using crowd-sourced, HMAC-hashed data. Initial market: Colombia.

**Source of truth for product decisions:** `docs/especificacion-v1.md` (Spanish). Legal notes: `docs/legal-colombia.md`. Read the spec before implementing any feature; if code and spec disagree, ask instead of guessing.

## Stack

- API: ASP.NET Core minimal APIs, `net10.0`. PostgreSQL (EF Core + Npgsql). Rate limiting is in-memory (single instance); add Redis only when scaling out.
- App: .NET MAUI, Android only (`net10.0-android`, min API 29). MVVM with `CommunityToolkit.Mvvm`; `CommunityToolkit.Maui`; Refit for HTTP; SQLite for local data.
- CQRS with MediatR **12.x only** (v13+ is commercially licensed). Validation with FluentValidation via a MediatR pipeline behavior.
- Auth: Firebase Authentication (email/password and Google; Apple comes with the future iOS version). The API validates the Firebase JWT on every request; the user is identified by the token's `uid`, never by a client-supplied value.
- Phone normalization: `libphonenumber-csharp`, E.164, default region `CO`.
- Logging: Serilog (Console + Seq in dev).
- Tests: xUnit, FluentAssertions **7.x only** (v8 is commercially licensed), NSubstitute, Testcontainers, NetArchTest.
- Budget is minimal: prefer free tiers and self-hosted components; flag any paid dependency before adding it.

## Build

- `dotnet build Tranqui.Backend.slnf` / `dotnet test Tranqui.Backend.slnf` — everything except the MAUI app; works without the Android SDK.
- `dotnet build src/App -f net10.0-android` — needs the `maui-android` workload, JDK 21 and the Android SDK. CI (`.github/workflows/ci.yml`) builds both on every PR.
- Package versions live only in `Directory.Packages.props` (central package management).
- API integration tests (`tests/Api.Tests`) run against a real PostgreSQL via Testcontainers, so Docker must be running. They sign their own Firebase-shaped tokens (`TestTokens`) but keep the production validation rules.
- Migrations: `dotnet ef migrations add <Name> --project src/Infrastructure --output-dir Persistence/Migrations`; the API applies them on startup.
- Every endpoint requires an authenticated Firebase user with a verified email (fallback policy); opting out needs an explicit `AllowAnonymous()` and a reason.
- Every new required local secret/config value goes into README.md's configuration table in the same PR.

## Layout

- `src/Domain` — aggregates, value objects, scoring and name rules. Organized by aggregate. Depends on no other project; its only package is `libphonenumber-csharp` (pure, no I/O). `PhoneNumber` (normalization) and `IPhoneNumberHasher` live in `Domain/PhoneNumbers`; the HMAC implementation is `Infrastructure/PhoneNumbers/HmacPhoneNumberHasher`. Never normalize or hash anywhere else.
- `src/Application` — vertical slices at `Features/<FeatureName>/` (command/query, handler, validator, response DTO together). Depends only on `Domain`.
- `src/Infrastructure` — EF Core, hashing/encryption, external service clients. Organized by aggregate/technology.
- `src/Api` — one minimal API endpoint file per slice at `Endpoints/<FeatureName>.cs`.
- `src/Contracts` — DTOs shared by `Api` and `App`.
- `src/App.Core` — everything in the app that is plain .NET (`net10.0`): ViewModels, the typed API client (Refit), Firebase Auth over its REST API. Testable without the Android SDK; put logic here, not in `src/App`.
- `src/App` — MAUI shell: XAML views (compiled bindings to `App.Core` ViewModels), platform services, `Platforms/Android`.
- `tests/` — one project per `src` project, mirroring its structure, plus architecture tests.

Dependency direction: `Api` → `Application`/`Infrastructure` → `Domain`; `App` → `App.Core` → `Contracts` + `Domain`. `Domain` and `Application` never reference EF Core, ASP.NET Core, Redis or Firebase types.

## Privacy non-negotiables

- Phone numbers are never persisted, logged, traced or returned in errors in plaintext — server-side storage is `HMAC-SHA256(key, E164)` only. Normalization and hashing each live in exactly one place.
- No PII (numbers, names, emails) in logs, exceptions, metrics or error responses, including as Serilog structured properties. Log templates use named placeholders, never string interpolation.
- Never persist which user looked up which number.
- Names are stored encrypted with a per-number derived key (`AesGcmNameProtector`) and shown only when at least `K = 3` distinct contributors agree. Reputation thresholds live only in `Domain/Reputation/ReputationRules`.
- Phone numbers travel in request bodies, never in URLs (request logging records paths).
- Secret keys never live in the repo or the database.
- Lookup, report, contact-upload and public endpoints are rate-limited.
- Never write user-facing privacy claims stronger than spec §2 allows ("impossible", "anonymous", "irreversible" are banned).

## Branding

- Never name, compare to, or describe Tranqui as a clone/alternative of any third-party product or brand — not in code, identifiers, comments, docs, commits, store listings or UI text. Describe it only by what it does (privacy-first caller ID and spam blocker).

## Coding conventions

- Braces on every `if`/`for`/`foreach`/`while` (`IDE0011` = error).
- Private fields without leading underscore, plain camelCase (`IDE1006` = error).
- `EnforceCodeStyleInBuild` + `TreatWarningsAsErrors` in `Directory.Build.props`; fix violations, never suppress them.
- Small single-purpose classes and methods; no dead or commented-out code; named constants instead of magic numbers/strings (all scoring thresholds included).
- Handlers stay thin; business rules live in `Domain`.
- Code, identifiers and commits in English; user-facing text in Spanish (es-CO) via resource files.
- UI: every state conveys meaning with icon + text, never color alone; WCAG AA contrast; 48dp touch targets; TalkBack labels.

## Git

- Never commit to `main`. Work on a branch, open a PR; merging is a human decision.
- Conventional Commits (`feat:`, `fix:`, `test:`, `docs:`, `chore:`), small commits.
- Run the backend build and tests before every push.
- No AI attribution anywhere: no `Co-Authored-By`/session trailers in commits, no "Generated with" lines in PRs, no credit in files or comments. Commits are authored as the repository owner.
