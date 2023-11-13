namespace Emulator.CGB.Memory.MBC;

internal class MBC0 : IMBC
{
    protected const ushort ROM_HIGH = 0x3FFF;
    protected const ushort ROM_BANK_HIGH = 0x7FFF;
    protected const ushort VRAM_HIGH = 0x9FFF;
    protected const ushort CRAM_HIGH = 0xBFFF;
    protected const ushort WRAM_HIGH = 0xDFFF;
    protected const ushort WRAME_HIGH = 0xFDFF;
    protected const ushort OAM_HIGH = 0xFE9F;
    protected const ushort UM_HIGH = 0xFEFF;
    protected const ushort IO_HIGH = 0xFF7F;
    protected const ushort ZPRAM_HIGH = 0xFFFE;
    protected byte[] ROM;

    public IDictionary<ushort, byte> RAM { get; } = new Dictionary<ushort, byte>();

    public MBC0(byte[] rom)
    {
        this.ROM = rom;
    }

    public virtual byte ReadERAM(ushort addr)
    {
        return 0xFF; //MBC0 dosn't have ERAM
    }

    public virtual byte ReadLoROM(ushort addr)
    {
        return RAM[addr];
    }

    public virtual byte ReadHighROM(ushort addr)
    {
        return RAM[addr];
    }

    public void WriteERAM(ushort addr, byte value)
    {
        //MBC0 should ignore writes
    }

    public void WriteROM(ushort addr, byte value)
    {
        //MBC0 should ignore writes
        RAM[addr] = value;
    }

    public bool IsAccessible(ushort address)
    {
        return address <= 0x7FFF;
    }

    public virtual byte Read(ushort address)
    {
        byte value = 0;
        try
        {
            switch (address)
            {
                case < ROM_HIGH:
                    return Read(address);
                case < ROM_BANK_HIGH:
                    return Read(address);
                case < VRAM_HIGH:
                    return Read(address);
                case < CRAM_HIGH:
                    return RAM[address];
                case < WRAM_HIGH:
                    return RAM[address];
                case < WRAME_HIGH:
                    return RAM[address];
                case < OAM_HIGH:
                    return RAM[address];
                case < UM_HIGH:
                    return 0xFF;
                case < IO_HIGH:
                    return RAM[address];
                case < ZPRAM_HIGH:
                    return RAM[address];
                default:
                    Console.WriteLine($"Out of index {address.ToString("X")}");
                    return 0;
            }
        }
        catch (Exception ex)
        {
            //Console.WriteLine($"Error reading from {address.ToString("X")} with error {ex.Message}");
        }
        return value;
    }

    public virtual void Write(ushort address, byte value)
    {
        try
        {
            switch (address)
            {
                case < ROM_HIGH: break;
                case < ROM_BANK_HIGH: break;
                case < VRAM_HIGH:
                    Write(address, value); break;
                case < CRAM_HIGH:
                    RAM[address] = value; break;
                case < WRAM_HIGH:
                    RAM[address] = value; break;
                case < OAM_HIGH:
                    RAM[address] = value; break;
                case < IO_HIGH:
                    RAM[address] = value; break;
                case < ZPRAM_HIGH:
                    RAM[address] = value; break;
                default:
                    Console.WriteLine($"Can't write heare {address.ToString("X")}");
                    break;
            }
        }
        catch (Exception ex)
        {
            //Console.WriteLine($"Error reading from {address.ToString("X")}");
        }
    }
}
