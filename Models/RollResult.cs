using System.Text.Json.Serialization;

public class IndividualRollResult
{
    public int? Id {get; set;}
    
    [JsonPropertyName("result")]
    public float Result { get; set; }

    [JsonPropertyName("expression")]
    public string Expression { get; set; }
}

public class RollResult
{
    public int? Id {get; set;}

    public DateTime? Timestamp {get; set;}

    [JsonPropertyName("rolls")]
    public ICollection<IndividualRollResult> Rolls { get; set; }

    [JsonPropertyName("result")]
    public float Result { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    // When a lone custom die is rolled, the rolled face's overridden text/image (if any).
    // Not part of the roll-service payload — populated after the roll and persisted for display.
    [JsonIgnore]
    public string? FaceText { get; set; }

    [JsonIgnore]
    public string? FaceImage { get; set; }
}
