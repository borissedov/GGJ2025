using GlobalGameJam2025_Bubbles.Models;

namespace GlobalGameJam2025_Bubbles.Services;

public class GameService
{
    private readonly int _maxGameDays;
    private readonly NewsService _newsService;
    private readonly OpenAiClient _openAiClient;

    public GameService(IConfiguration configuration, NewsService newsService, OpenAiClient openAiClient)
    {
        _newsService = newsService;
        _openAiClient = openAiClient;
        _maxGameDays = configuration.GetValue<int>("MaxGameDays");
    }

    public async Task<GameViewModel> NewGame(string username)
    {
        // Define some default bubble properties with seeds
        var bubbleTitles = new[] { "Humans", "Orcs", "Elves", "Dwarves" };
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
                Loyalty = 5, // Decreasing loyalty for variety
                Radius = 100 + (i * 10) // Increasing radius for variety
            });
        }

        // Create the GameViewModel with dynamic data
        var game = new GameViewModel()
        {
            Username = string.IsNullOrWhiteSpace(username) ? "Guest" : username,
            Comments = new List<Comment>(),
            DaysCounter = 1,
            AudienceBubbles = bubbles,
            PeopleLoyalty = 50,
            EmperorHapinessText = "Content",
            EmperorPhotoUrl = GenerateAvatarUrl("Emperor"),
        };

        await SetUiElementsState(game);

        return game;
    }

    // Helper method to generate avatar URLs based on seed
    private string GenerateAvatarUrl(string seed)
    {
        // Example using DiceBear Avatars API with the "adventurer" style
        return $"img/circle-{seed.ToLower()}.png";
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
            var aiResponse = _openAiClient.ProcessTweet(game.DaysCounter, game.NewsRecord.BodyText, game.Tweet);
            foreach (var elvesComment in aiResponse.Step2.Elves.Comments)
            {
                game.Comments.Add(new Comment()
                {
                    Username = "Elves",
                    BodyText = elvesComment
                });
            }

            foreach (var orcsComment in aiResponse.Step2.Orcs.Comments)
            {
                game.Comments.Add(new Comment()
                {
                    Username = "Orсs",
                    BodyText = orcsComment
                });
            }

            foreach (var dwarvesComment in aiResponse.Step2.Dwarves.Comments)
            {
                game.Comments.Add(new Comment()
                {
                    Username = "Dwarves",
                    BodyText = dwarvesComment
                });
            }

            foreach (var humansComment in aiResponse.Step2.Humans.Comments)
            {
                game.Comments.Add(new Comment()
                {
                    Username = "Humans",
                    BodyText = humansComment
                });
            }

            foreach (var bubble in game.AudienceBubbles)
            {
                switch (bubble.Title)
                {
                    case "Humans":
                        bubble.Loyalty += aiResponse.Step2.Humans.FinalValue;
                        bubble.LoyaltyDelta = aiResponse.Step2.Humans.FinalValue;
                        break;
                    case "Orcs":
                        bubble.Loyalty += aiResponse.Step2.Orcs.FinalValue;
                        bubble.LoyaltyDelta = aiResponse.Step2.Orcs.FinalValue;
                        break;
                    case "Elves":
                        bubble.Loyalty += aiResponse.Step2.Elves.FinalValue;
                        bubble.LoyaltyDelta = aiResponse.Step2.Elves.FinalValue;
                        break;
                    case "Dwarves":
                        bubble.Loyalty += aiResponse.Step2.Dwarves.FinalValue;
                        bubble.LoyaltyDelta = aiResponse.Step2.Dwarves.FinalValue;
                        break;
                }
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

                game.PalantirBubbleShown = true;
                game.PalantirText = $"Greetings {game.Username}!";
                game.PalantirImageShown = false;
                game.ActionButtonText = "Start the game!";
                break;
            // case GameState.EmperorSelector:
            //     game.TweetInputBlocked = true;
            //     game.ActionButtonText = "Select your Emperor!";
            //     break;
            case GameState.TweetEntry:
                game.TweetInputBlocked = false;
                game.CommentsBlockShown = false;
                game.PalantirBubbleShown = true;
                game.PalantirText = game.NewsRecord.BodyText;
                game.PalantirImageShown = true;
                game.PalantirPhotoUrl = game.NewsRecord.ImageUrl;
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
                game.PalantirBubbleShown = false;

                game.PalantirText = game.NewsRecord.BodyText;
                game.PalantirImageShown = true;
                game.PalantirPhotoUrl = game.NewsRecord.ImageUrl;

                game.ActionButtonText = "Next day";

                break;
            case GameState.GameOver:
                game.TweetInputBlocked = true;
                game.CommentsBlockShown = false;
                game.PalantirBubbleShown = true;
                game.PalantirImageShown = false;

                game.ActionButtonText = "Start Over";
                break;
            default:
                throw new Exception("Unknown game state");
        }
        
        game.EmperorPhotoUrl = RetrieveEmperorPhoto(game);
    }

    private string RetrieveEmperorPhoto(GameViewModel game)
    {
        var happiness = new[]
        {
            EmperorHappiness.Ease, EmperorHappiness.Pleased, EmperorHappiness.Concerned, EmperorHappiness.Angry,
            EmperorHappiness.Despair
        }.OrderBy(x => new Random().Next(100)).First();
       

        return $"img/emperor-{happiness.ToString().ToLower()}.png";
    }
}

internal enum EmperorHappiness
{
    Ease,
    Pleased,
    Concerned,
    Angry,
    Despair
}