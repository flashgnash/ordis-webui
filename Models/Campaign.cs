public class Campaign
{
    public int Id { get; set; }
    public string Name { get; set; }

    public User? DungeonMaster { get; set; }
    public string? DungeonMasterId {get; set;}

    public ICollection<PlayerCharacter>? Players { get; set; }

    public ICollection<CharacterPreset>? Presets { get; set; }

    public ICollection<CustomDie>? CustomDice { get; set; }

    public string? DefaultRollDie { get; set; }
    public string? StatModifierFormula { get; set; }
}
