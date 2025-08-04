
public class RpgConfig
{
    public Dictionary<string, string> StatusStats { get; set; } = new();
    public Dictionary<string, BarStat> BarStats { get; set; } = new();
}

public class BarStat
{
    public string Colour { get; set; } = "";
}
