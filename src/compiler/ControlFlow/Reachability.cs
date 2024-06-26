namespace Noa.Compiler.ControlFlow;

/// <summary>
/// Represents the reachability of an AST node.
/// </summary>
public enum Reachability
{
    /// <summary>
    /// The node is always reachable.
    /// </summary>
    Reachable,
    /// <summary>
    /// The node is reachable depending on a condition.
    /// </summary>
    ConditionallyReachable,
    /// <summary>
    /// The node is always unreachable.
    /// </summary>
    Unreachable
}

public static class ReachabilityExtensions
{
    /// <summary>
    /// Combines two reachabilities into a combined one.
    /// </summary>
    public static Reachability Combine(this Reachability a, Reachability b) => (a, b) switch
    {
        (Reachability.Reachable, Reachability.Reachable) => Reachability.Reachable,
        (Reachability.Unreachable, Reachability.Unreachable) => Reachability.Unreachable,
        _ => Reachability.ConditionallyReachable
    };
}
