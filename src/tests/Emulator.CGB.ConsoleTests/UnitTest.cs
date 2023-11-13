public class UnitTest
{
    public string name { get; set; }
    public Initial initial { get; set; } = new();
    public Final final { get; set; } = new();
    public object[][] cycles { get; set; }
}

public class Initial
{
    public ushort pc { get; set; }
    public ushort sp { get; set; }
    public byte a { get; set; }
    public byte b { get; set; }
    public byte c { get; set; }
    public byte d { get; set; }
    public byte e { get; set; }
    public byte f { get; set; }
    public byte h { get; set; }
    public byte l { get; set; }
    public int ime { get; set; }
    public int ie { get; set; }
    public int[][] ram { get; set; }
}

public class Final
{
    public byte a { get; set; }
    public byte b { get; set; }
    public byte c { get; set; }
    public byte d { get; set; }
    public byte e { get; set; }
    public byte f { get; set; }
    public byte h { get; set; }
    public byte l { get; set; }
    public ushort pc { get; set; }
    public ushort sp { get; set; }
    public int ime { get; set; }
    public int[][] ram { get; set; }
}



