# Testing Policy (C#)

This document defines the test strategy, coverage expectations, and CI enforcement for
C# / .NET codebases aligned with Cyber Fabric’s quality bar. It is the single source
of truth for **what must be tested and how** in that context.

**Unit testing**, **integration testing**, and **coverage** for those layers are
documented below. Sections for **E2E**, **fuzz**, and **static analysis** remain
**TBD** until tooling and gates are finalized.

---

## 1. Test pyramid

| Layer | Scope | Tooling | Feature gate | Runs in CI |
|-------|-------|---------|--------------|------------|
| **Unit** | Single method / class / small collaboration in isolation | `dotnet test` (xUnit, NUnit, or MSTest) | none (always compiled with the solution) | Every PR (all OS where applicable) |
| **Integration** | Real or containerized infrastructure (DB, queues, HTTP host in-process) with minimal mocking | `dotnet test` + filters and/or dedicated projects; Testcontainers / Docker; `WebApplicationFactory` (ASP.NET Core) | traits/categories, separate assemblies, or `--filter` so default runs stay fast | Every PR or nightly — *team choice*; document in your pipeline |
| **E2E** | Full request → response through a running host | *TBD* | *TBD* | *TBD* |
| **Fuzz** | Parser / validator robustness against arbitrary input | *TBD* | *TBD* | *TBD* |
| **Static analysis** | Style, correctness, architecture, dependencies, licenses | *TBD* | *TBD* | *TBD* |

### Quick-reference commands

```bash
dotnet test                              # default: unit only if integration is filtered out (see § 4)
dotnet test --filter "Category!=Integration"    # example: exclude integration (xUnit trait; adjust per framework)
dotnet test --filter "Category=Integration"     # example: integration only
dotnet test path/to/MyApp.IntegrationTests.csproj # dedicated integration project
```

---

## 2. Coverage

### 2.1 Threshold

Target a project-wide **line-coverage threshold of 80 %** for **unit** runs, consistent
with the Cyber Fabric quality bar. Enforce the threshold in CI
so builds fail or warn when coverage drops below the configured minimum.

### 2.2 Coverage modes

| Mode | Command | What it measures |
|------|---------|------------------|
| **Unit** | Coverlet / `dotnet-coverage` + `dotnet test` (see § 2.3) | Unit tests (exclude integration via filter if both live in one solution) |
| **Integration** | Same collectors + `dotnet test --filter Category=Integration` (or integration-only project) | Code paths exercised with real DB/containers/host factory |
| **E2E** | *TBD* | *TBD* |
| **Combined** | Merge reports (ReportGenerator, `dotnet-coverage merge`, or CI task) | Unit + integration (+ E2E when defined) |

**Note:** If `dotnet test` without a filter runs both unit and integration, coverage
numbers mix layers. Prefer **separate CI steps** or **filters** so thresholds stay
meaningful (e.g. 80 % on unit; integration as additional signal).

### 2.3 Collecting unit coverage locally

Common approaches on .NET:

| Approach | Command / notes |
|----------|------------------|
| **Coverlet** (via test project package) | `dotnet test /p:CollectCoverage=true` or MSBuild properties on the test project |
| **dotnet-coverage** | `dotnet-coverage collect dotnet test` |
| **Visual Studio** | Test Explorer with code coverage (Enterprise) |

Configure thresholds in **runsettings** (`DataCollectors` + `CodeCoverage`), in
**Coverlet**’s `Threshold` / `ThresholdType`, or in your CI step that parses Cobertura
or OpenCover output.

### 2.4 CI coverage

*TBD* — e.g. collector choice, threshold enforcement in pipeline, upload to Codecov or
equivalent.

### 2.5 Reports

Typical outputs for **unit** coverage (depending on collector):

| Artifact | Use |
|----------|-----|
| Cobertura XML | Azure DevOps, GitHub Actions, Jenkins |
| LCOV | IDE plugins, Codecov |
| HTML (e.g. ReportGenerator) | Local review per file / line |

---

## 3. Unit tests

### 3.1 Expectations

- Every new **public** API surface (types and non-trivial behaviour) **should** have at
  least one unit test that documents intent and guards regressions.
