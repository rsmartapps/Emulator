using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Emulator.CGB.SourceGeneratorTests
{
    public class SyntaxReceiver : ISyntaxContextReceiver
    {
        public List<GeneratorTargetClass> TestClasses { get; } = new();
        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if(context.Node is ClassDeclarationSyntax classDeclaration)
            {
                if (classDeclaration.Modifiers.Any(a => a.IsKind(SyntaxKind.PartialKeyword)))
                {
                    var testClassName = classDeclaration.Identifier.Text;

                    string namespaceDeclaration = classDeclaration.Ancestors().OfType<NamespaceDeclarationSyntax>().Reverse().FirstOrDefault().Name.ToFullString();
                    TestClasses.Add(new GeneratorTargetClass(namespaceDeclaration, testClassName));
                }
            }
        }
    }
}