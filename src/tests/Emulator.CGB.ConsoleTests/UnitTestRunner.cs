using Emulator.CGB.CPU;
using System.Diagnostics;
using System.Text.Json;
using Xunit;

namespace Emulator.CGB.ConsoleTests;

internal static class UnitTestRunner
{
    public static void Run(string path)
    {
        Stopwatch sw = Stopwatch.StartNew();
        var totalOPCodesFailed = 0;
        foreach (var file in Directory.EnumerateFiles(path))
        {
            var sanitizedName = Path.GetFileNameWithoutExtension(file);
            var jsonTest = File.ReadAllText(file);
            var ok = 0;
            var nok = 0;
            foreach (var item in JsonSerializer.Deserialize<IEnumerable<UnitTest>>(jsonTest))
            {
                try
                {
                    var expectedCycles = ((JsonElement)item.cycles[0][0]).Deserialize<int>();
                    var opCycles = RunTest(item);
                    ok++;
                }
                catch (Exception ex)
                {
                    nok++;
                    Console.WriteLine($"Test {item.name} error: {ex.Message}");
                    //break;
                }
            }
            if (nok > 0) totalOPCodesFailed++;
            Console.WriteLine($"Tests for {sanitizedName} Completed:{ok} Failed:{nok} total:{ok + nok}");
        }
        sw.Stop();
        Console.WriteLine($"Time {sw.Elapsed}");
        Console.WriteLine($"Total OPCodes Failed {totalOPCodesFailed}");
    }
    static int RunTest(UnitTest test)
    {
        var ram = new MBCMock();
        var gbcpu = new CGBCPU(ram);
        Common.InitializeCPU(gbcpu, test.initial);
        var cycles =gbcpu.ProcessOperation();
        var actual = Common.GetCPUAsFinal(gbcpu);
        
        Assert.Equivalent(test.final, actual);
        return cycles;
    }
}
