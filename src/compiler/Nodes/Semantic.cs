namespace Noa.Compiler.Nodes;

/// <summary>
/// A semantic value which may not be resolved.
/// Like <see cref="Nullable{T}"/> but more specific.
/// </summary>
/// <typeparam name="T">The type of the semantic value.</typeparam>
/// <param name="value">The constructed semantic value.</param>
public readonly struct Semantic<T>(T value)
{
    private readonly T? value = value;

    /// <summary>
    /// Whether the value is resolved or not.
    /// </summary>
    public bool IsResolved { get; } = true;

    /// <summary>
    /// The resolved semantic value.
    /// Throws an <see cref="InvalidOperationException"/> if <see cref="IsResolved"/> is false.
    /// </summary>
    /// <exception cref="InvalidOperationException"><see cref="IsResolved"/> is false.</exception>
    public T Value =>
        IsResolved
            ? value!
            : throw new InvalidOperationException(
                "Cannot access an unresolved semantic value.");

    /// <summary>
    /// Maps the semantic value.
    /// </summary>
    /// <typeparam name="TResult">The type of the new value.</typeparam>
    /// <param name="f">The function to map the value.</param>
    public Semantic<TResult> Map<TResult>(Func<T, TResult> f) =>
        IsResolved
            ? new(f(Value))
            : new();

    /// <summary>
    /// Implicitly constructs a semantic value.
    /// </summary>
    /// <param name="value">The semantic value.</param>
    public static implicit operator Semantic<T>(T value) => new(value);

    public override string? ToString() =>
        IsResolved
            ? Value?.ToString()
            : "<not resolved>";
}
