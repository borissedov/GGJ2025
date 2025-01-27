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
    
    public int[] GenerateRandomPermutation(int length, int seed = 0)
    {
        var rng = seed == 0 ? Random.Shared : new Random(seed);
        var array = Enumerable.Range(0, length).ToArray();

        // Fisher-Yates shuffle
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }

        return array;
    }

    public NewsRecord GetNewsRecordForDay(int day, int[] permutationArray)
    {
        if (day < 1 || day > permutationArray.Length)
            throw new ArgumentOutOfRangeException(nameof(day), "Day is out of range of the order array.");

        var randomIndex = permutationArray[day - 1];

        return _staticNewsRecords[randomIndex];
    }
}