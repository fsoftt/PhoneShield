# PhoneShield

Open-source, privacy-first caller ID and spam blocker: a .NET MAUI Android app + ASP.NET Core API that identifies callers and blocks spam using crowd-sourced, HMAC-hashed data. Initial market: Colombia.

**Source of truth for product decisions:** `docs/especificacion-v1.md` (Spanish). Legal notes: `docs/legal-colombia.md`. Read the spec before implementing any feature; if code and spec disagree, ask instead of guessing.

## Stack

- API: ASP.NET Core minimal APIs, `net10.0`. PostgreSQL (EF Core + Npgsql), Redis (cache + rate limiting).
- App: .NET MAUI, Android only (`net10.0-android`, min API 29). MVVM with `CommunityToolkit.Mvvm`; `CommunityToolkit.Maui`; Refit for HTTP; SQLite for local data.
- CQRS with MediatR **12.x only** (v13+ is commercially licensed). Validation with FluentValidation via a MediatR pipeline behavior.
- Auth: Firebase Authentication (email/password, Google, Apple). The API validates the Firebase JWT on every request; the user is identified by the token's `uid`, never by a client-supplied value.
- Phone normalization: `libphonenumber-csharp`, E.164, default region `CO`.
- Logging: Serilog (Console + Seq in dev).
- Tests: xUnit, FluentAssertions **7.x only** (v8 is commercially licensed), NSubstitute, Testcontainers, NetArchTest.
- Budget is minimal: prefer free tiers and self-hosted components; flag any paid dependency before adding it.

## Layout

- `src/Domain` — aggregates, value objects, scoring and name rules. Organized by aggregate. Depends on nothing.
- `src/Application` — vertical slices at `Features/<FeatureName>/` (command/query, handler, validator, response DTO together). Depends only on `Domain`.
- `src/Infrastructure` — EF Core, Redis, Firebase Admin, hashing/encryption. Organized by aggregate/technology.
- `src/Api` — one minimal API endpoint file per slice at `Endpoints/<FeatureName>.cs`.
- `src/Contracts` — DTOs shared by `Api` and `App`.
- `src/App` — MAUI app (Views, ViewModels, Services, Platforms/Android).
- `tests/` — one project per `src` project, mirroring its structure, plus architecture tests.

Dependency direction: `Api` → `Application`/`Infrastructure` → `Domain`; `App` → `Contracts` + `Domain`. `Domain` and `Application` never reference EF Core, ASP.NET Core, Redis or Firebase types.

## Privacy non-negotiables

- Phone numbers are never persisted, logged, traced or returned in errors in plaintext — server-side storage is `HMAC-SHA256(key, E164)` only. Normalization and hashing each live in exactly one place.
- No PII (numbers, names, emails) in logs, exceptions, metrics or error responses, including as Serilog structured properties. Log templates use named placeholders, never string interpolation.
- Never persist which user looked up which number.
- Names are stored encrypted with a per-number derived key and shown only when at least `K = 3` distinct contributors agree.
- Secret keys never live in the repo or the database.
- Lookup, report, contact-upload and public endpoints are rate-limited.
- Never write user-facing privacy claims stronger than spec §2 allows ("impossible", "anonymous", "irreversible" are banned).

## Branding

- Never name, compare to, or describe PhoneShield as a clone/alternative of any third-party product or brand — not in code, identifiers, comments, docs, commits, store listings or UI text. Describe it only by what it does (privacy-first caller ID and spam blocker).

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
- Run `dotnet build` and `dotnet test` before every push.
- No AI attribution anywhere: no `Co-Authored-By`/session trailers in commits, no "Generated with" lines in PRs, no credit in files or comments. Commits are authored as the repository owner.
