using System.ComponentModel.DataAnnotations;

public class Item : IDescriptable, IRollable
{
    public Guid Id {get; set;}

    public string? Icon { get; set; }

    [Required]
    public required string Name { get; set; }
    
    public string? Description {get; set;}

    public IEnumerable<Roll>? Rolls { get; set; }
}
