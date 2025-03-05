using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CSharp;

namespace Generator;

[Generator(LanguageNames.CSharp)]
public class ConstantMethodGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxForAttribute(
            "Kehlet.Generators.ConstantMethodGenerator.ConstantMethodAttribute",
            (node, token) => SyntaxTarget.Method(node, token)
                             && node is MethodDeclarationSyntax m
                             && m.Modifiers.Any(SyntaxKind.StaticKeyword)
                             && !m.Modifiers.Any(SyntaxKind.PartialKeyword),
            Transform
        );
        context.RegisterImplementationSourceOutput(provider, GenerateSource);
    }

    private static string Transform(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        var methodSyntax = (MethodDeclarationSyntax)context.TargetNode;
        return methodSyntax.WithAttributeLists([]).ToFullString();
    }

    private static void Write(SourceProductionContext context, string message) =>
        context.ReportDiagnostic(Diagnostic.Create("TEST", "Usage", message, DiagnosticSeverity.Warning, DiagnosticSeverity.Warning, true, 1));

    private static void GenerateSource(SourceProductionContext context, string methodSource)
    {
        Write(context, methodSource.ReplaceLineEndings("\\"));

        try
        {
            var source =
                $$"""
                using System;

                public static class Container
                {
                    {{methodSource}}
                }
                """;

            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            var compilation = CSharpCompilation.Create(
                "ContainerAssembly",
                [syntaxTree],
                [MetadataReference.CreateFromFile(typeof(Object).Assembly.Location)],
                new(outputKind: OutputKind.DynamicallyLinkedLibrary)
            );

            var outputStream = new MemoryStream();

            var emitResult = compilation.Emit(outputStream);
            if (!emitResult.Success)
            {
                throw new("Diagnostic: " + emitResult.Diagnostics[0].GetMessage());
            }

            Assembly assembly;
            try
            {
#pragma warning disable RS1035
                assembly = Assembly.Load(outputStream.ToArray());
#pragma warning restore RS1035
            }
            catch (Exception)
            {
                Write(context, "Load failed");
                throw;
            }

            var container = assembly.GetTypes().First();
            var method = container.GetMethods(BindingFlags.Public | BindingFlags.Static)[0];
            var result = method.Invoke(null, []);
            var resultString = result switch
            {
                string s => $"\"{s}\"",
                int n => $"{n}"
            };

            var resultSource =
                $$"""
                internal static class Container_{{method.Name}}
                {
                    public static object {{method.Name}}() => {{resultString}};
                }
                """;

            context.AddSource($"{method.Name}.g", SourceText.From(resultSource, Encoding.UTF8));
        }
        catch (Exception e)
        {
            context.ReportDiagnostic(Diagnostic.Create("TEST", "Usage", $"{e.Message}", DiagnosticSeverity.Error, DiagnosticSeverity.Error, true, 0));
        }
    }
}
