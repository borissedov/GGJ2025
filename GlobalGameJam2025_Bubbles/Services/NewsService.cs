using GlobalGameJam2025_Bubbles.Models;

namespace GlobalGameJam2025_Bubbles.Services;

public class NewsService
{
    private readonly NewsRecord[] _staticNewsRecords = new NewsRecord[]
    {
        new ()
        {
            BodyText = "1. The empire is calm, and people are happy.",
            ToDo = "Maintain stability."
        },
        new ()
        {
            BodyText = "2. The empire is calm, and people are happy.",
            ToDo = "Maintain stability."
        },
        new ()
        {
            BodyText = "3. The empire is calm, and people are happy.",
            ToDo = "Maintain stability."
        },
    };

    public NewsRecord GetNewsRecordForDay(int day)
    {
        return _staticNewsRecords[day - 1];
    }
}