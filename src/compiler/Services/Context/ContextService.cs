using Noa.Compiler.Nodes;
using Noa.Compiler.Symbols;
using Noa.Compiler.Syntax;
using SuperLinq;

namespace Noa.Compiler.Services.Context;

/// <summary>
/// Service for fetching syntax contexts.
/// </summary>
public static class ContextService
{
    /// <summary>
    /// Fetches the syntax context for a specified position within an AST.
    /// </summary>
    /// <param name="ast">The AST to get the context within.</param>
    /// <param name="position">The position within the AST to get the context at.</param>
    public static SyntaxContext GetSyntaxContext(this Ast ast, int position)
    {
        var rightToken = ast.SyntaxRoot.GetTokenAt(position);
        var leftToken = rightToken?.GetPreviousToken();

        // "Marty! You're just not thinking 4th dimensionally! Don't you see? The bridge *will* exist in 1985!"
        // This method contains a lot of "potentially"s and speculation, because we want to provide
        // as complete of a context as possible regardless of what the user is about to write.
        // This involves interpreting constructs which currently look like one thing as something else
        // because they might turn into something different after the user has written something new.

        // After a block which currently is an expression
        // but which might end up being a flow control statement.
        var isAfterPotentialFlowControlStatement = leftToken
            // { {} | }
            // { if x {} else {} | }
            is {
                Kind: TokenKind.CloseBrace,
                ParentNode:
                    BlockExpressionSyntax {
                        Parent:
                            BlockSyntax or
                            ElseClauseSyntax {
                                Parent: IfExpressionSyntax {
                                    Parent: not FlowControlStatementSyntax
                                }
                            }
                    }
            };           

        // After *strictly* a statement and not a potential flow control statement.
        var isStrictlyAfterStatement = leftToken
            // let x = 0; |
            // x = 0; |
            // f(); |
            // func f() => 0; |
            is {
                Kind: TokenKind.Semicolon,
                ParentNode:
                    LetDeclarationSyntax or
                    AssignmentStatementSyntax or
                    ExpressionStatementSyntax or
                    ExpressionBodySyntax
            }
            // {} | let x = 0;
            // func f() {} | let x = 0;
            // if x {} |
            // if x {} else {} | let x = 0;
            // loop {} | let x = 0;
            // func f() {} |
            or {
                Kind: TokenKind.CloseBrace,
                ParentNode:
                    BlockExpressionSyntax {
                        Parent: FlowControlStatementSyntax or BlockBodySyntax
                    } or
                    BlockExpressionSyntax {
                        Parent:
                            IfExpressionSyntax {
                                Parent: FlowControlStatementSyntax
                            } or
                            ElseClauseSyntax {
                                Parent: IfExpressionSyntax {
                                    Parent: FlowControlStatementSyntax
                                }
                            } or
                            LoopExpressionSyntax {
                                Parent: FlowControlStatementSyntax
                            }
                    }
            };

        // Expressions
        var isExpression = isAfterPotentialFlowControlStatement || leftToken
            // {|}
            is {
                Kind: TokenKind.OpenBrace,
                ParentNode: BlockExpressionSyntax { Block.Statements: [] }
            }
            // let x = |
            or {
                Kind: TokenKind.Equals,
                ParentNode: LetDeclarationSyntax
            }
            // x = |
            // including compound assignment tokens
            or {
                Kind:
                    TokenKind.Equals or
                    TokenKind.PlusEquals or
                    TokenKind.DashEquals or
                    TokenKind.StarEquals or
                    TokenKind.SlashEquals,
                ParentNode: AssignmentStatementSyntax
            }
            // (|
            or {
                Kind: TokenKind.OpenParen,
                ParentNode:
                    // Parens will typically auto-complete the closing paren,
                    // so if the user is about to type a parenthesized expression
                    // then it will start off looking like a nil expression.
                    ParenthesizedExpressionSyntax or
                    NilExpressionSyntax or
                    TupleExpressionSyntax or
                    CallExpressionSyntax
            }
            // (x) => |
            // func f() => |
            or {
                Kind: TokenKind.EqualsGreaterThan,
                ParentNode: LambdaExpressionSyntax or ExpressionBodySyntax
            }
            // (x, |)
            or {
                Kind: TokenKind.Comma,
                ParentNode: SeparatedSyntaxList<ExpressionSyntax>
            }
            // if |
            or {
                Kind: TokenKind.If,
                ParentNode: IfExpressionSyntax
            }
            // return |
            or {
                Kind: TokenKind.Return,
                ParentNode: ReturnExpressionSyntax
            }
            // break |
            or {
                Kind: TokenKind.Break,
                ParentNode: BreakExpressionSyntax
            }
            // Unary expressions
            or {
                Kind: TokenKind.Plus or TokenKind.Dash,
                ParentNode: UnaryExpressionSyntax
            }
            // Binary expressions
            or {
                Kind:
                    TokenKind.Plus or
                    TokenKind.Dash or
                    TokenKind.Star or
                    TokenKind.Slash or
                    TokenKind.EqualsEquals or
                    TokenKind.BangEquals or
                    TokenKind.LessThan or
                    TokenKind.GreaterThan or
                    TokenKind.LessThanEquals or
                    TokenKind.GreaterThanEquals,
                ParentNode: BinaryExpressionSyntax
            };
        
        // Check if the context could be a trailing expression after a statement.
        var isTrailingExpression = false;
        if (isStrictlyAfterStatement)
        {
            var statement = leftToken?.GetFirstAncestorOfType<StatementSyntax>()!;
            var statementIndex = statement.GetIndexInParent();
            var block = statement.GetFirstAncestorOfType<BlockSyntax>()!;
            isTrailingExpression =
                statementIndex == block.Statements.Count - 1 &&
                block.TrailingExpression is null;
            isExpression |= isTrailingExpression;
        }

        // After an expression
        var isPostExpression = leftToken
            // true |
            // false |
            is {
                ParentNode: BoolExpressionSyntax
            }
            // 0 |
            or {
                ParentNode: NumberExpressionSyntax
            }
            // "uwu" |
            or {
                Kind: TokenKind.EndString,
                ParentNode: StringExpressionSyntax
            }
            // x |
            or {
                ParentNode: IdentifierExpressionSyntax
            }
            // () |
            // (x) |
            // (a, b) |
            // f() |
            or {
                Kind: TokenKind.CloseParen,
                ParentNode:
                    NilExpressionSyntax or
                    ParenthesizedExpressionSyntax or
                    TupleExpressionSyntax or
                    CallExpressionSyntax
            }
            // let x = {} |
            // let x = if y {} else {} |
            // let x = loop {} |
            // 
            // Specifically not when the expression is a flow control statement.
            or {
                Kind: TokenKind.CloseBrace,
                ParentNode:
                    BlockExpressionSyntax {
                        Parent:
                            ElseClauseSyntax {
                                Parent: IfExpressionSyntax {
                                    Parent: not FlowControlStatementSyntax
                                }
                            } or
                            LoopExpressionSyntax {
                                Parent: not FlowControlStatementSyntax
                            }
                    } or
                    BlockExpressionSyntax {
                        Parent: not (
                            IfExpressionSyntax or
                            BlockBodySyntax
                        )
                    }
            };
        
        // At the very start of a block
        var isAtStartOfBlock = leftToken
            is {
                Kind: TokenKind.OpenBrace,
                ParentNode: BlockExpressionSyntax
            }
            or null;
        
        var isAtEndOfBlock = rightToken
            is {
                Kind: TokenKind.CloseBrace,
                ParentNode: BlockExpressionSyntax
            }
            or {
                Kind: TokenKind.EndOfFile,
                ParentNode: RootSyntax
            };

        // Statements
        var isStatement = isAfterPotentialFlowControlStatement || isStrictlyAfterStatement || isAtStartOfBlock;
        
        // Parameters or variables
        var isParameterOrVariable = leftToken
            // let |
            is {
                Kind: TokenKind.Let,
                ParentNode: LetDeclarationSyntax
            }
            // (|)
            // (|a, b)
            or {
                Kind: TokenKind.OpenParen,
                ParentNode:
                    // Parens will typically auto-complete the closing paren,
                    // so if the user is about to type a lambda expression
                    // then it will start off looking like a nil expression.
                    NilExpressionSyntax or
                    ParameterListSyntax
            }
            // (a, |)
            or {
                Kind: TokenKind.Comma,
                ParentNode: SeparatedSyntaxList<ParameterSyntax>
            };
        
        // After an if body without an else clause
        var isPostIfBodyWithoutElse = leftToken
            // if x {} |
            is {
                Kind: TokenKind.CloseBrace,
                ParentNode: BlockExpressionSyntax {
                    Parent: IfExpressionSyntax {
                        Else: null
                    }
                }
            };
        
        var isInLoop = leftToken is not null && IsInLoop(ast, leftToken);
        
        var kind = SyntaxContextKind.None;        
        if (isExpression) kind |= SyntaxContextKind.Expression;
        if (isPostExpression) kind |= SyntaxContextKind.PostExpression;
        if (isStatement) kind |= SyntaxContextKind.Statement;
        if (isParameterOrVariable) kind |= SyntaxContextKind.ParameterOrVariable;
        if (isPostIfBodyWithoutElse) kind |= SyntaxContextKind.PostIfBodyWithoutElse;
        if (isInLoop) kind |= SyntaxContextKind.InLoop;

        var accessibleSymbols = GetAccessibleSymbols(ast, leftToken, rightToken, isAtEndOfBlock);

        return new(
            position,
            kind,
            ast,
            leftToken,
            rightToken,
            accessibleSymbols);
    }

