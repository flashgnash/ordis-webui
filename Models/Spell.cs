public struct Spell: IDescriptable, IRollable {

	public string? Icon {get; set;}

	public string Name {get; set;}

	public IEnumerable<Roll>? Rolls {get; set;}	

}

