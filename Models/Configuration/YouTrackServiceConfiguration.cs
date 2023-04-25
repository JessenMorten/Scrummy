namespace Scrummy.Models.Configuration;

public class YouTrackServiceConfiguration
{
    public required string Token { get; set; }

    public required string Url { get; set; }

    public required string Filter { get; set; }
}