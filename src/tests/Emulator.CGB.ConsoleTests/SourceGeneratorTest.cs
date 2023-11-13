using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Emulator.CGB.SourceGeneratorTests
{

[Generator]
public class SourceGeneratorTest : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        var syntaxContext = (SyntaxReceiver)context.SyntaxContextReceiver;
        foreach (var testClass in syntaxContext.TestClasses)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"namespace {testClass.NamespaceDeclaration}");
            sb.AppendLine("{");
            sb.AppendLine($"  public class MyFirstTestClass");
            sb.AppendLine("  {");
            sb.AppendLine("  }");
            sb.AppendLine("}");
            context.AddSource($"{testClass}GeneratedTest.cs", SourceText.From(sb.ToString(), Encoding.UTF8));

        }

        // Agregar el código generado al contexto para su compilación
    }

    public void Initialize(GeneratorInitializationContext context)
    {
#if DEBUG
        if (!System.Diagnostics.Debugger.IsAttached)
        {
            System.Diagnostics.Debugger.Launch();
        }
#endif
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }
}
}
