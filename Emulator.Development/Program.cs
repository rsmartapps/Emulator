// See https://aka.ms/new-console-template for more information
using Emulator.Domain;
using Emulator.GBC;


Console.WriteLine("Hello, World!");
async Task Tests()
{
    try
    {
        IMachine Machine = new GBCMachine();
        foreach (var file in Directory.EnumerateFiles("B:\\Dev\\Emulators\\ROMs\\tests\\CPU\\individual"))
        {
            Console.WriteLine($"Executing {file}");
            try
            {

                await Machine.LoadGame(file);

                Machine.ExecuteGame();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}

async Task RunGame()
{
    try
    {

        IMachine Machine = new GBCMachine();
        await Machine.LoadGame("B:\\Dev\\Emulators\\ROMs\\Tennis.gb");

        Machine.ExecuteGame();
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}
await RunGame();

