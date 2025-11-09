using System.Text.Json;

public class PlayerCharacter
{

    public int? Id { get; set; }
    public string? UserId { get; set; }
    public string? Name { get; set; }


    public string? RollServerId {get; set;}

    public IEnumerable<Item> Inventory {get; set; } = new List<Item>() {
        new Item() {
            Icon = "🍔",
            Name = "Cheeseburger"
        },
        new Item() {
            Icon = "⚔️",
            Name = "Sword",
            Rolls = new List<Roll>() {new Roll(){Name = "attack", RollString="1d12+1"} }
        }
        
    };


    public IEnumerable<Spell> Spells {get; set; } = new List<Spell>() {
        new Spell() {
            Icon = "🧊",
            Name = "Ice Bolt",
            Rolls = new List<Roll>() {new Roll(){Name = "attack", RollString="1d12+1"} }
            
        },
        new Spell() {
            Icon = "🔥",
            Name = "Fireball",
            Rolls = new List<Roll>() {new Roll(){Name = "attack", RollString="1d12+1"} }
        }
        
    };


    public int? Level => TryGetInt("level");

    public string? Race { get; set; }

    public List<Stat>? Stats
    {
        get
        {
            if (StatJson?.RootElement.TryGetProperty("stats", out var stats) == true && stats.ValueKind == JsonValueKind.Object)
            {
                var list = new List<Stat>();
                foreach (var prop in stats.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.Number && prop.Value.TryGetInt32(out var v))
                        if (v != 0)
                            list.Add(new Stat { Name = prop.Name, Value = v });
                }
                return list;
            }
            return null;
        }
    }

    
    public List<Stat>? SpecialStats
    {
        get
        {
            if (StatJson?.RootElement.TryGetProperty("special_stats", out var stats) == true && stats.ValueKind == JsonValueKind.Object)
            {
                var list = new List<Stat>();
                foreach (var prop in stats.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.Number && prop.Value.TryGetInt32(out var v))
                        if (v != 0)
                            list.Add(new Stat { Name = prop.Name, Value = v });
                }
                return list;
            }
            return null;
        }
    }

    public IEnumerable<Status>? Statuses { get; set; }

    public List<Gauge> Gauges
    {
        get
        {
            var gauges = new List<Gauge>();

            if (CurrentHealth.HasValue && MaxHealth.HasValue)
                gauges.Add(new Gauge {Icon = "", Name = "Health", Value = CurrentHealth.Value, Max = MaxHealth.Value });

            if (Mana.HasValue && EnergyPool.HasValue)
                gauges.Add(new Gauge { Name = "Energy", Value = Mana.Value, Max = EnergyPool.Value  });

            if (CurrentSoul.HasValue && MaxSoul.HasValue)
                gauges.Add(new Gauge { Name = "Soul", Value = CurrentSoul.Value, Max = MaxSoul.Value });

            if (CurrentArmour.HasValue && MaxArmour.HasValue)
                gauges.Add(new Gauge { Name = "Armour", Value = CurrentArmour.Value, Max = MaxArmour.Value });

            return gauges;
        }
    }

    public string? StatBlockHash { get; set; }
    public string? StatBlock { get; set; }
    public string? StatBlockMessageId { get; set; }
    public string? StatBlockChannelId { get; set; }
    public string? SpellBlockChannelId { get; set; }
    public string? SpellBlockMessageId { get; set; }
    public string? SpellBlock { get; set; }
    public string? SpellBlockHash { get; set; }
    public int? Mana { get; set; }
    public string? ManaReadoutChannelId { get; set; }
    public string? ManaReadoutMessageId { get; set; }




public Dictionary<string, string>? SavedRolls
{
    get
    {
        var saved_rolls = TryGetString("saved_rolls"); //this is kind of atrocious but I am limited by discord's UI (and my own laziness)

        return saved_rolls
            ?.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            ?.Select(line => line.Split(':', 2))
            ?.ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim());
    }
}

    JsonDocument? _statJson;

    private JsonDocument? StatJson
    {
        get
        {
            if (_statJson == null && !string.IsNullOrEmpty(StatBlock))
            {
                try { _statJson = JsonDocument.Parse(StatBlock); }
                catch { _statJson = null; }
            }
            return _statJson;
        }
    }

    int? TryGetInt(string prop)
    {
        if (StatJson?.RootElement.TryGetProperty(prop, out var p) == true && p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out var v))
            return v;
        return null;
    }

    string? TryGetString(string prop)
    {
        if (StatJson?.RootElement.TryGetProperty(prop, out var p) == true && p.ValueKind == JsonValueKind.String)
            return p.GetString();
        return null;
    }

    public int? Hunger => TryGetInt("hunger");
    public string? DefaultRoll => TryGetString("default_roll");
    public string? ModifierFormula => TryGetString("modifier_formula");
    public int? Actions => TryGetInt("actions");
    public int? Reactions => TryGetInt("reactions");
    public int? Speed => TryGetInt("speed");
    public int? MaxArmour => TryGetInt("armour");
    public int? CurrentArmour => TryGetInt("current_armour");
    public int? MaxSoul => TryGetInt("soul");
    public int? CurrentSoul => TryGetInt("current_soul");
    public int? MaxHealth => TryGetInt("hp");
    public int? CurrentHealth => TryGetInt("current_hp");
    public int? HealthRegen => TryGetInt("hpr");
    public int? EnergyPool => TryGetInt("energy_pool");


    
}