    /// <summary>
    /// Checks whether a token is inside the context of a loop.
    /// </summary>
    private static bool IsInLoop(Ast ast, ITokenLike token)
    {
        foreach (var node in ast.GetAstNode(token.ParentNode).AncestorsAndSelf())
        {
            // If we find a function declaration or the root then we know we're not inside a loop.
            if (node is FunctionDeclaration or Root) break;
            if (node is LoopExpression) return true;
        }

        return false;
    }

    /// <summary>
    /// Fetches the accessible symbols at the current location.
    /// </summary>
    private  static IBuffer<ISymbol> GetAccessibleSymbols(
        Ast ast,
        ITokenLike? leftToken,
        ITokenLike? rightToken,
        bool isAtEndOfBlock)
    {
        // => |
        if (leftToken?.Kind is TokenKind.EqualsGreaterThan)
        {
            var expressionBody = leftToken.ParentNode switch
            {
                LambdaExpressionSyntax { Expression: var e } => e,
                ExpressionBodySyntax { Expression: var e } => e,
                _ => null
            };

            if (expressionBody is not null)
            {
                var node = ast.GetAstNode(expressionBody);
                return node.Scope.Value.AccessibleAt(LookupLocation.AtNode(node)).Memoize();
            }
        }

        if (isAtEndOfBlock)
        {
            // { | }
            // { a; | }
            var block = ast.GetAstNode(rightToken!.ParentNode) switch
            {
                BlockExpression b => b.Block,
                Root r => r.Block,
                _ => throw new UnreachableException()
            };
            return block.DeclaredScope.Value.AccessibleAt(LookupLocation.AtEnd()).Memoize();
        }
        else
        {
            // { | a; }
            // { a; | b; }
            var statement = GetClosestAncestorStatementLikeNode(ast.GetAstNode(rightToken!.ParentNode))
                ?? throw new InvalidOperationException(
                    "Could not find a statement ancestor AST node.");
            return statement.Scope.Value.AccessibleAt(LookupLocation.AtNode(statement)).Memoize();
        }
    }

    /// <summary>
    /// Fetches the closest ancestor node which is either the root node, a statement, or a trailing expressions.
    /// </summary>
    private static Node? GetClosestAncestorStatementLikeNode(Node child)
    {
        foreach (var node in child.AncestorsAndSelf())
        {
            if (node is
                Root or
                Statement or
                Expression { Parent.Value: LambdaExpression or FunctionDeclaration or Block })
                return node;
        }

        return null;
    }
}
