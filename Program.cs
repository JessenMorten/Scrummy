using Scrummy;
using Scrummy.Data;
using Scrummy.Models;
using Scrummy.Reporters;
using Scrummy.Services;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

var filterName = Cli.GetRequiredArgument(args, "filter");
var configFileName = Cli.GetOptionalArgument(args, "config") ?? "config.yaml";
var shouldSnap = Cli.HasFlag(args, "snap");

if (!File.Exists(configFileName))
{
    System.Console.WriteLine($"Config file not found '{configFileName}'");
    Environment.Exit(1);
}

var rawConfig = await File.ReadAllTextAsync(configFileName);
var deserializer = new DeserializerBuilder()
    .WithNamingConvention(UnderscoredNamingConvention.Instance)
    .Build();
var config = deserializer.Deserialize<Config>(rawConfig);

if (!config.Filters.ContainsKey(filterName))
{
    System.Console.WriteLine($"Unknown filter provided '{filterName}'");
    Environment.Exit(1);
}
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

if (shouldSnap)
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
    var grouped = observations.GroupBy(o => o.Issue);

    foreach (var group in grouped)
    {
        var width = 69;
        var maxLength = width - 1;
        var assigned = group.Key.Assignee.IsSome ? "Assigned to " + group.Key.Assignee.Value.FullName : "Unassigned";
        System.Console.WriteLine($"╔{"".PadRight(width + 1, '═')}╗");
        System.Console.WriteLine($"║ {Truncate(group.Key.Summary, maxLength).PadRight(width)}║");
        System.Console.WriteLine($"║ {assigned.PadRight(width)}║");
        System.Console.WriteLine($"║ {group.Key.Url.ToString().PadRight(width)}║");

        foreach (var observation in group)
        {
            System.Console.WriteLine($"║     -> {observation.Text.PadRight(width - 7)}║");
        }

        System.Console.WriteLine($"╚{"".PadRight(width + 1, '═')}╝");
    }
}

static void LogWarning(string message)
{
    Console.WriteLine(message);
    Thread.Sleep(2000);
}

static string Truncate(string message, int maxLength)
{
    return message.Length > maxLength
        ? message.Substring(0, maxLength - 3) + "..."
        : message;
}