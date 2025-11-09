using System.Text.Json.Serialization;

public struct IndividualRollResult {
    [JsonPropertyName("result")]
    public float Result { get; set; }

    [JsonPropertyName("expression")]
    public string Expression { get; set; }
}

public struct RollResult {
    [JsonPropertyName("rolls")]
    public IEnumerable<IndividualRollResult> Rolls { get; set; }

    [JsonPropertyName("result")]
    public float Result { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }
}
