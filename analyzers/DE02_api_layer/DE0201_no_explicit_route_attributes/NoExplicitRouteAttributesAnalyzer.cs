using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CyberFabric.Analyzers.NoExplicitRouteAttributes;

/// <summary>
/// Disallows any use of <see cref="Microsoft.AspNetCore.Mvc.RouteAttribute"/> ([Route]).
/// Prefer conventional routing, endpoint mapping, or other approved routing patterns.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NoExplicitRouteAttributesAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "DE0201";
    private const string RouteAttributeName = "Microsoft.AspNetCore.Mvc.RouteAttribute";

    private static readonly LocalizableString Title = "Route attribute is not allowed";
    private static readonly LocalizableString MessageFormat =
        "The [Route] attribute is not allowed. Use conventional routing or endpoint configuration instead.";
    private static readonly LocalizableString Description =
        "Explicit RouteAttribute usage is forbidden in this project.";
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

        context.RegisterCompilationStartAction(compilationStartContext =>
        {
            var routeAttribute = compilationStartContext.Compilation.GetTypeByMetadataName(RouteAttributeName);
            if (routeAttribute is null)
                return;

            compilationStartContext.RegisterSyntaxNodeAction(syntaxContext =>
            {
                if (syntaxContext.Node is not AttributeSyntax attributeSyntax)
                    return;

                var cancellationToken = syntaxContext.CancellationToken;
                var attributeSymbol = syntaxContext.SemanticModel.GetSymbolInfo(attributeSyntax, cancellationToken).Symbol;
                if (attributeSymbol is not IMethodSymbol ctorSymbol)
                    return;

                var attributeType = ctorSymbol.ContainingType;
                if (attributeType is null || !SymbolEqualityComparer.Default.Equals(attributeType, routeAttribute))
                    return;

                syntaxContext.ReportDiagnostic(Diagnostic.Create(Rule, attributeSyntax.GetLocation()));
            }, SyntaxKind.Attribute);
        });
    }
}
