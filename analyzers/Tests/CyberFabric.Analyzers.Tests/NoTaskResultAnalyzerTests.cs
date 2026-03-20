using CyberFabric.Analyzers.NoTaskResult;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace CyberFabric.Analyzers.Tests;

using Verify = CSharpAnalyzerVerifier<NoTaskResultAnalyzer, DefaultVerifier>;

public class NoTaskResultAnalyzerTests
{
    [Fact]
    public async Task Task_Result_reports_DE1303()
    {
        var code = @"
using System.Threading.Tasks;
class C
{
    static void M()
    {
        var t = Task.FromResult(1);
        var x = t.Result;
    }
}";
        var (line, col) = AnalyzerTestHelper.GetLineColumn(code, "t.Result");
        await Verify.VerifyAnalyzerAsync(code, Verify.Diagnostic().WithLocation(line, col));
    }

    [Fact]
    public async Task GetAwaiter_GetResult_reports_DE1303()
    {
        var code = @"
using System.Threading.Tasks;
class C
{
    static void M()
    {
        Task.CompletedTask.GetAwaiter().GetResult();
    }
}";
        var (line, col) = AnalyzerTestHelper.GetLineColumn(code, "Task.CompletedTask.GetAwaiter().GetResult()");
        await Verify.VerifyAnalyzerAsync(code, Verify.Diagnostic().WithLocation(line, col));
    }

    [Fact]
    public async Task Await_no_diagnostic()
    {
        var code = @"
using System.Threading.Tasks;
class C
{
    static async Task M()
    {
        await Task.Delay(0);
    }
}";
        await Verify.VerifyAnalyzerAsync(code);
    }
}
