# DE02 — API layer

Roslyn analyzers intended for **ASP.NET Core API / web host** projects. They enforce routing and HTTP surface rules so the API layer stays consistent with platform conventions (e.g. centralized route configuration instead of scattered attributes).

## Analyzers

| Project | Diagnostic | Short description |
|--------|------------|-------------------|
| [CyberFabric.Analyzers.NoExplicitRouteAttributes](./CyberFabric.Analyzers.NoExplicitRouteAttributes/) | **DE0201** | Forbids `[Route]` / `RouteAttribute`; use conventional routing or endpoint mapping. |

## Referencing in a project

Add the analyzer as an analyzer-only project reference:

```xml
<ProjectReference Include="path/to/CyberFabric.Analyzers.NoExplicitRouteAttributes.csproj"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false" />
```