- Prefer **one test project per production project** (e.g. `MyApp.Tests` → `MyApp`) or
  a clear folder convention; keep tests **next to the feature** they protect in the
  solution structure.
- Use **descriptive names**, for example:
  - `MethodName_Scenario_ExpectedResult` (underscore style), or
  - `MethodName_should_expected_when_scenario` (BDD-style).
- Prefer **deterministic** assertions: avoid `Thread.Sleep`, wall-clock timing, or
  reliance on locale unless the test explicitly fixes culture and time.
- **Isolate** dependencies with interfaces, fakes, or mocking libraries; do not bind
  unit tests to production infrastructure.

### 3.2 Placement

- **Same assembly** (optional): `internal` types can be tested via
  `[assembly: InternalsVisibleTo("MyAssembly.Tests")]`.
- **Separate test assembly** (recommended for libraries and services): references the
  project under test; mirrors its folder layout for easy navigation.

### 3.3 Running

```bash
dotnet test                                    # entire solution
dotnet test path/to/MyProject.Tests.csproj     # one test project
dotnet test --filter "FullyQualifiedName~Namespace.ClassName"   # filter by name
dotnet test --configuration Release            # release configuration
```

### 3.4 Framework notes (brief)

- **xUnit**: `[Fact]`, `[Theory]` + `[InlineData]` / `[MemberData]` for data-driven
  cases; parallelizes by default within the assembly (be careful with static shared
  state).

---

## 4. Integration tests

Integration tests verify behaviour across assemblies and **real or realistic**
infrastructure: databases, message brokers, file systems where required, and for web
apps the full pipeline **in-process** via the ASP.NET Core test host.

### 4.1 Expectations

- Add integration coverage when **unit tests alone** cannot prove correctness (e.g. SQL
  mappings, migrations, transaction boundaries, auth middleware + handlers, ORM
  configuration).
- **Prefer**: one integration test proving the “happy path” and **targeted** tests for
  error paths that depend on infrastructure (deadlocks, constraint violations,
  timeouts).
- **Determinism**: isolate data per test (transactions, unique schemas, truncate +
  seed, or **Respawn**-style reset) so order and parallelism do not flake results.
- **Secrets**: use configuration + **user secrets** / CI variables; never commit
  credentials. Point tests at disposable local or CI services.

### 4.2 Separation from unit tests

Choose at least one convention so `dotnet test` in a tight loop stays fast:

| Approach | Pros |
|----------|------|
| **Dedicated project** (e.g. `MyApp.IntegrationTests`) | Clear boundary; CI runs it in a separate job. |
| **Traits / categories** | xUnit: `[Trait("Category", "Integration")]`, NUnit: `[Category("Integration")]`, MSTest: `[TestCategory("Integration")]` — filter with `dotnet test --filter`. |
| **Conditional compilation** | `#if INTEGRATION` around entire fixtures — use sparingly; easy to misconfigure locally. |

Default developer workflow: run **unit** only unless working on integration; CI runs
**both** (or integration on agents with Docker and pre-provisioned services, depending on
your infrastructure).

### 4.3 ASP.NET Core

- **`WebApplicationFactory<TProgram>`** (minimal hosting) or **`WebApplicationFactory<TEntryPoint>`**
  bootstraps the app with **test** configuration (in-memory or real DB connection
  string from env).
- Prefer **`CreateClient()`** for HTTP assertions without binding a real port; use
  **`HttpClient`** + **`BaseAddress`** when you customize the factory.
- Replace external dependencies with **test doubles** only when the real service is
  unavailable; for DB-heavy features, prefer **Testcontainers** or a CI-provided
  instance so behaviour matches production.

### 4.4 Databases

| Option | When to use |
|--------|-------------|
| **SQLite** (file or in-memory) | Fast smoke for EF Core / Dapper where SQL is portable. |
| **Testcontainers** (PostgreSQL, SQL Server, MySQL, etc.) | Parity with production engine and types. |
| **Shared CI instance** | Acceptable if schemas are isolated and cleanup is reliable. |

Run **EF Core migrations** (or baseline schema) before tests in a fixture; avoid relying
on stale shared DB state left by previous PRs.

