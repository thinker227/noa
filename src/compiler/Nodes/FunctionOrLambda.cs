using System.Diagnostics.CodeAnalysis;

namespace Noa.Compiler.Nodes;

/// <summary>
/// Either a function declaration or a lambda expression.
/// </summary>
public sealed class FunctionOrLambda
{
    /// <summary>
    /// Whether the node is a function or lambda.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Function))]
    [MemberNotNullWhen(true, nameof(Lambda))]
    public required bool IsLambda { get; init; }
    
    /// <summary>
    /// The function node.
    /// </summary>
    public required FunctionDeclaration? Function { get; init; }
    
    /// <summary>
    /// The lambda expression node.
    /// </summary>
    public required LambdaExpression? Lambda { get; init; }
}
