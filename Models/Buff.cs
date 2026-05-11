using System.Text.Json;
using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BuffType
{
    Equipment,
    Status,
    Skill
}

public class Effect
{
    public string Formula { get; set; } = "";
    public bool IsCumulative { get; set; }
    public int Stacks { get; set; } = 1;
}

[JsonConverter(typeof(BuffConverter))]
public class Buff
{
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "shield";
    public List<Effect> Effects { get; set; } = new();
    public BuffType Type { get; set; } = BuffType.Status;

    [JsonIgnore]
    public bool HasCumulativeEffects => Effects.Any(e => e.IsCumulative);
}

/// Handles deserializing both old format (Formulae as string[], Type "Cumulative", Stacks)
/// and new format (Effects as Effect[]).
public class BuffConverter : JsonConverter<Buff>
{
    public override Buff Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var buff = new Buff();

        if (root.TryGetProperty("Name", out var name))
            buff.Name = name.GetString() ?? "";

        if (root.TryGetProperty("Icon", out var icon))
            buff.Icon = icon.GetString() ?? "shield";

        // Parse type, mapping old "Cumulative" to Status
        if (root.TryGetProperty("Type", out var typeProp))
        {
            var typeStr = typeProp.GetString() ?? "Status";
            if (typeStr.Equals("Cumulative", StringComparison.OrdinalIgnoreCase))
                buff.Type = BuffType.Status;
            else if (Enum.TryParse<BuffType>(typeStr, true, out var parsed))
                buff.Type = parsed;
        }

        // New format: Effects array
        if (root.TryGetProperty("Effects", out var effects))
        {
            buff.Effects = JsonSerializer.Deserialize<List<Effect>>(effects.GetRawText(), options) ?? new();
        }
        // Old format: Formulae string array + optional Stacks
        else if (root.TryGetProperty("Formulae", out var formulae))
        {
            var wasCumulative = root.TryGetProperty("Type", out var tp)
                && (tp.GetString() ?? "").Equals("Cumulative", StringComparison.OrdinalIgnoreCase);
            var stacks = root.TryGetProperty("Stacks", out var sp) ? sp.GetInt32() : 1;

            foreach (var f in formulae.EnumerateArray())
            {
                buff.Effects.Add(new Effect
                {
                    Formula = f.GetString() ?? "",
                    IsCumulative = wasCumulative,
                    Stacks = wasCumulative ? stacks : 1
                });
            }
        }

        return buff;
    }

    public override void Write(Utf8JsonWriter writer, Buff value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("Name", value.Name);
        writer.WriteString("Icon", value.Icon);
        writer.WriteString("Type", value.Type.ToString());
        writer.WritePropertyName("Effects");
        JsonSerializer.Serialize(writer, value.Effects, options);
        writer.WriteEndObject();
    }
}
