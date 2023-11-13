//// See https://aka.ms/new-console-template for more information
//using System.Runtime.Intrinsics.Arm;
//using System.Text;
//using System.Text.RegularExpressions;

//Console.WriteLine("Hello, World!");

//static void Run(string path)
//{
//    foreach (var file in Directory.EnumerateFiles(path))
//    {
//        var sanitizedName = $"OpCode_{GetValidMethodName(Path.GetFileNameWithoutExtension(file))}";
//        var jsonTest = File.ReadAllText(file);
//        var doc = BuildDoc(sanitizedName, System.Text.Json.JsonSerializer.Deserialize<IEnumerable<UnitTest>>(jsonTest));
//        File.WriteAllText($"UnitTests\\{sanitizedName}.cs", doc,Encoding.UTF8);
//    }
//}

//static string BuildDoc(string name, IEnumerable<UnitTest> unitTests)
//{
//    StringBuilder doc = new StringBuilder();
//    doc.AppendLine("namespace Emulator.CGB.Tests.CPU;");
//    doc.AppendLine();
//    doc.AppendLine($"public class {name}");
//    doc.AppendLine("{");
//    foreach (var unitTest in unitTests)
//    {
//        var methodName = GetValidMethodName(unitTest.name);
//        doc.AppendLine("\t[Fact]");
//        doc.AppendLine($"\tpublic void Test{methodName}()");
//        doc.AppendLine("\t{");
//        doc.AppendLine($"\t\tvar test = LoadTest{methodName}();");
//        doc.AppendLine("\t\tvar memoryManager = new MemoryManager();");
//        doc.AppendLine("\t\tvar gbcpu = new CGBCPU(memoryManager);");
//        doc.AppendLine("\t\tCommon.InitializeCPU(gbcpu, test.initial);");
//        doc.AppendLine("\t\tgbcpu.ProcessOperation();");
//        doc.AppendLine("\t\tvar actual = Common.GetCPUAsFinal(gbcpu);");
//        doc.AppendLine("\t\tAssert.Equivalent(test.final, actual);");
//        doc.AppendLine("\t}");

//        doc.AppendLine($"\tpublic UnitTest LoadTest{methodName}()");
//        doc.AppendLine("\t{");
//        doc.AppendLine("\t\tUnitTest ut = new ();");
//        doc.AppendLine($"\t\tut.initial.pc =  {unitTest.initial.pc};");
//        doc.AppendLine($"\t\tut.initial.sp =  {unitTest.initial.sp};");
//        doc.AppendLine($"\t\tut.initial.a =   {unitTest.initial.a};");
//        doc.AppendLine($"\t\tut.initial.b =   {unitTest.initial.b};");
//        doc.AppendLine($"\t\tut.initial.c =   {unitTest.initial.c};");
//        doc.AppendLine($"\t\tut.initial.d =   {unitTest.initial.d};");
//        doc.AppendLine($"\t\tut.initial.e =   {unitTest.initial.e};");
//        doc.AppendLine($"\t\tut.initial.f =   {unitTest.initial.f};");
//        doc.AppendLine($"\t\tut.initial.h =   {unitTest.initial.h};");
//        doc.AppendLine($"\t\tut.initial.l =   {unitTest.initial.l};");
//        doc.AppendLine($"\t\tut.initial.ime = {unitTest.initial.ime};");
//        doc.AppendLine($"\t\tut.initial.ie =  {unitTest.initial.ie};");
//        doc.AppendLine($"\t\tut.initial.ram = {ConvertIntVectorToString(unitTest.initial.ram)};"); 
//        doc.AppendLine($"\t\tut.final.pc =  {unitTest.final.pc};");
//        doc.AppendLine($"\t\tut.final.sp =  {unitTest.final.sp};");
//        doc.AppendLine($"\t\tut.final.a =   {unitTest.final.a};");
//        doc.AppendLine($"\t\tut.final.b =   {unitTest.final.b};");
//        doc.AppendLine($"\t\tut.final.c =   {unitTest.final.c};");
//        doc.AppendLine($"\t\tut.final.d =   {unitTest.final.d};");
//        doc.AppendLine($"\t\tut.final.e =   {unitTest.final.e};");
//        doc.AppendLine($"\t\tut.final.f =   {unitTest.final.f};");
//        doc.AppendLine($"\t\tut.final.h =   {unitTest.final.h};");
//        doc.AppendLine($"\t\tut.final.l =   {unitTest.final.l};");
//        doc.AppendLine($"\t\tut.final.ime = {unitTest.final.ime};");
//        doc.AppendLine($"\t\tut.final.ram = {ConvertIntVectorToString(unitTest.final.ram)};");
//        //doc.AppendLine($"\t\t\t ut.cycles = {ConvertObjectVectorToString(unitTest.cycles)};");
//        doc.AppendLine("\t\treturn ut;");
//        doc.AppendLine("\t}");
//        doc.AppendLine("\t");
//    }
//    doc.AppendLine("}");
//    return doc.ToString();
//}
//static string MakeValidFileName(string name)
//{
//    string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
//    string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

//    return Regex.Replace(Regex.Replace(name, invalidRegStr, "_"), " ", "_");
//}
//static string GetValidMethodName(string input)
//{
//    // Remover caracteres no permitidos para nombres de método
//    string cleanedString = Regex.Replace(input, @"[^a-zA-Z0-9_]", "_");

//    // Verificar si el primer carácter es un número y agregar un subrayado si es necesario
//    if (char.IsDigit(cleanedString[0]))
//    {
//        cleanedString = "_" + cleanedString;
//    }

//    return Regex.Replace(cleanedString, @"[ ]", "_");
//}

//static string ConvertIntVectorToString(int[][] matriz)
//{
//    StringBuilder sb = new StringBuilder();

//    sb.Append("new int[][]{ ");

//    for (int i = 0; i < matriz.Length; i++)
//    {
//        sb.Append("new int[]{");

//        for (int j = 0; j < matriz[i].Length; j++)
//        {
//            sb.Append(matriz[i][j]);

//            if (j < matriz[i].Length - 1)
//                sb.Append(", ");
//        }

//        sb.Append("}");

//        if (i < matriz.Length - 1)
//            sb.Append(", ");
//    }

//    sb.Append("}");

//    return sb.ToString();
//}
//static string ConvertObjectVectorToString(object[][] matriz)
//{
//    StringBuilder sb = new StringBuilder();

//    sb.Append("{ ");

//    for (int i = 0; i < matriz.Length; i++)
//    {
//        sb.Append("new object [] {");

//        for (int j = 0; j < matriz[i].Length; j++)
//        {
//            sb.Append(matriz[i][j]);

//            if (j < matriz[i].Length - 1)
//                sb.Append(", ");
//        }

//        sb.Append("}");

//        if (i < matriz.Length - 1)
//            sb.Append(", ");
//    }

//    sb.Append("}");

//    return sb.ToString();
//}

//Run("Tests");