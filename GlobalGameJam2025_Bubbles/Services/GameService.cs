using GlobalGameJam2025_Bubbles.Models;

namespace GlobalGameJam2025_Bubbles.Services;

public class GameService
{
    private readonly int _maxGameDays;
    private readonly NewsService _newsService;

    public GameService(IConfiguration configuration, NewsService newsService)
    {
        _newsService = newsService;
        _maxGameDays = configuration.GetValue<int>("MaxGameDays");
    }

    public GameViewModel NewGame(string username)
    {
        // Define some default bubble properties with seeds
        var bubbleTitles = new[] { "Humans", "Orks", "Elves", "Dwarfs" };
        var bubbleColors = new[] { "#FF5733", "#33FF57", "#3357FF", "#F3FF33" }; // Diverse colors
        var bubbles = new List<AudienceBubble>();

        for (int i = 0; i < 4; i++)
        {
            // Generate an avatar URL using a seed (title here)
            var avatarUrl = GenerateAvatarUrl(bubbleTitles[i]);

            bubbles.Add(new AudienceBubble
            {
                Title = bubbleTitles[i],
                AvatarUrl = avatarUrl,
                BodyText =
                    $"{bubbleTitles[i]} are a vital part of the empire. They contribute to its strength and stability.",
                ColorCode = bubbleColors[i],
                Loyalty = 100 - (i * 10), // Decreasing loyalty for variety
                Radius = 100 + (i * 10) // Increasing radius for variety
            });
        }

        // Create the GameViewModel with dynamic data
        return new GameViewModel()
        {
            Username = string.IsNullOrWhiteSpace(username) ? "Guest" : username,
            Comments = new List<Comment>(),
            DaysCounter = 1,
            AudienceBubbles = bubbles,
            PeopleLoyalty = 50,
            EmperorHapinessText = "Content",
            EmperorPhotoUrl = GenerateAvatarUrl("Emperor"),
            PalantirText = "The Emperor oversees the empire.",
            PalantirActive = true,
            PalantirPhotoUrl = GenerateAvatarUrl("Palantir")
        };
    }

    // Helper method to generate avatar URLs based on seed
    private string GenerateAvatarUrl(string seed)
    {
        // Example using DiceBear Avatars API with the "adventurer" style
        return $"https://avatars.dicebear.com/api/adventurer/{Uri.EscapeDataString(seed)}.svg";
    }

    public async Task NextStep(GameViewModel game)
    {
        if (!ValidateGameState(game))
        {
            return;
        }

        var firstDay = game.GameState == GameState.Greetings || game.GameState == GameState.EmperorSelector;
        game.GameState = await ResolveNextStep(game);
        
      
        if (game.GameState == GameState.TweetEntry)
        {
            if (!firstDay)
            {
                game.DaysCounter++;

            }
            game.NewsRecord = _newsService.GetNewsRecordForDay(game.DaysCounter);
        }

        // if (game.GameState == GameState.Waiting)
        // {
        //     StartProcessingTweet(game);
        // }

        if (game.GameState == GameState.CommentsShow)
        {
            var aiResponse = ProcessTweet(game.NewsRecord!, game.Tweet);
            game.Comments = aiResponse.Comments;
            foreach (var bubble in game.AudienceBubbles)
            {
                // bubble.LoyaltyDelta = aiResponse.LoyaltyDeltas[bubble.Title];
            }
        }
        
        await SetUiElementsState(game);
    }
    
    private bool ValidateGameState(GameViewModel game)
    {
        return true;
    }

    private async Task<GameState> ResolveNextStep(GameViewModel game)
    {
        switch (game.GameState)
        {
            case GameState.Greetings:
                //return GameState.EmperorSelector;
                return GameState.TweetEntry;
            case GameState.EmperorSelector:
                return GameState.TweetEntry;
            case GameState.TweetEntry:
                // return GameState.Waiting;
                return GameState.CommentsShow;
            // case GameState.Waiting:
            //     return GameState.CommentsShow;
            case GameState.CommentsShow:
            {
                if (game.DaysCounter == _maxGameDays)
                {
                    return GameState.GameOver;
                }

                return GameState.TweetEntry;
            }
            case GameState.GameOver:
                return GameState.GameOver;
            default:
                return GameState.Greetings;
        }
    }

    private async Task SetUiElementsState(GameViewModel game)
    {
        switch (game.GameState)
        {
            case GameState.Greetings:
                game.TweetInputBlocked = true;
                game.CommentsBlockShown = false;
                game.PalantirActive = true;
                
                game.PalantirText = $"Greetings {game.Username}";
                game.ActionButtonText = "Start the game!";
                break;
            // case GameState.EmperorSelector:
            //     game.TweetInputBlocked = true;
            //     game.ActionButtonText = "Select your Emperor!";
            //     break;
            case GameState.TweetEntry:
                game.TweetInputBlocked = false;
                game.CommentsBlockShown = false;
                game.PalantirActive = true;
                
                game.PalantirText = game.NewsRecord.BodyText;
                    
                game.ActionButtonText = "Post";
                break;
            // case GameState.Waiting:
            //     game.TweetInputBlocked = true;
            //     game.CommentsBlockShown = false;
            //     game.PalantirActive = false;
            //     game.ActionButtonText = "Wait for the comments";
            //     break;
            case GameState.CommentsShow:
                game.TweetInputBlocked = true;
                game.CommentsBlockShown = true;
                game.PalantirActive = false;
                game.ActionButtonText = "Next day";
                break;
            case GameState.GameOver:
                game.TweetInputBlocked = true;
                game.CommentsBlockShown = false;
                game.PalantirActive = true;
                game.ActionButtonText = "Start Over";
                break;
            default:
                throw new Exception("Unknown game state");
        }
    }
    
    private AiResponse ProcessTweet(NewsRecord gameNewsRecord, string gameTweet)
    {
        return new AiResponse()
        {
            Comments = new List<Comment>()
            {
                new ()
                {
                    Username = "AI",
                    AvatarUrl = GenerateAvatarUrl("AI"),
                    BodyText = "This is a comment from the AI."
                }
            }
        };
    }

}

internal record AiResponse
{
    public IList<Comment> Comments { get; set; }
}