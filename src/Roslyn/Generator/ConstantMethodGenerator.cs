using System.Globalization;
using System.Reflection;
using System.Text;
using Kehlet.Generators.ConstantMethod;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Generator;

using static SyntaxTarget;
using static StaticContentModule;
using BF = BindingFlags;
using static TargetFilterOptions;
using static SymbolDisplayMiscellaneousOptions;

[Generator(LanguageNames.CSharp)]
public partial class ConstantMethodGenerator : IIncrementalGenerator
{
    private static readonly string AttributeFullName = typeof(ConstantMethodAttribute).FullName!;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource<ConstantMethodAttribute>(ConstantMethodAttributeSource);
        });

        var provider = context.CreateTargetProvider(AttributeFullName, Static | Partial, Method, Transform).Choose();

        context.RegisterImplementationSourceOutput(provider, GenerateSource);
    }

    private static Option<TargetMethodModel> Transform(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        object ConvertArray(TypedConstant constant)
        {
            // oh god. im in too deep. can't quit now.
            var elementTypeSymbol = ((IArrayTypeSymbol)constant.Type!).ElementType;
            var assemblyName = elementTypeSymbol.ContainingAssembly.Identity.Name;
            var assembly = AppDomain.CurrentDomain.GetAssemblies().First(x => x.GetName().Name == assemblyName);
            var typeName = elementTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.RemoveMiscellaneousOptions(UseSpecialTypes)
                                                                                .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
            var elementType = assembly.GetType(typeName);

            var values = constant.Values.Select(TypedConstantToValue).ToArray();

            var array = Array.CreateInstance(elementType, constant.Values.Length);
            for (int i = 0; i < array.Length; i++)
            {
                array.SetValue(Convert.ChangeType(values[i], elementType), i);
            }

            return array;
        }

        object? TypedConstantToValue(TypedConstant constant)
        {
            return constant switch
            {
                { Kind: TypedConstantKind.Array } => ConvertArray(constant),
                _                                 => constant.Value
            };
        }

        var methodSyntax = (MethodDeclarationSyntax)context.TargetNode;

        var module = SyntaxHelper.GetTargetWithContext(methodSyntax);
        if (module.IsNone)
        {
            return Option.None;
        }

        var args = context.Attributes.First().ConstructorArguments;

        var targetMethodName = (string)args[0].Value!;
        var targetMethodArgs = args[1].Values.Select(TypedConstantToValue).ToArray();

        var type = (TypeDeclarationSyntax)methodSyntax.Parent!;

        var implementationNode = type.Members.First(node => node is MethodDeclarationSyntax method && method.Identifier.ValueText == targetMethodName);
        var implementation = implementationNode.ToString();

        return Some(new TargetMethodModel(module.UnsafeValue, implementation, targetMethodArgs));
    }

    private static void GenerateSource(SourceProductionContext context, TargetMethodModel model)
    {
        try
        {
            // We create a new assembly with a single static class containing a copy-pasta of the user implementation.
            // We then compile and dynamically invoke the method.
            // I don't know if there's a simpler way to do this as I didn't stop to think before doing it.
            var source =
                $$"""
                using System;
                using System.Collections.Generic;
                using System.Linq;

                public static class Container
                {
                    {{model.MethodSource}}
                }
                """;

            var compilation = CSharpCompilation.Create(
                "ContainerAssembly",
                [CSharpSyntaxTree.ParseText(source)],
                [
#pragma warning disable RS1035 // ㅋㅋㅋ
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("netstandard")).Location),
                    MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime")).Location),
                    MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Linq")).Location)
#pragma warning restore RS1035
                ],
                new(outputKind: OutputKind.DynamicallyLinkedLibrary)
            );

            var outputStream = new MemoryStream();

            var emitResult = compilation.Emit(outputStream);
            if (!emitResult.Success)
            {
                throw new("Diagnostic: " + emitResult.Diagnostics[0].GetMessage());
            }

#pragma warning disable RS1035 // ㅋㅋㅋ
            var assembly = Assembly.Load(outputStream.ToArray());
#pragma warning restore RS1035

            var container = assembly.GetTypes().First();
            var method = container.GetMethods(BF.Public | BF.Static)[0];
            var result = method.Invoke(null, model.Args);
            var resultString = result switch
            {
                string s => $"\"{s}\"",
                IFormattable formattable => formattable.ToString("R", CultureInfo.InvariantCulture) +
                                            formattable switch
                                            {
                                                uint    => "u",
                                                long    => "l",
                                                ulong   => "ul",
                                                float   => "f",
                                                double  => "d",
                                                decimal => "m",
                                                _       => ""
                                            }
            };

            var resultSource = new Emitter(resultString).Visit(model.Module).UnsafeValue.ToString();

            context.AddSource($"{method.Name}.g", SourceText.From(resultSource, Encoding.UTF8));
        }
        catch (Exception e)
        {
            context.ReportDiagnostic(Diagnostic.Create("CONSTANT0001", "Usage", $"{e.Message}", DiagnosticSeverity.Error, DiagnosticSeverity.Error, true, 0));
        }
    }
}
