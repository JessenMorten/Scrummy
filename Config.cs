namespace Scrummy;

public class Config
{
    public string ApiUrl { get; set; } = "";

    public string ApiToken { get; set; } = "";

    public Dictionary<string, string> Filters { get; set; } = new();
}
