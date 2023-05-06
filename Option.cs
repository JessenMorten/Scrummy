public class Option<T>
{
    private readonly bool _hasValue;

    private readonly T? _value;

    public bool IsSome => _hasValue;

    public bool IsNone => !IsSome;

    public T Value => _value ?? throw new InvalidOperationException($"Option<{typeof(T).Name}> is none");

    private Option(T value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
        _hasValue = true;
    }

    private Option()
    {
        _value = default;
    }

    public static Option<T> Some(T value) => new Option<T>(value);

    public static Option<T> None() => new Option<T>();

    public R Match<R>(Func<T, R> some, Func<R> none)
    {
        return IsSome ? some(Value) : none();
    }

    public R MatchOr<R>(Func<T, R> some, R noneValue)
    {
        return IsSome ? some(Value) : noneValue;
    }

    public T SomeOr(T noneValue)
    {
        return IsSome ? Value : noneValue;
    }
}