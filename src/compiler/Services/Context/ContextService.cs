using Noa.Compiler.Syntax;

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

        // After a statement
        var isAfterStatement = leftToken
            // let x = 0; |
            // x = 0; |
            // f(); |
            is {
                Kind: TokenKind.Semicolon,
                Parent:
                    LetDeclarationSyntax or
                    AssignmentStatementSyntax or
                    ExpressionStatementSyntax
            }
            // {} |
            // if x {} |
            // if x {} else {} |
            // loop {} |
            or {
                Kind: TokenKind.CloseBrace,
                Parent:
                    BlockExpressionSyntax {
                        Parent: FlowControlStatementSyntax
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
        var isExpresssion = leftToken
            // {|}
            is {
                Kind: TokenKind.OpenBrace,
                Parent: BlockExpressionSyntax { Block.Statements: [] }
            }
            // let x = |
            or {
                Kind: TokenKind.Equals,
                Parent: LetDeclarationSyntax
            }
            // x = |
            //
            // including compound assignment tokens
            or {
                Kind:
                    TokenKind.Equals or
                    TokenKind.PlusEquals or
                    TokenKind.DashEquals or
                    TokenKind.StarEquals or
                    TokenKind.SlashEquals,
                Parent: AssignmentStatementSyntax
            }
            // (|
            or {
                Kind: TokenKind.OpenParen,
                Parent:
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
                Parent: LambdaExpressionSyntax or ExpressionBodySyntax
            }
            // (x, |)
            or {
                Kind: TokenKind.Comma,
                Parent: SeparatedSyntaxList<ExpressionSyntax>
            }
            // if |
            or {
                Kind: TokenKind.If,
                Parent: IfExpressionSyntax
            }
            // return |
            or {
                Kind: TokenKind.Return,
                Parent: ReturnExpressionSyntax
            }
            // break |
            or {
                Kind: TokenKind.Break,
                Parent: BreakExpressionSyntax
            }
            // Unary expressions
            or {
                Kind: TokenKind.Plus or TokenKind.Dash,
                Parent: UnaryExpressionSyntax
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
                Parent: BinaryExpressionSyntax
            };
        
        // Check if the context could be a trailing expression.
        if (isAfterStatement) {
            var statement = leftToken!.GetFirstAncestorOfType<StatementSyntax>()!;
            var statementIndex = statement.GetIndexInParent();
            var block = statement.GetFirstAncestorOfType<BlockSyntax>()!;
            var isTrailingExpression =
                statementIndex == block.Statements.Count - 1 &&
                block.TrailingExpression is null;
            isExpresssion |= isTrailingExpression;
        }

        // Post expression
        var isPostExpression = leftToken
            // true |
            // false |
            is {
                Parent: BoolExpressionSyntax
            }
            // 0 |
            or {
                Parent: NumberExpressionSyntax
            }
            // "uwu" |
            or {
                Kind: TokenKind.EndString,
                Parent: StringExpressionSyntax
            }
            // x |
            or {
                Parent: IdentifierExpressionSyntax
            }
            // () |
            // (x) |
            // (a, b) |
            or {
                Kind: TokenKind.CloseParen,
                Parent:
                    NilExpressionSyntax or
                    ParenthesizedExpressionSyntax or
                    TupleExpressionSyntax
            }
            // {} |
            // if x {} |
            // if x {} else {} |
            // loop {} |
            // 
            // Specifically not when the expression is a flow control statement.
            or {
                Kind: TokenKind.CloseBrace,
                Parent:
                    BlockExpressionSyntax {
                        Parent:
                            IfExpressionSyntax {
                                Parent: not FlowControlStatementSyntax
                            } or
                            ElseClauseSyntax {
                                Parent: IfExpressionSyntax {
                                    Parent: not FlowControlStatementSyntax
                                }
                            } or
                            LoopExpressionSyntax {
                                Parent: not FlowControlStatementSyntax
                            }
                    }
            };

        // Statements
        var isStatement = isAfterStatement || /*isAfterTrailingFlowControlExpression ||*/ leftToken
            is {
                Kind: TokenKind.OpenBrace,
                Parent: BlockExpressionSyntax
            };
        
        // Parameters or variables
        var isParameterOrVariable = leftToken
            is {
                Kind: TokenKind.Let,
                Parent: LetDeclarationSyntax
            }
            or {
                Kind: TokenKind.OpenParen,
                Parent:
                    // Parens will typically auto-complete the closing paren,
                    // so if the user is about to type a lambda expression
                    // then it will start off looking like a nil expression.
                    NilExpressionSyntax or
                    ParameterListSyntax
            }
            or {
                Kind: TokenKind.Comma,
                Parent: SeparatedSyntaxList<ParameterSyntax>
            };
        
        // Post if body without an else clause
        var isPostIfBodyWithoutElse = leftToken
            // 
            is {
                Kind: TokenKind.CloseBrace,
                Parent: BlockExpressionSyntax {
                    Parent: IfExpressionSyntax {
                        Else: null
                    }
                }
            };
        
        var kind = SyntaxContextKind.None;        
        if (isExpresssion) kind |= SyntaxContextKind.Expression;
        if (isPostExpression) kind |= SyntaxContextKind.PostExpression;
        if (isStatement) kind |= SyntaxContextKind.Statement;
        if (isParameterOrVariable) kind |= SyntaxContextKind.ParameterOrVariable;
        if (isPostIfBodyWithoutElse) kind |= SyntaxContextKind.PostIfBodyWithoutElse;

        return new(position, kind, ast, leftToken, rightToken);
    }
}
