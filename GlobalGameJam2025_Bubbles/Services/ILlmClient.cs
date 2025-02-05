using GlobalGameJam2025_Bubbles.Services.GptOutput;

namespace GlobalGameJam2025_Bubbles.Services;

public interface ILlmClient
{
    TweetProcessingResponse? ProcessTweet(int day, string newsText, string tweetText);
    string Debug(string systemPrompt, string userPrompt);
}