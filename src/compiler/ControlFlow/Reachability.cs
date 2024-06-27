using System.Diagnostics.CodeAnalysis;

namespace Noa.Compiler.ControlFlow;

/// <summary>
/// Represents the reachability of an AST node.
/// </summary>
public readonly struct Reachability : IEquatable<Reachability>
{
    private readonly bool isUnreachable;
    private readonly bool isConditional;
    private readonly UnreachabilitySource source;

    /// <summary>
    /// Whether the node is at all reachable.
    /// </summary>
    [MemberNotNullWhen(true, nameof(IsConditional))]
    [MemberNotNullWhen(false, nameof(Source))]
    public bool IsReachable => !isUnreachable;

    /// <summary>
    /// Whether the node is always and unconditionally reachable.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Source))]
    public bool IsUnconditionallyReachable => IsReachable && !isConditional;

    /// <summary>
    /// If the node is reachable, whether the node is conditionally so.
    /// </summary>
    public bool? IsConditional => IsReachable
        ? isConditional
        : null;

    /// <summary>
    /// If the node is unreachable, the source of the unreachability.
    /// </summary>
    public UnreachabilitySource? Source =>
        isUnreachable
            ? source
            : null;

    private Reachability(bool isUnreachable, bool isConditional, UnreachabilitySource source)
    {
        this.isUnreachable = isUnreachable;
        this.isConditional = isConditional;
        this.source = source;
    }

    /// <summary>
    /// The node is always and unconditionally reachable.
    /// </summary>
    public static Reachability Reachable { get; } = new(false, false, default);

    /// <summary>
    /// The node is conditionally reachable.
    /// </summary>
    /// <param name="source">The source of the potential unreachability.</param>
    public static Reachability ConditionallyReachable(UnreachabilitySource source) =>
        new(false, true, source);

    /// <summary>
    /// The node is always unreachable.
    /// </summary>
    /// <param name="source">The source of the unreachability.</param>
    public static Reachability Unreachable(UnreachabilitySource source) =>
        new(true, default, source);

    /// <summary>
    /// Joins the reachability with another.
    /// </summary>
    /// <param name="other">The reachability to join with.</param>
    public Reachability Join(Reachability other)
    {
        if (IsUnconditionallyReachable && other.IsUnconditionallyReachable) return Reachable;
        if (!IsReachable && !other.IsReachable) return Unreachable(Join(Source.Value, other.Source.Value));

        var sauce = (Source, other.Source) switch
        {
            ({} a, {} b) => Join(a, b),
            ({} x, null) => x,
            (null, {} x) => x,
            _ => throw new UnreachableException()
        };

        return new(false, true, sauce);
    }

    private static UnreachabilitySource Join(UnreachabilitySource a, UnreachabilitySource b) => (a, b) switch
    {
        (UnreachabilitySource.Return, _) => UnreachabilitySource.Return,
        (_, UnreachabilitySource.Return) => UnreachabilitySource.Return,
        (UnreachabilitySource.Break, _) => UnreachabilitySource.Break,
        (_, UnreachabilitySource.Break) => UnreachabilitySource.Break,
        _ => UnreachabilitySource.Continue
    };

    public bool Equals(Reachability other) =>
        isUnreachable == other.isUnreachable &&
        isConditional == other.isConditional &&
        source == other.source;

    public override bool Equals([NotNullWhen(true)] object? obj) =>
        obj is Reachability other && Equals(other);

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(isUnreachable);
        hashCode.Add(isConditional);
        hashCode.Add(source);
        return hashCode.ToHashCode();
    }

    public override string ToString()
    {
        if (IsUnconditionallyReachable) return "reachable";

        var type = isConditional
            ? "conditionally reachable"
            : "unreachable";
        var sauce = source switch
        {
            UnreachabilitySource.Return => "return",
            UnreachabilitySource.Break => "break",
            UnreachabilitySource.Continue => "continue",
            _ => throw new UnreachableException()
        };

        return $"{type} ({sauce})";
    }

    public static bool operator ==(Reachability a, Reachability b) => a.Equals(b);
    
    public static bool operator !=(Reachability a, Reachability b) => !a.Equals(b);
}

/// <summary>
/// The source of a node being unreachable.
/// </summary>
public enum UnreachabilitySource
{
    /// <summary>
    /// The node is unreachable after a return expression.
    /// </summary>
    Return,
    /// <summary>
    /// The node is unreachable after a break expression.
    /// </summary>
    Break,
    /// <summary>
    /// The node is unreachable after a continue expression.
    /// </summary>
    Continue
}
