using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

public class BuffTemplate
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = "";
    public string Icon { get; set; } = "shield";
    public BuffType Type { get; set; } = BuffType.Status;

    [Column("effects")]
    public string EffectsJson { get; set; } = "[]";

    [NotMapped]
    public List<Effect> Effects
    {
        get => JsonSerializer.Deserialize<List<Effect>>(EffectsJson ?? "[]") ?? new();
        set => EffectsJson = JsonSerializer.Serialize(value);
    }

    // Player-level library: set OwnerId, CampaignId = null
    // Campaign-level library: set CampaignId, OwnerId = null
    public string? OwnerId { get; set; }
    public User? Owner { get; set; }

    public int? CampaignId { get; set; }
    public Campaign? Campaign { get; set; }

    // If this player-level template was imported from a campaign template, track the source
    public Guid? SourceTemplateId { get; set; }
    public BuffTemplate? SourceTemplate { get; set; }

    public bool IsOutOfSync(BuffTemplate? source) =>
        source != null && (Name != source.Name || Icon != source.Icon || Type != source.Type || EffectsJson != source.EffectsJson);
}
