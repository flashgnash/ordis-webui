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
}
