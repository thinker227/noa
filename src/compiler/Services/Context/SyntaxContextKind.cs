namespace Noa.Compiler.Services.Context;

/// <summary>
/// Specifies what kind of context a <see cref="SyntaxContext"/> is.
/// </summary>
[Flags]
public enum SyntaxContextKind
{
    /// <summary>
    /// No context.
    /// </summary>
    None = 0,
    /// <summary>
    /// An expression, not including expression statements.
    /// <c>(|</c>, <c>1 + |</c>, <c>let x = |</c>, ...
    /// </summary>
    Expression = 1 << 0,
    /// <summary>
    /// Immediately following an expression.
    /// </summary>
    PostExpression = 1 << 1,
    /// <summary>
    /// A statement.
    /// <c>let x = 1; |</c>, <c>{|</c>, ...
    /// </summary>
    Statement = 1 << 3,
    /// <summary>
    /// A parameter or variable.
    /// <c>let |</c>, <c>func f(|</c>, <c>(|</c>, ...
    /// </summary>
    ParameterOrVariable = 1 << 4,
    /// <summary>
    /// Immediately following the end of the body of an if-statement without an else clause.
    /// <c>if x {} |</c>
    /// </summary>
    PostIfBodyWithoutElse = 1 << 5,
    /// <summary>
    /// Any level of nesting inside a loop.
    /// <c>loop { |</c>
    /// </summary>
    InLoop = 1 << 6,
}
