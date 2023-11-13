// See https://aka.ms/new-console-template for more information
using Emulator.Domain;


Console.WriteLine("Hello, World!");
async Task Tests()
{
    try
    {
        foreach (var file in Directory.EnumerateFiles("B:\\Dev\\Emulators\\ROMs\\tests\\CPU\\individual"))
        {
            Console.WriteLine($"Executing {file}");
            try
            {

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

    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}
await RunGame();

