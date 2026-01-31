public class Spell : IDescriptable, IRollable
{
    public Guid Id {get; set;}

    public string? Icon { get; set; }

    public string Name { get; set; }

    public string? Description {get; set;}

    public IEnumerable<Roll>? Rolls { get; set; }
}
