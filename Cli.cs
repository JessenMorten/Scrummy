namespace Scrummy;

public static class Cli
{
    public static bool HasFlag(string[] args, string flag, string prefix = "--")
    {
        return args.Contains(prefix + flag);
    }

    public static string GetRequiredArgument(string[] args, string name, string prefix = "--", char separator = '=')
    {
        var key = prefix + name + separator;
        var argument = args.FirstOrDefault(a => a.StartsWith(key) && a.Length > key.Length);

        if (argument is null)
        {
            System.Console.WriteLine($"'{name}' not provided, add missing argument, e.g.: {key}<some-value>");
            Environment.Exit(1);
        }

        return argument.Substring(key.Length).Trim();
    }

    public static string? GetOptionalArgument(string[] args, string name, string prefix = "--", char separator = '=')
    {
        var key = prefix + name + separator;
        var argument = args.FirstOrDefault(a => a.StartsWith(key) && a.Length > key.Length);

        return argument is null
            ? null
            : argument.Substring(key.Length).Trim();
    }
}