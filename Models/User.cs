using System;
using System.Collections.Generic;

public partial class User
{
    public string Id { get; set; } = null!;

    public string? Username { get; set; }

    public int? Count { get; set; }

    public string? SelectedCharacterId { get; set; }

    public int? SelectedCharacter { get; set; }
}
