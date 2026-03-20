# DE03 — Domain layer

Roslyn analyzers for **domain** and **core business** assemblies. They keep infrastructure concerns (e.g. raw HTTP clients) out of the domain so logic stays testable and free of transport details.

## Analyzers

| Project | Diagnostic | Short description |
|--------|------------|-------------------|
| [CyberFabric.Analyzers.NoHttpClient](./CyberFabric.Analyzers.NoHttpClient/) | **DE0301** | Disallows direct use of `HttpClient`; prefer abstractions such as `IHttpClientFactory` or application-defined gateways. |

## Referencing in a project

```xml
<ProjectReference Include="path/to/CyberFabric.Analyzers.NoHttpClient.csproj"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false" />
```
