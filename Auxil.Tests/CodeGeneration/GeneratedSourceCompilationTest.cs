using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Auxil.Tests.CodeGeneration;

public sealed class GeneratedSourceCompilationTest
{
    [Fact]
    public void CSharpTryCatchOutputCompiles()
    {
        CodeBuilder<CSharp> code = new();
        code.File("generated.cs")
            .Class("Generated")
            .Method("Run")
            .Body(body => body
                .Try(tryBody => tryBody.Throw("new System.InvalidOperationException()"))
                .Catch("System.InvalidOperationException", "error", catchBody => catchBody.Line("System.Console.WriteLine(error);"))
                .Catch(catchBody => catchBody.Line("System.Console.WriteLine(\"fallback\");"))
                .Finally(finallyBody => finallyBody.Line("System.GC.KeepAlive(\"cleanup\");")))
            .EndMethod()
            .EndClass()
            .EndFile();

        string source = code.Render();
        CSharpCompilation compilation = CSharpCompilation.Create(
            "GeneratedSource",
            [CSharpSyntaxTree.ParseText(source)],
            TrustedPlatformReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using MemoryStream output = new();
        EmitResult result = compilation.Emit(output);

        result.Success.ShouldBeTrue(string.Join(Environment.NewLine, result.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)));
    }

    private static IEnumerable<MetadataReference> TrustedPlatformReferences()
    {
        string trustedAssemblies = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;
        return trustedAssemblies
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path));
    }
}
