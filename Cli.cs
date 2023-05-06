using System.Diagnostics;
using System.Drawing;
using Pastel;

namespace Scrummy;

public static class Cli
{
    public static Action TimedLog(string message)
    {
        Console.Write(message.PadRight(50, '.').Pastel(Color.Gray));
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        return () =>
        {
            stopwatch.Stop();
            var elapsed = " " + stopwatch.ElapsedMilliseconds + "ms.";
            stopwatch = null;
            Console.WriteLine(elapsed.Pastel(Color.Gray));
        };
    }

    public static void Error(string message)
    {
        Console.WriteLine(message.Pastel(Color.Red));
    }

    public static void Warn(string message)
    {
        Console.WriteLine(message.Pastel(Color.Orange));
    }

    public static void Info(string message)
    {
        Console.WriteLine(message.Pastel(Color.Cyan));
    }

    public static void Log(string message)
    {
        Console.WriteLine(message.Pastel(Color.WhiteSmoke));
    }

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
            Error($"'{name}' not provided, add missing argument, e.g.: {key}<some-value>");
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