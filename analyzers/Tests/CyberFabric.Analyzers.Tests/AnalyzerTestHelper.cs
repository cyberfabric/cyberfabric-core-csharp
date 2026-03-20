using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace CyberFabric.Analyzers.Tests;

internal static class AnalyzerTestHelper
{
    /// <summary>1-based line and column for the start of <paramref name="marker"/> in <paramref name="source"/>.</summary>
    public static (int Line, int Column) GetLineColumn(string source, string marker)
    {
        var i = source.IndexOf(marker, StringComparison.Ordinal);
        Assert.True(i >= 0, $"Marker not found: {marker}");
        var line = 1;
        var lineStart = 0;
        for (var p = 0; p < i; p++)
        {
            if (source[p] == '\n')
            {
                line++;
                lineStart = p + 1;
            }
        }
        var column = i - lineStart + 1;
        return (line, column);
    }

    public static Task RunAnalyzerTestAsync<TAnalyzer>(
        string source,
        params DiagnosticResult[] expected)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestState = { Sources = { source } },
        };
        test.TestState.ExpectedDiagnostics.AddRange(expected);
        return test.RunAsync();
    }

    public static Task RunAnalyzerTestAsync<TAnalyzer>(
        string source,
        IEnumerable<MetadataReference> additionalReferences,
        params DiagnosticResult[] expected)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestState = { Sources = { source } },
        };
        foreach (var r in additionalReferences)
            test.TestState.AdditionalReferences.Add(r);
        test.TestState.ExpectedDiagnostics.AddRange(expected);
        return test.RunAsync();
    }

    public static Task RunAnalyzerTestAsync<TAnalyzer>(
        string source,
        IEnumerable<MetadataReference> additionalReferences)
        where TAnalyzer : DiagnosticAnalyzer, new()
        => RunAnalyzerTestAsync<TAnalyzer>(
            source,
            additionalReferences,
            System.Array.Empty<DiagnosticResult>());
}
