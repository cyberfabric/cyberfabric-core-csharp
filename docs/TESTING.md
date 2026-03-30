# Testing Policy (C#)

This document defines the test strategy, coverage expectations, and CI enforcement for
C# / .NET codebases aligned with Cyber Fabric’s quality bar. It is the single source
of truth for **what must be tested and how** in that context.

Sections for **integration**, **end-to-end**, **fuzz**, and **static analysis** are
placeholders (**TBD**) until tooling, commands, and gates are finalized. The **unit
testing** and **coverage** material below is authoritative today.

---

## 1. Test pyramid

| Layer | Scope | Tooling | Feature gate | Runs in CI |
|-------|-------|---------|--------------|------------|
| **Unit** | Single method / class / small collaboration in isolation | `dotnet test` (xUnit, NUnit, or MSTest) | none (always compiled with the solution) | Every PR (all OS where applicable) |
| **Integration** | Cross-assembly or infrastructure-backed logic (DB, message bus, etc.) | *TBD* | *TBD* | *TBD* |
| **E2E** | Full request → response through a running host | *TBD* | *TBD* | *TBD* |
| **Fuzz** | Parser / validator robustness against arbitrary input | *TBD* | *TBD* | *TBD* |
| **Static analysis** | Style, correctness, architecture, dependencies, licenses | *TBD* | *TBD* | *TBD* |

### Quick-reference commands

```bash
dotnet test                  # unit tests (solution or project)
# Integration / E2E / fuzz / static-analysis commands — TBD
```

---

## 2. Coverage

### 2.1 Threshold

Target a project-wide **line-coverage threshold of 80 %** for **unit** runs (same
intent as [TESTING.md § 2.1](./TESTING.md#21-threshold)). Enforce the threshold in CI
so builds fail or warn when coverage drops below the configured minimum.

### 2.2 Coverage modes

| Mode | Command | What it measures |
|------|---------|------------------|
| **Unit** | Coverlet / `dotnet-coverage` + `dotnet test` (see § 2.3) | Tests in unit test projects |
| **Integration** | *TBD* | *TBD* |
| **E2E** | *TBD* | *TBD* |
| **Combined** | *TBD* | *TBD* |

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
  - `MethodName_Scenario_ExpectedResult` (underscore style).
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

Pick xUnit and stay consistent.

---

## 4. Integration tests

*TBD* — scope (databases, containers, test doubles), project layout, traits/categories,
local vs CI commands, and feature flags if any.

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

Primary entry point for **unit tests** today is the **.NET CLI** and your pipeline
(GitHub Actions, Azure Pipelines, etc.).

```bash
dotnet restore
dotnet build --no-restore
dotnet test --no-build --verbosity normal
```

Typical CI steps for **unit tests** (until the full pipeline is documented):

1. Restore and build the solution (or affected projects).
2. Run `dotnet test` with coverage collection enabled.
3. Fail the job if coverage is below the threshold (script or built-in collector
   option).
4. Upload coverage artifacts (Cobertura/LCOV) to your reporting service if used.

Unified script / Makefile equivalents and commands for other layers — *TBD*.

---

## 9. CI pipeline summary

```
PR opened / updated
  └── *TBD* — full workflow
        ├── unit tests — dotnet test (+ coverage threshold) — as defined above
        ├── integration — TBD
        ├── E2E — TBD
        ├── fuzz — TBD
        └── static analysis / security — TBD

Nightly / scheduled — TBD
```

---

## 10. Contributor checklist

Before opening a PR:

- [ ] `dotnet test` passes for the solution (or scoped projects) on a clean tree.
- [ ] New or changed behaviour has **unit** tests (names and cases match § 3.1).
- [ ] **Unit** coverage for the touched area meets the **80 %** line threshold (or the
  stricter policy your team adopts).
- [ ] No broad `#pragma warning disable` or test-only hacks without justification in
  review.
- [ ] *TBD* — integration tests when DB or external services are touched.
- [ ] *TBD* — E2E updates when public HTTP contracts change.
- [ ] *TBD* — fuzz targets when parser/validator logic changes.
- [ ] *TBD* — static analysis / security gates satisfied per finalized policy.

---

## 11. Related documents

- [TESTING.md](./TESTING.md) — full Cyber Fabric test policy (Rust): integration, E2E,
  fuzz, static analysis, Makefile / `ci.py`.
- [CONTRIBUTING.md](../CONTRIBUTING.md) — development workflow and PR process for this
  repository.
