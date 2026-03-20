using CyberFabric.Analyzers.NoAsyncVoid;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace CyberFabric.Analyzers.Tests;

using Verify = CSharpAnalyzerVerifier<NoAsyncVoidAnalyzer, DefaultVerifier>;

public class NoAsyncVoidAnalyzerTests
{
    [Fact]
    public async Task Async_void_method_reports_DE1301()
    {
        var code = @"
class C
{
    async void M() { }
}";
        var (line, col) = AnalyzerTestHelper.GetLineColumn(code, "void M()");
        await Verify.VerifyAnalyzerAsync(code, Verify.Diagnostic().WithLocation(line, col + 5));
    }

    [Fact]
    public async Task Async_Task_no_diagnostic()
    {
        var code = @"
using System.Threading.Tasks;
class C
{
    async Task M() { await Task.CompletedTask; }
}";
        await Verify.VerifyAnalyzerAsync(code);
    }
}
