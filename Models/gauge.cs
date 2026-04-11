using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

public enum GaugeType
{
    Orb,
    IconBar,
}

public class Gauge
{
    [NotMapped]
    public static readonly Dictionary<string, GaugeType> GaugeTypeLookup = new()
    {
        { "health", GaugeType.IconBar },
        { "mana", GaugeType.Orb },
        { "hunger", GaugeType.IconBar },
    };

    // public string Id => $"{Owner}_{Name}";

    [Key]
    public Guid Id { get; set; }

    public GaugeType GaugeType { get; set; }

    public int PlayerCharacterId { get; set; }

    private Dictionary<string, string> _colourLookup = new Dictionary<string, string>
    {
        { "hp", "red" },
        { "health", "red" },
        { "energy", "blue" },
        { "armour", "yellow" },
        { "armor", "yellow" },
        { "soul", "purple" },
    };

    public string? Icon { get; set; }

    [Required]
    public string Name { get; set; }
    public int Value { get; set; }
    [Required]
    public int Max { get; set; }

    
    
    public string? Colour {get; set;}
}
