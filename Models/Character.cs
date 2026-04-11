using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

public class PlayerCharacter : IValidatableObject
{


public IEnumerable<ValidationResult> Validate(ValidationContext ctx)
{
    foreach (var g in Gauges)
    {
        if (string.IsNullOrWhiteSpace(g.Name))
        {
            yield return new ValidationResult(
                "Gauge name missing",
                new[] { nameof(Gauges) }
            );
        }
    }
    if(StatBlock?.Stats != null) {
        foreach (var s in StatBlock.Stats)
        {
            if (string.IsNullOrWhiteSpace(s.Name))
            {
                yield return new ValidationResult(
                    "Stat name missing",
                    new[] { nameof(StatBlock.Stats) }
                );
            }
        }
        
    }
}

    [Key]
    public int Id { get; set; }
    public string? UserId { get; set; }

    public string? Name { get; set; }

    public Campaign? Campaign { get; set; }

    public string? RollServerId { get; set; }

    public ICollection<RollResult>? Rolls {get; set;}


    public IEnumerable<Item> Inventory { get; set; }

    public IEnumerable<Spell> Spells { get; set; }

    [NotMapped]
    public string? Race { get; set; }

    [NotMapped]
    public IEnumerable<Status>? Statuses { get; set; }

    public ICollection<Gauge>? Gauges { get; set; }

    // public string? StatBlockHash { get; set; }
    [Column("stat_block")]
    public string? StatBlockJson { get; set; }

    [NotMapped]
    private StatBlock? _statBlock;

    [NotMapped]
    public StatBlock? StatBlock
    {
        get
        {
            if (_statBlock == null && !string.IsNullOrEmpty(StatBlockJson))
                _statBlock = JsonSerializer.Deserialize<StatBlock>(StatBlockJson);

            return _statBlock;
        }
        set
        {
            _statBlock = value;
            StatBlockJson = value == null ? null : JsonSerializer.Serialize(value);
        }
    }   

    public string? StatBlockHash { get; set; }
    public string? StatBlockMessageId { get; set; }
    public string? StatBlockChannelId { get; set; }
    public string? StatBlockServerId { get; set; }

    public string? SpellBlockChannelId { get; set; }
    public string? SpellBlockMessageId { get; set; }
    public string? SpellBlock { get; set; }
    public string? SpellBlockHash { get; set; }
    public int? Mana { get; set; }
    public string? ManaReadoutChannelId { get; set; }
    public string? ManaReadoutMessageId { get; set; }

    public string? SavedRolls { get; set; }

    [NotMapped]
    public Dictionary<string, string>? SavedRollsDict
    {
        get
        {
            var saved_rolls = SavedRolls; //this is kind of atrocious but I am limited by discord's UI (and my own laziness)

            return saved_rolls
                ?.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                ?.Select(line => line.Split(':', 2))
                ?.ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim());
        }
    }

    // [Column("stats")]
    // JsonDocument? _statJson;

    // private JsonDocument? StatJson
    // {
    //     get
    //     {
    //         if (_statJson == null && !string.IsNullOrEmpty(StatBlock))
    //         {
    //             try { _statJson = JsonDocument.Parse(StatBlock); }
    //             catch { _statJson = null; }
    //         }
    //         return _statJson;
    //     }
    // }



    // public string? StatBlockServerId { get; set; }
}
