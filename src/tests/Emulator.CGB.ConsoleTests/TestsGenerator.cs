﻿//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.Text;
//using System.Text;

//namespace Emulator.CGB.SourceGeneratrTests;

//[Generator]
//public class TestsGenerator : ISourceGenerator
//{
//    public void Execute(GeneratorExecutionContext context)
//    {
//        foreach(var file in Directory.EnumerateFiles("Tests"))
//        {
//            StringBuilder sb = new StringBuilder();
//            var fileName = new FileInfo(file).Name.Replace(" ", "_");
//            sb.Append(@"
//// <auto-generated/>
//namespace Emulator.CGB.CPU.Tests;");
//            sb.AppendLine($"public class {fileName}_Tests");
//            sb.Append(@"

//{
//    [Theory]
//    [MemberData(nameof(LoadUnitTestsData),""remplaceMePath"")]
//    public void remplaceMeTestName(UnitTest test)
//    {
//        var memoryManager = new MemoryManager();
//        var gbcpu = new CGBCPU(memoryManager);
//        Common.InitializeCPU(gbcpu, test.initial);
//        gbcpu.ProcessOperation();
//        var actual = Common.GetCPUAsFinal(gbcpu);
//        Assert.Equivalent(test.final, actual);
//    }

//    public static IEnumerable<Object[]> LoadUnitTestsData(string filePath)
//    {
//        var jsonTest = File.ReadAllText(filePath);
//        foreach (var unitTest in System.Text.Json.JsonSerializer.Deserialize<IEnumerable<UnitTest>>(jsonTest))
//        {
//            yield return new object[1] { unitTest };
//        }
//    }
//}");
//            context.AddSource($"{fileName}.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
//        }
//    }

//    public void Initialize(GeneratorInitializationContext context)
//    {
//        throw new NotImplementedException();
//    }
//}
