using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FunctionsAttributes
{
    [Generator]
    public class FunctionsAttributesGenerator : ISourceGenerator
    {
        private const string _generateFunctionsWithAttributeName = "FunctionsAttributes.GenerateFunctionsWithAttribute";
        private const string _azureFunctionAttributeName = "Microsoft.Azure.Functions.Worker.FunctionAttribute";

        private const string _attributeSource = @"using System;

namespace FunctionsAttributes
{
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
    public partial class GenerateFunctionsWithAttribute : Attribute
    {
        public GenerateFunctionsWithAttribute(Type attributeType)
        {
            // intentionally empty, only used for code generator
        }
    }
}";

        public void Initialize(GeneratorInitializationContext context)
        {
            // Register the attribute source
            context.RegisterForPostInitialization((i) => i.AddSource("GenerateFunctionsWithAttribute.g.cs", _attributeSource));

            // Register a syntax receiver that will be created for each generation pass
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // retrieve the populated receiver 
            if (!(context.SyntaxContextReceiver is SyntaxReceiver receiver))
                return;

            var typesToListen = context
                .Compilation
                .Assembly
                .GetAttributes()
                .Where(ad => ad.AttributeClass.ToDisplayString() == _generateFunctionsWithAttributeName)
                .Select(ad => ad.ConstructorArguments[0].Value as INamedTypeSymbol)
                .ToList();

            if (!typesToListen.Any())
                return;

            context.AddSource($"GenerateFunctionsWithAttribute.Functions.g.cs", SourceText.From(ProcessTypes(typesToListen, receiver.Functions), Encoding.UTF8));
        }

        private string ProcessTypes(List<INamedTypeSymbol> typeSymbols, List<IMethodSymbol> functionSymbols)
        {
            var sb = new StringBuilder();

            AddUsings(sb);

            AddNamespaceBeginning(sb);
            AddClassBeginning(sb);

            AddFunctionsList(sb, typeSymbols, functionSymbols);

            AddClassClosing(sb);
            AddNamespaceClosing(sb);

            return sb.ToString();
        }

        private void AddUsings(StringBuilder sb)
        {
            sb.AppendLine("using System.Collections.ObjectModel;");
            sb.AppendLine();
        }

        private void AddNamespaceBeginning(StringBuilder sb)
        {
            sb.AppendLine($@"namespace FunctionsAttributes");
            sb.AppendLine($@"{{");
        }

        private void AddNamespaceClosing(StringBuilder sb)
        {
            sb.AppendLine($@"}}");
        }

        private void AddClassBeginning(StringBuilder sb)
        {
            sb.Append(new string(' ', 4));
            sb.AppendLine($@"public partial class GenerateFunctionsWithAttribute");
            sb.Append(new string(' ', 4));
            sb.AppendLine($@"{{");
        }

        private void AddClassClosing(StringBuilder sb)
        {
            sb.Append(new string(' ', 4));
            sb.AppendLine($@"}}");
        }

        private void AddFunctionsList(StringBuilder sb, List<INamedTypeSymbol> typeSymbols, List<IMethodSymbol> functionSymbols)
        {
            foreach (var type in typeSymbols)
            {
                var matchingFunctions = functionSymbols
                    .Where(f => f.GetAttributes().Any(ad => ad.AttributeClass.Equals(type, SymbolEqualityComparer.Default)))
                    .Select(f => f.GetAttributes().Single(ad => ad.AttributeClass.ToDisplayString() == _azureFunctionAttributeName))
                    .Select(ad => ad.ConstructorArguments[0].Value as string)
                    .ToList();

                sb.Append(new string(' ', 2 * 4));
                sb.AppendLine($@"public static readonly ReadOnlyCollection<string> {type.Name} = new ReadOnlyCollection<string>(new[]");
                sb.Append(new string(' ', 2 * 4));
                sb.AppendLine($@"{{");

                foreach (var function in matchingFunctions)
                {
                    sb.Append(new string(' ', 3 * 4));
                    sb.AppendLine($@"""{function}"",");
                }

                sb.Append(new string(' ', 2 * 4));
                sb.AppendLine($@"}});");
            }
        }

        /// <summary>
        /// Created on demand before each generation pass
        /// </summary>
        class SyntaxReceiver : ISyntaxContextReceiver
        {
            public List<IMethodSymbol> Functions { get; } = new List<IMethodSymbol>();

            /// <summary>
            /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
            /// </summary>
            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                if (context.Node is MethodDeclarationSyntax methodDeclarationSyntax && methodDeclarationSyntax.AttributeLists.Count > 0)
                {
                    var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax) as IMethodSymbol;
                    if (methodSymbol.GetAttributes().Any(ad => ad.AttributeClass.ToDisplayString() == _azureFunctionAttributeName))
                    {
                        Functions.Add(methodSymbol);
                    }
                }
            }
        }
    }
}
