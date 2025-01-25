namespace GlobalGameJam2025_Bubbles.Models;

public record GameViewModel
{
    public string Username { get; set; }
    
    public GameState GameState { get; set; }
    
    public string ActionButtonText { get; set; }

    
    public int DaysCounter { get; set; }
    
    public NewsRecord? NewsRecord { get; set; }
    
    public bool PalantirActive { get; set; }
    // public string PalantirTitle { get; set; }
    public string PalantirPhotoUrl { get; set; }
    public string PalantirText { get; set; }
    // public int PalantirDelta { get; set; }
    
    public IList<AudienceBubble> AudienceBubbles { get; set; }
    
    public string Tweet { get; set; }
    
    public IList<Comment> Comments { get; set; }

    public string EmperorPhotoUrl { get; set; }
    public string EmperorHapinessText { get; set; }
    public int PeopleLoyalty { get; set; }
    public bool TweetInputBlocked { get; set; }
    public bool CommentsBlockShown { get; set; }
}

public enum GameState
{
    Greetings,
    EmperorSelector,
    TweetEntry,
    //Waiting,
    CommentsShow,
    GameOver
}

public record AudienceBubble
{
    public string Title { get; set; }
    public string AvatarUrl { get; set; }
    public string BodyText { get; set; }
    public string ColorCode { get; set; }
    public int Loyalty { get; set; }  
    public int LoyaltyDelta { get; set; }
    public int Radius { get; set; }
}

public record Comment
{
    public string Username { get; set; }
    public string AvatarUrl { get; set; }
    public string BodyText { get; set; }
}

public record NewsRecord
{
    public string BodyText { get; set; }
    public string ToDo { get; set; }
    // public string Source { get; set; }
}
