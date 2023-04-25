using System.Text.Json;
using Scrummy.Models;

namespace Scrummy.Data;

public class SnapshotStorage
{
    private readonly string _dataDirectory;

    public SnapshotStorage(string filter)
    {
        _dataDirectory = Path.Combine($"{nameof(Scrummy)}{nameof(Scrummy.Data)}", filter);
    }

    public async Task Store(Snapshot snapshot)
    {
        var entities = snapshot.Issues.Select(IssueEntity.From);
        var json = JsonSerializer.Serialize(entities);
        var fileName = GetFileName(snapshot.Timestamp);
        var path = Path.Combine(EnsureDir(), fileName);
        await File.WriteAllTextAsync(path, json);
    }

    public async Task<IEnumerable<Snapshot>> Load()
    {
        var files = Directory.GetFiles(EnsureDir());
        var snapshots = new List<Snapshot>();

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            var timestamp = GetDateTime(fileName);
            var json = await File.ReadAllTextAsync(file);
            var entities = JsonSerializer.Deserialize<IssueEntity[]>(json);

            snapshots.Add(new Snapshot
            {
                Timestamp = timestamp,
                Issues = entities.Select(e => e.ToIssue()).ToArray()
            });
        }

        return snapshots;
    }

    private string EnsureDir()
    {
        if (!Directory.Exists(_dataDirectory))
        {
            Directory.CreateDirectory(_dataDirectory);
        }

        return _dataDirectory;
    }

    private static DateTime GetDateTime(string fileName)
    {
        var numbers = fileName.Replace(".json", "").Split("-");
        var year = int.Parse(numbers[0]);
        var month = int.Parse(numbers[1]);
        var day = int.Parse(numbers[2]);
        var hour = int.Parse(numbers[3]);
        var minute = int.Parse(numbers[4]);
        var second = int.Parse(numbers[5]);
        return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local);
    }

    private static string GetFileName(DateTime dt)
    {
        return $"{dt.Year}-{dt.Month}-{dt.Day}-{dt.Hour}-{dt.Minute}-{dt.Second}.json";
    }
}
