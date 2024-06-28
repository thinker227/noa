using System.Diagnostics.CodeAnalysis;

namespace Noa.Compiler.ControlFlow;

/// <summary>
/// Represents the control flow reachability of an AST node.
/// </summary>
/// <param name="CanFallThrough">Whether the node can be reached by falling through execution of other nodes.</param>
/// <param name="CanReturn">Whether a return expression may have been executed before reaching this node.</param>
/// <param name="CanBreak">Whether a break expression may have been executed before reaching this node.</param>
/// <param name="CanContinue">Whether a continue expression may have been executed before reaching this node.</param>
public readonly record struct Reachability(
    bool CanFallThrough,
    bool CanReturn,
    bool CanBreak,
    bool CanContinue)
{
    /// <summary>
    /// Performs a logical or operation on the elements of two reachabilities.
    /// </summary>
    public static Reachability operator |(Reachability a, Reachability b) =>
        new(a.CanFallThrough || b.CanFallThrough,
            a.CanReturn || b.CanReturn,
            a.CanBreak || b.CanBreak,
            a.CanContinue || b.CanContinue);

    /// <summary>
    /// Whether the node can at all be reached.
    /// </summary>
    public bool IsReachable => CanFallThrough;

    /// <summary>
    /// The default unconditionally reachable state.
    /// </summary>
    public static Reachability Reachable { get; } = new(
        true,
        false,
        false,
        false);

    /// <summary>
    /// An unconditionally unreachable state.
    /// </summary>
    public static Reachability Unreachable { get; } = new(
        false,
        false,
        false,
        false);

    public override string ToString()
    {
        var state = IsReachable
            ? "reachable"
            : "unreachable";
        
        var modifiers = new List<string>(3);
        if (CanReturn) modifiers.Add("return");
        if (CanBreak) modifiers.Add("break");
        if (CanContinue) modifiers.Add("continue");

        return modifiers is not []
            ? $"{state} ({string.Join(" | ", modifiers)})"
            : state;
    }
}
