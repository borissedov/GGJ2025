using System.Text.Json;
using GlobalGameJam2025_Bubbles.Models;

namespace GlobalGameJam2025_Bubbles.Services;

public class NewsService
{
    private readonly NewsRecord[] _staticNewsRecords;

    public NewsService()
    {
        var newsFileContent = File.ReadAllText(Path.Combine("wwwroot", "news.json"));
        _staticNewsRecords = JsonSerializer.Deserialize<NewsRecord[]>(newsFileContent)!;
    }

    public NewsRecord GetNewsRecordForDay(int day)
    {
        return _staticNewsRecords[day - 1];
    }
}