using System.ComponentModel.DataAnnotations;

public struct Item : IDescriptable, IRollable {


	public string? Icon {get; set;}
	
	[Required]
	public required string Name {get; set;}

	public IEnumerable<Roll>? Rolls {get; set;}	

	
}
