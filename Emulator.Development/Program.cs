// See https://aka.ms/new-console-template for more information
using Emulator.Domain;
using Emulator.GBC;

Console.WriteLine("Hello, World!");
IMachine Machine = new GBCMachine();

try
{

    await Machine.LoadGame("B:\\Dev\\Emulators\\ROMs\\Tennis.gb");

    Machine.ExecuteGame();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

