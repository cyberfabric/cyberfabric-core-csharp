using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CyberFabric.Analyzers.NoHttpClient;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NoHttpClientAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "DE0301";
    private const string HttpClientTypeName = "System.Net.Http.HttpClient";

    private static readonly LocalizableString Title = "Usage of HttpClient is not allowed";
    private static readonly LocalizableString MessageFormat = "Usage of HttpClient is not allowed";
    private static readonly LocalizableString Description = "HttpClient must not be used in this project. Prefer IHttpClientFactory or similar patterns.";
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
            var httpClientType = compilationStartContext.Compilation.GetTypeByMetadataName(HttpClientTypeName);
            if (httpClientType is null)
                return;

            compilationStartContext.RegisterOperationAction(operationContext =>
            {
                ITypeSymbol? type = operationContext.Operation switch
                {
                    IObjectCreationOperation objectCreation => objectCreation.Type,
                    IInvocationOperation invocation => invocation.TargetMethod.ContainingType,
                    IMemberReferenceOperation memberRef => memberRef.Member switch
                    {
                        IFieldSymbol field => field.Type,
                        IPropertySymbol property => property.Type,
                        _ => memberRef.Member.ContainingType
                    },
                    IVariableDeclarationOperation variableDeclaration => variableDeclaration.Type,
                    IParameterReferenceOperation parameterRef => parameterRef.Parameter.Type,
                    _ => null
                };

                if (type is not null && IsHttpClientType(type, httpClientType))
                {
                    operationContext.ReportDiagnostic(
                        Diagnostic.Create(
                            Rule,
                            operationContext.Operation.Syntax.GetLocation()
                            ));
                }
            }, OperationKind.ObjectCreation, OperationKind.Invocation,
                OperationKind.MethodReference, OperationKind.PropertyReference,
                OperationKind.FieldReference, OperationKind.EventReference,
                OperationKind.VariableDeclaration, OperationKind.ParameterReference);
        });
    }

    private static bool IsHttpClientType(ITypeSymbol type, INamedTypeSymbol httpClientType)
    {
        for (var current = type; current is not null; current = current.ContainingType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, httpClientType))
                return true;
        }
        return false;
    }
}
