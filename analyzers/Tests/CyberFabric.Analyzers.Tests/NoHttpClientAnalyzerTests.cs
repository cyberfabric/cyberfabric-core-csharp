using CyberFabric.Analyzers.NoHttpClient;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace CyberFabric.Analyzers.Tests;

using Verify = CSharpAnalyzerVerifier<NoHttpClientAnalyzer, DefaultVerifier>;

public class NoHttpClientAnalyzerTests
{
    [Fact]
    public async Task New_HttpClient_reports_DE0301()
    {
        var code = @"
using System.Net.Http;
class C
{
    static void M()
    {
        var c = new HttpClient();
    }
}";
        var (line, col) = AnalyzerTestHelper.GetLineColumn(code, "new HttpClient()");
        await Verify.VerifyAnalyzerAsync(code, Verify.Diagnostic().WithLocation(line, col));
    }

    [Fact]
    public async Task No_HttpClient_no_diagnostic()
    {
        var code = @"
using System;
class C
{
    static void M()
    {
        var x = Uri.TryCreate(""https://a"", UriKind.Absolute, out _);
    }
}";
        await Verify.VerifyAnalyzerAsync(code);
    }
}
