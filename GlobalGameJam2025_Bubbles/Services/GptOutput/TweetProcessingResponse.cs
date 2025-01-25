using System.Text.Json.Serialization;

namespace GlobalGameJam2025_Bubbles.Services.GptOutput;

public class TweetProcessingResponse
{
    [JsonPropertyName("step1")]
    public Step1 Step1 { get; set; }

    [JsonPropertyName("step2")]
    public Step2 Step2 { get; set; }
}

public class Step1
{
    [JsonPropertyName("outcome")]
    public string Outcome { get; set; }

    [JsonPropertyName("rationale")]
    public string Rationale { get; set; }
}

public class Step2
{
    [JsonPropertyName("elves")]
    public RaceAnalysis Elves { get; set; }

    [JsonPropertyName("gnomes")]
    public RaceAnalysis Gnomes { get; set; }

    [JsonPropertyName("humans")]
    public RaceAnalysis Humans { get; set; }

    [JsonPropertyName("orcs")]
    public RaceAnalysis Orcs { get; set; }
}

public class RaceAnalysis
{
    [JsonPropertyName("initial_reaction")]
    public int InitialReaction { get; set; }
    
    [JsonPropertyName("adjusted_reaction")]
    public int AdjustedReaction { get; set; }

    [JsonPropertyName("changes_made")]
    public string ChangesMade { get; set; }

    [JsonPropertyName("final_value")]
    public int FinalValue { get; set; }

    [JsonPropertyName("summary")]
    public string Summary { get; set; }
    
    [JsonPropertyName("comments")]
    public string[] Comments { get; set; }
}