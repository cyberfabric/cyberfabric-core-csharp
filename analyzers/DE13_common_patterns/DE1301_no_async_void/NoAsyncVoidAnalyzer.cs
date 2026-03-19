using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CyberFabric.Analyzers.NoAsyncVoid;

/// <summary>
/// Disallows <c>async void</c> methods (including lambdas and local functions). Use <c>async Task</c> or <c>async ValueTask</c> instead.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NoAsyncVoidAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "DE1301";

    private static readonly LocalizableString Title = "async void is not allowed";
    private static readonly LocalizableString MessageFormat =
        "Do not declare async void methods. Use async Task, async Task<T>, or async ValueTask instead.";
    private static readonly LocalizableString Description =
        "async void makes exception handling unreliable and should not be used except in rare framework patterns; this project forbids it entirely.";
    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(symbolContext =>
        {
            if (symbolContext.Symbol is not IMethodSymbol method)
                return;
            if (!method.IsAsync || !method.ReturnsVoid)
                return;

            foreach (var syntaxRef in method.DeclaringSyntaxReferences)
            {
                var node = syntaxRef.GetSyntax(symbolContext.CancellationToken);
                var location = node switch
                {
                    MethodDeclarationSyntax m => m.Identifier.GetLocation(),
                    LocalFunctionStatementSyntax l => l.Identifier.GetLocation(),
                    AnonymousFunctionExpressionSyntax a => a.AsyncKeyword.RawKind != 0
                        ? a.AsyncKeyword.GetLocation()
                        : a.GetLocation(),
                    _ => node.GetLocation()
                };

                if (location.SourceTree is not null)
                    symbolContext.ReportDiagnostic(Diagnostic.Create(Rule, location));
            }
        }, SymbolKind.Method);
    }
}
