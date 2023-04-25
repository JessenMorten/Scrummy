namespace Scrummy.Models;

public class Observation
{
    public required Issue Issue { get; init; }

    public required string Text { get; init; }
}