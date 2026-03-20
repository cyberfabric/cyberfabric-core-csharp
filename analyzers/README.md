# Analyzers

## Abstract

AI-generated code can be inconsistent and may violate project-specific rules: wrong APIs (e.g. `Console.WriteLine` instead of logging), banned types (e.g. `HttpClient` in domain logic), or invalid patterns (e.g. ad-hoc `[Route]` attributes instead of centralized routing). Relying only on prompts and reviews does not guarantee that these rules are followed. This project enforces such rules **deterministically** at compile time using custom Roslyn analyzers, so that builds fail when the rules are broken—whether the code was written by a human or by AI.

## The Problem

Different project types and layers have different constraints. Those constraints must be enforced reliably:

- **ASP.NET / API projects:** Direct use of `System.Console` is not allowed; a logging abstraction (e.g. `ILogger`) must be used instead so that output is controllable and consistent in production.
- **Domain / application logic:** `HttpClient` must never be used directly (socket exhaustion, testability, lifecycle); an abstraction such as `IHttpClientFactory` or a dedicated client interface should be used.
- **ASP.NET Web APIs:** Explicit `[Route]` attributes can be forbidden so routing stays centralized (conventional routing, endpoint mapping, or a single approved pattern).

Without machine-checkable rules, these policies are easy to break, especially when code is generated or copied. We need a **deterministic, build-time** enforcement mechanism.

## Solution: Roslyn Analyzers

The approach is to implement **custom Roslyn analyzers** that run during compilation and report **errors** (so the build fails) when a rule is violated. Benefits:

- **Deterministic:** Same code always gets the same result; no dependence on AI or human discipline.
- **Immediate feedback:** Violations are reported in the IDE and in the build, with a clear location and message.
- **Composable:** Each rule lives in its own analyzer; projects reference only the analyzers that apply to them (e.g. API project gets **NoExplicitRouteAttributes**, domain project gets **NoHttpClient**, shared libraries get **NoConsoleUsage** / **NoTaskResult** / **NoAsyncVoid** as needed).
- **Familiar tooling:** Works with existing .NET builds, CI, and editor experience.

Analyzers are grouped by the **kind of rule** they enforce (forbidden type, API pattern, etc.). Shared logic (e.g. “is this symbol a forbidden type?”) can be extracted into a common layer so that adding a new “forbidden type” analyzer is mostly configuration.

## Analyzer Groups and Patterns

Analyzers in this repo are organized by usage pattern:

| Pattern              | Purpose                                      | Example analyzers              |
|----------------------|----------------------------------------------|--------------------------------|
| **Forbidden type**   | Disallow use of a specific type entirely      | NoConsoleUsage, NoHttpClient   |
| **API / framework**  | Enforce framework-specific rules (e.g. routes)| NoExplicitRouteAttributes      |

Common analyzer code (e.g. “resolve type from operation”, “report at location”) can be factored into shared helpers or a small shared project so new forbidden-type analyzers stay thin and consistent.

## Analyzers and Samples

Each analyzer has a corresponding **sample project** that references the analyzer. The samples intentionally contain violations so that building them demonstrates the diagnostics.

## Using the Analyzers in Your Projects

Reference the analyzer project as an analyzer (not as a normal assembly reference):

```xml
<ItemGroup>
  <ProjectReference Include="path/to/CyberFabric.Analyzers.NoConsoleUsage.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

Apply only the analyzers that match your project type (e.g. API project: **NoExplicitRouteAttributes**; domain project: **NoHttpClient**). Some analyzers only run when relevant types exist in the compilation (e.g. **NoExplicitRouteAttributes** when ASP.NET Core MVC is referenced).

## Solution Structure

- **DE02_api_layer**, **DE03_domain_layer**, **DE13_common_patterns** — Analyzer projects grouped by layer/pattern (see each folder’s `README.md`).
- **Samples/** — Sample projects that reference analyzers and contain intentional violations to demonstrate the rules.

By enforcing these rules at compile time, we make AI-generated and human-written code adhere to the same deterministic, project-specific policies.
