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
            var loyalty = 10;

            bubbles.Add(new AudienceBubble
            {
                Title = bubbleTitles[i],
                AvatarUrl = avatarUrl,
                BodyText =
                    $"{bubbleTitles[i]} are a vital part of the empire. They contribute to its strength and stability.",
                ColorCode = bubbleColors[i],
                Loyalty = loyalty
            });
        }

        // Create the GameViewModel with dynamic data
        var game = new GameViewModel()
        {
            Username = string.IsNullOrWhiteSpace(username) ? "Guest" : username,
            Comments = new List<Comment>(),
            DaysCounter = 1,
            AudienceBubbles = bubbles,
            PeopleLoyalty = 5,
            EmperorHapinessText = "Content",
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
            
            if (aiResponse.Step2.Elves != null && aiResponse.Step2.Elves.Comments.Any())
                foreach (var elvesComment in aiResponse.Step2.Elves.Comments)
                {
                    game.Comments.Add(new Comment()
                    {
                        Username = "Elves",
                        BodyText = elvesComment
                    });
                }

            if (aiResponse.Step2.Orcs != null && aiResponse.Step2.Orcs.Comments.Any())
                foreach (var orcsComment in aiResponse.Step2.Orcs.Comments)
                {
                    game.Comments.Add(new Comment()
                    {
                        Username = "Orcs",
                        BodyText = orcsComment
                    });
                }

            if (aiResponse.Step2.Dwarves != null && aiResponse.Step2.Dwarves.Comments.Any())
                foreach (var dwarvesComment in aiResponse.Step2.Dwarves.Comments)
                {
                    game.Comments.Add(new Comment()
                    {
                        Username = "Dwarves",
                        BodyText = dwarvesComment
                    });
                }

            if (aiResponse.Step2.Humans != null && aiResponse.Step2.Humans.Comments.Any())
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
                int loyalty;
                
                switch (bubble.Title)
                {
                    case "Humans":
                        loyalty = (aiResponse.Step2.Humans?.FinalValue ?? 0) / 2;
                        break;
                    case "Orcs":
                        loyalty = (aiResponse.Step2.Orcs?.FinalValue ?? 0) / 2;
                        break;
                    case "Elves":
                        loyalty = (aiResponse.Step2.Elves?.FinalValue ?? 0) / 2;
                        break;
                    case "Dwarves":
                        loyalty = (aiResponse.Step2.Dwarves?.FinalValue ?? 0) / 2;
                        break;
                    default:
                        loyalty = 0;
                        break;
                }
                
                bubble.Loyalty += loyalty;
                if(bubble.Loyalty < 0)
                {
                    bubble.Loyalty = 0;
                }
                if(bubble.Loyalty > 10)
                {
                    bubble.Loyalty = 10;
                }
                bubble.LoyaltyDelta = loyalty;
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
                if(game.AudienceBubbles.Any(b=>b.Loyalty == 0))
                {
                    return GameState.GameOver;
                }

                if (game.DaysCounter == _maxGameDays)
                {
                    return GameState.GameOverGood;
                }

                return GameState.TweetEntry;
            }
            case GameState.GameOver:
                return GameState.Greetings;
            case GameState.GameOverGood:
                return GameState.Greetings;
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
                game.PalantirText = $"Welcome {game.Username}, Minister of Truth!\n\nYour task is clear: rewrite the narrative, secure loyalty, and ensure the Emperor’s glory. Take up your quill and turn scandals into triumphs.\n The fate of the Empire is in your hands.";
                game.PalantirImageShown = false;
                game.ActionButtonText = "Start the game!";
                
                game.Comments = new List<Comment>();
                
                game.Tweet = "";
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
                game.Tweet = "";
                game.Comments = new List<Comment>();
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
                game.PalantirBubbleShown = true;

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
                game.PalantirText =
                    $"Game Over: The Emperor Has Fallen\n\nThe rebellion has toppled the Emperor, and your role as Minister of Truth is over. If you escape judgment, perhaps another dictator will need your talents. For now, your time is done.";
                
                game.Tweet = "";
                game.ActionButtonText = "Start Over";
            break;
            case GameState.GameOverGood:
                game.TweetInputBlocked = true;
                game.CommentsBlockShown = false;
                game.PalantirBubbleShown = true;
                game.PalantirImageShown = false;
                game.PalantirText =
                    $"Congratulations {game.Username}, Minister of Truth! You’ve masterfully spun every story, secured unwavering loyalty, and kept the Emperor firmly on his throne. But stay sharp—new news are always on the horizon!";
                game.Tweet = "";

                game.ActionButtonText = "Start Over";
                break;
            default:
                throw new Exception("Unknown game state");
        }

        game.EmperorPhotoUrl = RetrieveEmperorPhoto(game);
    }

    private string RetrieveEmperorPhoto(GameViewModel game)
    {
        var minLoyalty = game.AudienceBubbles.Min(b => b.Loyalty);
        var index = Math.Min(5 - minLoyalty / 2, 4);
        var happinesses = new[]
        {
            EmperorHappiness.Ease, EmperorHappiness.Pleased, EmperorHappiness.Concerned, EmperorHappiness.Angry,
            EmperorHappiness.Despair
        };


        return $"img/emperor-{happinesses[index].ToString().ToLower()}.png";
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