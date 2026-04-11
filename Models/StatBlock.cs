using System.Text.Json.Serialization;
using System.Text.Json;

public class StatListConverter : JsonConverter<List<Stat>>
{
    public override List<Stat> Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o)
    {
        var dict = JsonSerializer.Deserialize<Dictionary<string,int?>>(ref r, o);
        return dict?
            .Select(k => new Stat { Name = k.Key, Value = k.Value ?? 0 })
            .ToList() ?? new();
    }

    public override void Write(Utf8JsonWriter w, List<Stat> v, JsonSerializerOptions o)
    {
        var dict = v.ToDictionary(s => s.Name, s => s.Value);
        JsonSerializer.Serialize(w, dict, o);
    }
}


public class StatBlock
{
    [JsonPropertyName("level")]
    public int? Level { get; set; }

    [JsonPropertyName("hunger")]
    public int? Hunger { get; set; }

    [JsonPropertyName("default_roll")]
    public string? DefaultRoll { get; set; }

    [JsonPropertyName("modifier_formula")]
    public string? ModifierFormula { get; set; }

    [JsonPropertyName("actions")]
    public int? Actions { get; set; }

    [JsonPropertyName("reactions")]
    public int? Reactions { get; set; }

    [JsonPropertyName("speed")]
    public int? Speed { get; set; }

    [JsonPropertyName("armour")]
    public int? MaxArmour { get; set; }

    [JsonPropertyName("current_armour")]
    public int? CurrentArmour { get; set; }

    [JsonPropertyName("soul")]
    public int? MaxSoul { get; set; }

    [JsonPropertyName("current_soul")]
    public int? CurrentSoul { get; set; }

    [JsonPropertyName("hp")]
    public int? MaxHealth { get; set; }

    [JsonPropertyName("current_hp")]
    public int? CurrentHealth { get; set; }

    [JsonPropertyName("hpr")]
    public int? HealthRegen { get; set; }

    [JsonPropertyName("energy_pool")]
    public int? EnergyPool { get; set; }

    [JsonPropertyName("stats")]
    [JsonConverter(typeof(StatListConverter))]
    public List<Stat>? Stats { get; set; }

    [JsonConverter(typeof(StatListConverter))]
    [JsonPropertyName("special_stats")]
    public List<Stat>? SpecialStats { get; set; }
}
