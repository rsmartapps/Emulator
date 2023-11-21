using Emulator.Domain;
using Emulator.GBC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Emulator.CGB.ConsoleTests
{
    internal class UITestsRunner
    {
        public static void Run(string path)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var totalOPCodesFailed = 0;
            foreach (var file in Directory.EnumerateFiles(path))
            {
                var sanitizedName = Path.GetFileNameWithoutExtension(file);
                Console.WriteLine("Tests for {0}", sanitizedName);
                CGBMachine machine = new CGBMachine();
                machine.InsertCartRidge(file);

                try
                {
                    while (true)
                    {
                        machine.RunGame();
                    }
                }catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            sw.Stop();
            Console.WriteLine($"Time {sw.Elapsed}");
            Console.WriteLine($"Total OPCodes Failed {totalOPCodesFailed}");
        }
    }
}
