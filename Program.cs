using Scrummy;
using Scrummy.Analysis;
using Scrummy.Analysis.IssueAnalyzers;
using Scrummy.Analysis.SnapshotAnalyzers;
using Scrummy.Data;
using Scrummy.Models;
using Scrummy.Services;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

var filterName = args.FirstOrDefault();

if (string.IsNullOrWhiteSpace(filterName))
{
    Cli.Error("No filter provided");
    Environment.Exit(1);
}

var configFileName = Cli.GetOptionalArgument(args, "config") ?? "config.yaml";
var shouldSnap = Cli.HasFlag(args, "snap");
var shouldPeek = Cli.HasFlag(args, "peek");
var shouldClean = Cli.HasFlag(args, "clean");

if (!File.Exists(configFileName))
{
    Cli.Error($"Config file not found '{configFileName}'");
    Environment.Exit(1);
}

var stopTimer = Cli.TimedLog($"Reading {configFileName}");
var rawConfig = await File.ReadAllTextAsync(configFileName);
stopTimer();

stopTimer = Cli.TimedLog("Deserializing config");
var deserializer = new DeserializerBuilder()
    .WithNamingConvention(UnderscoredNamingConvention.Instance)
    .Build();
var config = deserializer.Deserialize<Config>(rawConfig);
stopTimer();

if (!config.Filters.ContainsKey(filterName))
{
    Cli.Error($"Unknown filter provided '{filterName}'");
    Environment.Exit(1);
}
var filter = config.Filters[filterName];

var service = new YouTrackService(new()
{
    Url = config.ApiUrl,
    Token = config.ApiToken,
    Filter = filter
});

var issueAnalyzers = new IIssueAnalyzer[]
{
    new HighEtcAnalyzer(),
    new NoEtcAnalyzer(),
    new InProgressAndUnassignedAnalyzer(),
    new IsDoneAndHasEtcAnalyzer(),
    new IsNotDoneAndHasZeroEtcAnalyzer(),
    new UnassignedAnalyzer(),
    new ReassignedAnalyzer(),
    new AssignedAnalyzer(),
    new AddedAnalyzer(),
    new RemovedAnalyzer(),
    new IncreasedEtcAnalyzer(),
    new IncreasedEstimateAnalyzer(),
    new TimeSpentVersusEtcAnalyzer(),
    new StateChangeAnalyzer(),
    new BlockedAnalyzer(),
    new NoEstimateAnalyzer(),
    new SummaryChangedAnalyzer()
};

var snapshotAnalyzers = new ISnapshotAnalyzer[]
{
    new HighestEtcAnalyzer(),
    new LowestEtcAnalyzer(),
    new PercentBlockedAnalyzer(),
    new TotalEstimateAnalyzer(),
    new TotalEtcAnalyzer(),
    new DeviationAnalyzer()
};

var storage = new SnapshotStorage(filterName);

if (shouldClean)
{
    stopTimer = Cli.TimedLog($"Cleaning '{filterName}' storage");
    storage.Clean();
    stopTimer();
}

stopTimer = Cli.TimedLog("Loading all snapshots");
var snapshots = (await storage.Load()).ToList();
stopTimer();

if (shouldPeek || shouldSnap)
{
    stopTimer = Cli.TimedLog("Fetching issues");
    var issues = await service.GetIssues();
    stopTimer();

    var snapshot = new Snapshot { Timestamp = DateTime.Now, Issues = issues };
    snapshots.Add(snapshot);

    if (shouldSnap)
    {
        stopTimer = Cli.TimedLog("Storing snapshot");
        await storage.Store(snapshot);
        stopTimer();
    }
}

if (!snapshots.Any())
{
    Cli.Warn("There are no snapshots. Create a new snapshot with the --snap flag.");
    Environment.Exit(0);
}

var ordered = snapshots.OrderByDescending(s => s.Timestamp);
var current = ordered.First();
var previous = snapshots.Count() > 1 ? ordered.ElementAt(1) : current;
var timeDiff = DateTime.Now - current.Timestamp;

if (snapshots.Count() == 1)
{
    Cli.Warn("Only 1 snapshot exists, create a another snapshot using the --snap flag.");
}
else if (timeDiff > TimeSpan.FromHours(1))
{
    Cli.Warn($"Newest snapshot is more than {(int)timeDiff.TotalHours}h old, create a new snapshot using the --snap flag.");
}

var results = new List<IAnalysisResult>();

var distinctIssueIds = current.Issues.Concat(previous.Issues).Select(i => i.Id).Distinct();
stopTimer = Cli.TimedLog($"Analyzing {distinctIssueIds.Count()} issues with {issueAnalyzers.Length} issue analyzers");
foreach (var issueId in distinctIssueIds)
{
    var currentIssue = current.Issues.SingleOrDefault(i => i.Id == issueId);
    var previousIssue = previous.Issues.SingleOrDefault(i => i.Id == issueId);
    var currentIssueOption = currentIssue is null ? Option<Issue>.None() : Option<Issue>.Some(currentIssue);
    var previousIssueOption = previousIssue is null ? Option<Issue>.None() : Option<Issue>.Some(previousIssue);

    foreach (var analyzer in issueAnalyzers)
    {
        var result = analyzer.Analyze(previousIssueOption, currentIssueOption);
        if (result.IsSome)
        {
            results.Add(result.Value);
        }
    }
}
stopTimer();

stopTimer = Cli.TimedLog($"Analyzing 2 snapshots with {snapshotAnalyzers.Length} snapshot analyzers");
foreach (var analyzer in snapshotAnalyzers)
{
    var result = analyzer.Analyze(previous, current);
    if (result.IsSome)
    {
        results.Add(result.Value);
    }
}
stopTimer();

foreach (var groupedResults in results.GroupBy(r => r.RelatedTo))
{
    Cli.Log("\n" + groupedResults.Key);

    foreach (var r in groupedResults)
    {
        Action<string> log = r.Severity switch
        {
            Severity.Info => Cli.Info,
            Severity.Warning => Cli.Warn,
            _ => throw new NotSupportedException($"Unexpected severity '{r.Severity}'")
        };

        log("    - " + r.Text);
    }
}