### 4.5 Containers and external services

- **Testcontainers for .NET** spins up Dockerized dependencies in CI and locally
  (requires Docker).
- Start/stop in a **fixture** (`IAsyncLifetime` in xUnit, `OneTimeSetUp` in NUnit) once
  per assembly or collection to limit startup cost.
- Document **prerequisites** in the repo README (Docker version, ports, resource
  limits).

### 4.6 Running

```bash
dotnet test --filter "Category=Integration"
dotnet test MyApp.IntegrationTests/MyApp.IntegrationTests.csproj
docker compose -f docker-compose.test.yml up -d   # if your suite expects external stack — optional pattern
dotnet test
```

### 4.7 CI

Integration jobs often need:

- Docker (for Testcontainers) or pre-provisioned services.
- Longer timeouts than unit jobs.
- **Sequential** execution if tests contend for one shared database (`dotnet test --maxcpucount:1` or disable parallelization in the test framework for that assembly).

Exact workflow file names and schedules — align with your org. A common split is **fast
unit tests on every PR and OS** versus **integration in a dedicated job** with containers
or shared services.

---

## 5. End-to-end (E2E) tests

*TBD* — host startup, HTTP or UI drivers, environment fixtures, smoke vs full suite,
and CI scheduling.

---

## 6. Fuzz testing

*TBD* — tooling (e.g. SharpFuzz, custom harnesses), targets, corpus, and CI (nightly vs
PR).

---

## 7. Static analysis and safety

*TBD* — analyzers (IDE + `dotnet format`, Roslyn analyzers, StyleCop, Sonar, etc.),
nullable reference types, banned API rules, dependency/license gates (`dotnet list
package --vulnerable`, third-party deny lists), and how they map to CI jobs.

---

## 8. CI / development commands

Primary entry point is the **.NET CLI** and your pipeline (GitHub Actions, Azure
Pipelines, etc.).

```bash
dotnet restore
dotnet build --no-restore

# Unit (example: exclude integration traits)
dotnet test --no-build --verbosity normal --filter "Category!=Integration"

# Integration (dedicated job or local)
dotnet test --no-build --filter "Category=Integration"
# or
dotnet test --no-build path/to/MyApp.IntegrationTests.csproj
```

Typical CI steps:

1. Restore and build the solution (or affected projects).
2. Run **unit** `dotnet test` with coverage and threshold (see § 2).
3. Run **integration** `dotnet test` with required services (Docker, env vars); coverage
   optional or merged separately (§ 2.2).
4. Upload coverage artifacts (Cobertura/LCOV) if used.

Unified automation entry points — *TBD*.

---

## 9. CI pipeline summary

```
PR opened / updated
  └── *TBD* — full workflow naming
        ├── unit tests — dotnet test (exclude integration) + coverage threshold
        ├── integration — dotnet test (integration only / IntegrationTests project) + Docker or service containers
        ├── E2E — TBD
        ├── fuzz — TBD
        └── static analysis / security — TBD

Nightly / scheduled — TBD
```

---

## 10. Contributor checklist

Before opening a PR:

- [ ] `dotnet test` passes for the solution (or scoped projects) on a clean tree —
  **unit** profile (exclude integration if your default includes both).
- [ ] New or changed behaviour has **unit** tests (names and cases match § 3.1).
- [ ] **Unit** coverage for the touched area meets the **80 %** line threshold (or the
  stricter policy your team adopts).
- [ ] If **data access, migrations, or cross-cutting infrastructure** changed:
  **integration** tests added or updated (§ 4); local run documented or scriptable.
- [ ] No broad `#pragma warning disable` or test-only hacks without justification in
  review.
- [ ] *TBD* — E2E updates when public HTTP contracts change.
- [ ] *TBD* — fuzz targets when parser/validator logic changes.
- [ ] *TBD* — static analysis / security gates satisfied per finalized policy.

---

## 11. Related documents

- [TESTING.md](./TESTING.md) — additional testing topics in this repository: integration,
  E2E, fuzz, static analysis, and shared CI conventions.
- [CONTRIBUTING.md](../CONTRIBUTING.md) — development workflow and PR process for this
  repository.
