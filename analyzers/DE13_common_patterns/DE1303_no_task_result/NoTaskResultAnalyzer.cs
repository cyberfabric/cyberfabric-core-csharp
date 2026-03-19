using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CyberFabric.Analyzers.NoTaskResult;

/// <summary>
/// Disallows blocking waits: <see cref="System.Threading.Tasks.Task{TResult}.Result"/> and
/// <c>Task.GetAwaiter().GetResult()</c> on <see cref="System.Threading.Tasks.Task"/> / <see cref="System.Threading.Tasks.Task{TResult}"/>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NoTaskResultAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "DE1303";
    private const string TaskTypeName = "System.Threading.Tasks.Task";
    private const string GenericTaskTypeName = "System.Threading.Tasks.Task`1";

    private static readonly LocalizableString Title = "Blocking wait on Task is not allowed";
    private static readonly LocalizableString MessageFormat = "Blocking wait on Task is not allowed";
    private static readonly LocalizableString Description =
        "Do not use Task.Result or Task.GetAwaiter().GetResult(); use await or asynchronous APIs instead to avoid deadlocks.";
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
            var taskType = compilationStartContext.Compilation.GetTypeByMetadataName(TaskTypeName);
            var taskOfTType = compilationStartContext.Compilation.GetTypeByMetadataName(GenericTaskTypeName);
            if (taskType is null || taskOfTType is null)
                return;

            compilationStartContext.RegisterOperationAction(operationContext =>
            {
                switch (operationContext.Operation)
                {
                    case IPropertyReferenceOperation prop when
                        prop.Member.Name == "Result" &&
                        prop.Member.ContainingType is INamedTypeSymbol containing &&
                        SymbolEqualityComparer.Default.Equals(containing.OriginalDefinition, taskOfTType) &&
                        IsTaskOrTaskOfT(prop.Instance?.Type, taskType, taskOfTType):
                    {
                        operationContext.ReportDiagnostic(
                            Diagnostic.Create(
                                Rule,
                                prop.Syntax.GetLocation()
                                ));
                        break;
                    }
                    case IInvocationOperation { TargetMethod.Name: "GetResult" } outer when
                        SkipConversions(outer.Instance) is IInvocationOperation inner &&
                        inner.TargetMethod.Name == "GetAwaiter" &&
                        IsTaskOrTaskOfT(inner.Instance?.Type, taskType, taskOfTType):
                    {
                        operationContext.ReportDiagnostic(
                            Diagnostic.Create(
                                Rule,
                                outer.Syntax.GetLocation()
                                ));
                        break;
                    }
                }
            }, OperationKind.PropertyReference, OperationKind.Invocation);
        });
    }

    private static IOperation? SkipConversions(IOperation? operation)
    {
        while (operation is IConversionOperation conv)
            operation = conv.Operand;
        return operation;
    }

    private static bool IsTaskOrTaskOfT(ITypeSymbol? type, INamedTypeSymbol task, INamedTypeSymbol taskOfT)
    {
        if (type is null)
            return false;
        if (type is INamedTypeSymbol named)
        {
            if (SymbolEqualityComparer.Default.Equals(named.OriginalDefinition, task))
                return true;
            if (SymbolEqualityComparer.Default.Equals(named.OriginalDefinition, taskOfT))
                return true;
        }
        return false;
    }
}
