# DE13 — Common patterns

Roslyn analyzers that apply across many project types (API, workers, libraries). They target widespread foot-guns: blocking on tasks, async misuse, and console I/O where structured logging is expected.

## Analyzers

| Project | Diagnostic | Short description |
|--------|------------|-------------------|
| [CyberFabric.Analyzers.NoConsoleUsage](./CyberFabric.Analyzers.NoConsoleUsage/) | **DE1302** | Forbids `System.Console`; use logging (`ILogger`, etc.). |
| [CyberFabric.Analyzers.NoTaskResult](./CyberFabric.Analyzers.NoTaskResult/) | **DE1303** | Forbids `Task.Result` and `Task.GetAwaiter().GetResult()` blocking patterns. |
| [CyberFabric.Analyzers.NoAsyncVoid](./CyberFabric.Analyzers.NoAsyncVoid/) | **DE1301** | Forbids `async void` (methods, lambdas, local functions). |

## Referencing in a project

Use the analyzers that match your policy; each is a separate package/project:

```xml
<ProjectReference Include="path/to/CyberFabric.Analyzers.NoConsoleUsage.csproj"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false" />
```

Repeat for `NoTaskResult` and/or `NoAsyncVoid` as needed.
