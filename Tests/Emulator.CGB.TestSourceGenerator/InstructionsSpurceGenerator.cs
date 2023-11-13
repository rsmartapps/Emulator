using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;

namespace Emulator.CGB.TestSourceGenerator
{
    [Generator]
    public class InstructionsSpurceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            context.AddSource("file.cs", SourceText.From("public class test {}", System.Text.Encoding.UTF8));        }

        public void Initialize(GeneratorInitializationContext context)
        {
            throw new NotImplementedException();
        }
    }
}
