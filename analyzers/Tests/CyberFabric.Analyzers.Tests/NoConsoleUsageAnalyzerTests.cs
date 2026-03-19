using CyberFabric.Analyzers.NoConsoleUsage;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace CyberFabric.Analyzers.Tests;

using Verify = CSharpAnalyzerVerifier<NoConsoleUsageAnalyzer, DefaultVerifier>;

public class NoConsoleUsageAnalyzerTests
{
    [Fact]
    public async Task Console_WriteLine_reports_DE1302()
    {
        var code = @"
class Program
{
    static void Main()
    {
        System.Console.WriteLine(1);
    }
}";
        var (line, col) = AnalyzerTestHelper.GetLineColumn(code, "System.Console.WriteLine");
        await Verify.VerifyAnalyzerAsync(code, Verify.Diagnostic().WithLocation(line, col));
    }

    [Fact]
    public async Task No_console_usage_no_diagnostic()
    {
        var code = @"
class Program
{
    static void Main()
    {
        var x = 42;
    }
}";
        await Verify.VerifyAnalyzerAsync(code);
    }
}
