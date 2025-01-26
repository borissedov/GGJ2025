using System.ClientModel;
using System.Text.Json;
using Azure.AI.OpenAI;
using GlobalGameJam2025_Bubbles.Services.GptOutput;
using OpenAI.Chat;

namespace GlobalGameJam2025_Bubbles.Services
{
    public class OpenAiClient
    {
        private readonly string _endpoint;
        private readonly string _deploymentName;
        private readonly string _apiKey;
        private readonly string _systemMessage;

        public OpenAiClient(IConfiguration configuration)
        {
            _endpoint = configuration.GetValue<string>("OpenAIEndpoint")!;
            _deploymentName = configuration.GetValue<string>("OpenAIDeploymentName")!;
            _apiKey = Environment.GetEnvironmentVariable("OpenAIApiKey")!;
            _systemMessage = $@"
{File.ReadAllText(Path.Combine("wwwroot", "Prompts", "system_prompt.txt"))}

Content of game_description.txt: 
{File.ReadAllText(Path.Combine("wwwroot", "Prompts", "game_description.txt"))}

Content of race_description_full.txt: 
{File.ReadAllText(Path.Combine("wwwroot", "Prompts", "race_description_full.txt"))}

Content of race_description_ingame.txt: 
{File.ReadAllText(Path.Combine("wwwroot", "Prompts", "race_description_ingame.txt"))}
";
        }

        public TweetProcessingResponse? ProcessTweet(int day, string newsText, string tweetText)
        {
            AzureOpenAIClient azureClient = new(
                new Uri(_endpoint),
                new ApiKeyCredential(_apiKey)
            );
            ChatClient chatClient = azureClient.GetChatClient(_deploymentName);

            var userMessage = $@"
Event {day}: {newsText}
Tweet {day}: {tweetText}
";
            Console.Write("Prompt:");
            Console.WriteLine(userMessage);
            
            ChatCompletion completion = chatClient.CompleteChat(
            [
                new SystemChatMessage(_systemMessage),
                new UserChatMessage(userMessage)
            ]);

            string responseText = completion.Content[0].Text;

            Console.WriteLine("OpenAI Response:");
            Console.WriteLine(responseText);
            
            return JsonSerializer.Deserialize<TweetProcessingResponse>(responseText);
        }

        public string Debug(string systemPrompt, string userPrompt)
        {
            AzureOpenAIClient azureClient = new(
                new Uri(_endpoint),
                new ApiKeyCredential(_apiKey)
            );
            ChatClient chatClient = azureClient.GetChatClient(_deploymentName);

            ChatCompletion completion = chatClient.CompleteChat(
            [
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userPrompt)
            ]);

            return completion.Content[0].Text;

        }
    }
}