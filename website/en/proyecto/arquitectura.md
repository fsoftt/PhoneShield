# How it is built

Tranqui is open source: you can check that it does what it says on [GitHub](https://github.com/fsoftt/Tranqui).

## Technology

<div class="stack-grid">
  <div><strong>App</strong>.NET MAUI for Android, MVVM with CommunityToolkit</div>
  <div><strong>Backend</strong>.NET 10, ASP.NET Core minimal APIs, MediatR 12, FluentValidation</div>
  <div><strong>Data</strong>PostgreSQL 17 with EF Core 10</div>
  <div><strong>Accounts</strong>Firebase Authentication, tokens validated on the server</div>
  <div><strong>Cryptography</strong>HMAC-SHA256, AES-256-GCM, HKDF</div>
  <div><strong>Testing</strong>xUnit v3, Testcontainers, architecture tests</div>
  <div><strong>Deployment</strong>Docker Compose and Caddy with automatic HTTPS</div>
  <div><strong>CI</strong>GitHub Actions: backend, Docker image and Android app</div>
</div>

## Architecture

- **Domain-Driven Design:** the domain (numbers, reputation, accounts) depends on nothing external.
- **Clean Architecture with vertical slices:** each feature keeps its command or query, validator and handler together.
- **Architecture tests** that fail if a layer depends on one it must not.
- **App.Core:** all the app's logic (deciding what to do with a call, the session, the ViewModels) is plain .NET and tested without an emulator.

## Privacy by design

- The number is normalized (international E.164 format) and turned into a code in **exactly one place** in the code.
- Numbers travel in request bodies, never in URLs, so they never end up in access logs.
- Logs contain no numbers, names or emails.
- Secret keys are never in the repository or the database, and can be rotated.

## License

[AGPL-3.0](https://github.com/fsoftt/Tranqui/blob/main/LICENSE): anyone can use, study and modify the code, and
whoever offers a modified version as a service must publish their changes.
