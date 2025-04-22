namespace Noa.Compiler.Services.Context;

/// <summary>
/// The context at a specific position within a syntax tree.
/// </summary>
[Flags]
public enum SyntaxContext
{
    /// <summary>
    /// No context.
    /// </summary>
    None = 0,
    /// <summary>
    /// An expression.
    /// <c>(|</c>, <c>1 + |</c>, <c>let x = |</c>, ...
    /// </summary>
    Expression = 1 << 0,
    /// <summary>
    /// A statement.
    /// <c>let x = 1; |</c>, <c>{|</c>, ...
    /// </summary>
    Statement = 1 << 1,
    /// <summary>
    /// A parameter or variable.
    /// <c>let |</c>, <c>func f(|</c>, <c>(|</c>, ...
    /// </summary>
    ParameterOrVariable = 1 << 2,
    /// <summary>
    /// Immediately following the end of the body of an if-statement without an else clause.
    /// <c>if x {} |</c>
    /// </summary>
    PostIfBodyWithoutElse = 1 << 3,
    /// <summary>
    /// Any level of nesting inside a loop.
    /// <c>loop { |</c>
    /// </summary>
    InLoop = 1 << 4,
}
