using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CyberFabric.Analyzers.NoConsoleUsage;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NoConsoleUsageAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "DE1302";
    private const string ConsoleTypeName = "System.Console";

    private static readonly LocalizableString Title = "Usage of System.Console is not allowed";
    private static readonly LocalizableString MessageFormat = "Usage of System.Console is not allowed";
    private static readonly LocalizableString Description = "System.Console must not be used in this project.";
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
            var consoleType = compilationStartContext.Compilation.GetTypeByMetadataName(ConsoleTypeName);
            if (consoleType is null)
                return;

            compilationStartContext.RegisterOperationAction(operationContext =>
            {
                ITypeSymbol? type = operationContext.Operation switch
                {
                    IInvocationOperation invocation => invocation.TargetMethod.ContainingType,
                    IMemberReferenceOperation memberRef => memberRef.Member.ContainingType,
                    IObjectCreationOperation objectCreation => objectCreation.Type,
                    _ => null
                };

                if (type is not null && IsConsoleType(type, consoleType))
                {
                    operationContext.ReportDiagnostic(
                        Diagnostic.Create(
                            Rule,
                            operationContext.Operation.Syntax.GetLocation()));
                }
            }, OperationKind.Invocation, OperationKind.MethodReference, OperationKind.PropertyReference,
                OperationKind.FieldReference, OperationKind.EventReference, OperationKind.ObjectCreation);

            compilationStartContext.RegisterSyntaxNodeAction(syntaxContext =>
            {
                if (syntaxContext.Node is not UsingDirectiveSyntax usingDirective)
                    return;

                if (usingDirective.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
                {
                    var symbol = syntaxContext.SemanticModel
                        .GetSymbolInfo(usingDirective.Name, syntaxContext.CancellationToken)
                        .Symbol as INamedTypeSymbol;
 
                    if (symbol is not null && SymbolEqualityComparer.Default.Equals(symbol, consoleType))
                    {
                        syntaxContext.ReportDiagnostic(
                            Diagnostic.Create(
                                Rule,
                                usingDirective.GetLocation()
                            ));
                    }
                }
            }, SyntaxKind.UsingDirective);
        });
    }

    private static bool IsConsoleType(ITypeSymbol type, INamedTypeSymbol consoleType)
    {
        for (var current = type; current is not null; current = current.ContainingType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, consoleType))
                return true;
        }
        return false;
    }
}
