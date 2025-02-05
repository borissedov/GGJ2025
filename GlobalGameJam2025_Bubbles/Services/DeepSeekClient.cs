using System.Diagnostics;
using System.Text;
using System.Text.Json;
using GlobalGameJam2025_Bubbles.Services.GptOutput;
using GlobalGameJam2025_Bubbles.Services.GptOutput.DeepSeek;
using Microsoft.ApplicationInsights;

namespace GlobalGameJam2025_Bubbles.Services;

public class DeepSeekClient : ILlmClient
{
    private readonly TelemetryClient _telemetryClient;
    private readonly string _endpoint;
    private readonly string _model;
    private readonly string _apiKey;
    private readonly string _systemMessage;
    private readonly HttpClient _httpClient;

    public DeepSeekClient(IConfiguration configuration, TelemetryClient telemetryClient, HttpClient httpClient)
    {
        _telemetryClient = telemetryClient;
        _httpClient = httpClient;
        _endpoint = configuration.GetValue<string>("DeepSeekEndpoint")!;
        _model = configuration.GetValue<string>("DeepSeekModel")!;
        _apiKey = configuration.GetValue<string>("DeepSeekApiKey")!;
        _systemMessage = File.ReadAllText(Path.Combine("wwwroot", "Prompts", "full_system_prompt.txt"));
    }

    public TweetProcessingResponse? ProcessTweet(int day, string newsText, string tweetText)
    {
        // Build the user message combining the event and tweet text
        var userMessage = $@"
Event {day}: {newsText}
Tweet {day}: {tweetText}
";

        var requestId = Guid.NewGuid();
        var payload = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = _systemMessage },
                new { role = "user", content = userMessage }
            },
            stream = false,
            response_format = new { type = "json_object" }
        };

        // Serialize the payload
        string jsonPayload = JsonSerializer.Serialize(payload);

        // Create a POST request to the DeepSeek API endpoint (ensuring no duplicate '/')
        string url = _endpoint.TrimEnd('/') + "/chat/completions";
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("Authorization", $"Bearer {_apiKey}");
        request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        // Start timing the call
        var stopwatch = Stopwatch.StartNew();
        var response = _httpClient.SendAsync(request).GetAwaiter().GetResult();
        stopwatch.Stop();

        response.EnsureSuccessStatusCode();

        string responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        // Deserialize the DeepSeek response
        var deepSeekResponse = JsonSerializer.Deserialize<DeepSeekResponse>(responseContent);
        // Assuming the assistant response is in the first choice's message content.
        string responseText = deepSeekResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? "";

        // Log telemetry (using a similar pattern as in OpenAiClient)
        _telemetryClient.TrackEvent("DeepSeekCall",
            new Dictionary<string, string>
            {
                { "CallId", requestId.ToString() },
                { "Prompt", userMessage },
                { "Response", responseText }
            },
            new Dictionary<string, double>
            {
                { "DurationMs", stopwatch.ElapsedMilliseconds }
            }
        );

        // Deserialize the assistant’s output into your TweetProcessingResponse
        return JsonSerializer.Deserialize<TweetProcessingResponse>(responseText);
    }

    public string Debug(string systemPrompt, string userPrompt)
    {
        var payload = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            stream = false
        };

        string jsonPayload = JsonSerializer.Serialize(payload);

        string url = _endpoint.TrimEnd('/') + "/chat/completions";
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("Authorization", $"Bearer {_apiKey}");
        request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var response = _httpClient.SendAsync(request).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();

        string responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        var deepSeekResponse = JsonSerializer.Deserialize<DeepSeekResponse>(responseContent);
        string responseText = deepSeekResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? "";

        return responseText;
    }
}