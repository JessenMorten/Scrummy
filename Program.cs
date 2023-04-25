using Scrummy;
using Scrummy.Data;
using Scrummy.Models;
using Scrummy.Reporters;
using Scrummy.Services;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

var filterName = args.FirstOrDefault(a => a.StartsWith("--filter=")).Split("=").Last();

var rawConfig = await File.ReadAllTextAsync("config.yaml");

var deserializer = new DeserializerBuilder()
    .WithNamingConvention(UnderscoredNamingConvention.Instance)
    .Build();

var config = deserializer.Deserialize<Config>(rawConfig);
var filter = config.Filters[filterName];

IIssuesService service = new YouTrackService(new()
{
    Url = config.ApiUrl,
    Token = config.ApiToken,
    Filter = filter
});

var reporters = new IReporter[]
{
    new IllegalIssueStateReporter(),
    new NewsReporter()
};

var storage = new SnapshotStorage(filterName);

if (args is not null && args.Contains("--snap"))
{
    System.Console.WriteLine("Creating snapshot...");
    var issues = await service.GetIssues();
    var snapshot = new Snapshot { Timestamp = DateTime.Now, Issues = issues };
    await storage.Store(snapshot);
}

var snapshots = await storage.Load();

if (!snapshots.Any())
{
    LogWarning("There are no snapshots. Create a new snapshot with the --snap flag.");
    Environment.Exit(0);
}

var ordered = snapshots.OrderByDescending(s => s.Timestamp);
var current = ordered.First();
var previous = snapshots.Count() > 1 ? ordered.ElementAt(1) : current;
var timeDiff = DateTime.Now - current.Timestamp;

if (snapshots.Count() == 1)
{
    LogWarning("Only 1 snapshot exists, create a another snapshot using the --snap flag.");
}
else if (timeDiff > TimeSpan.FromHours(1))
{
    LogWarning($"Newest snapshot is more than {(int)timeDiff.TotalHours}h old, create a new snapshot using the --snap flag.");
}

foreach (var reporter in reporters)
{
    var observations = reporter.GetObservations(previous.Issues, current.Issues);
    System.Console.WriteLine($"{reporter.GetType().Name} has {observations.Count()} observations");

    var grouped = observations.GroupBy(o => o.Issue);

    foreach (var group in grouped)
    {
        System.Console.WriteLine($"    - {group.Key.Id}: {group.Key.Summary}");
        var ass = group.Key.Assignee.IsSome ? group.Key.Assignee.Value.FullName : "nobody";
        System.Console.WriteLine($"    - Assigned to {ass}");

        foreach (var observation in group)
        {
            System.Console.WriteLine($"      -> {observation.Text}");
        }
    }
}

static void LogWarning(string message)
{
    Console.WriteLine(message);
    Thread.Sleep(2000);
}
