namespace Emulator.CGB.SourceGeneratorTests
{
    public class GeneratorTargetClass
    {
        public GeneratorTargetClass(string namespaceDeclaration, string testClassName)
        {
            NamespaceDeclaration = namespaceDeclaration;
            TestClassName = testClassName;
        }

        public string NamespaceDeclaration { get; }
        public string TestClassName { get; }
    }
}