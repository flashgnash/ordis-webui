using System.ComponentModel.DataAnnotations;

public class CharacterBuff
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TemplateId { get; set; }
    public BuffTemplate Template { get; set; } = null!;

    public int PlayerCharacterId { get; set; }
    public PlayerCharacter PlayerCharacter { get; set; } = null!;

    // Runtime state: current stack count for cumulative effects
    public int Stacks { get; set; } = 0;
}
