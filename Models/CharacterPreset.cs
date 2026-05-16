using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

public class PresetGauge
{
    public string? Name { get; set; }
    public int Max { get; set; }
    public string? Colour { get; set; }
    public GaugeType GaugeType { get; set; }
}

public class CharacterPreset
{
    [Key]
    public int Id { get; set; }

    public string? Name { get; set; }

    public int CampaignId { get; set; }
    public Campaign? Campaign { get; set; }

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

    public string? GaugesJson { get; set; }

    [NotMapped]
    private List<PresetGauge>? _gauges;

    [NotMapped]
    public List<PresetGauge> Gauges
    {
        get
        {
            if (_gauges == null)
                _gauges = GaugesJson != null
                    ? JsonSerializer.Deserialize<List<PresetGauge>>(GaugesJson) ?? new()
                    : new();
            return _gauges;
        }
        set
        {
            _gauges = value;
            GaugesJson = value == null ? null : JsonSerializer.Serialize(value);
        }
    }
}
