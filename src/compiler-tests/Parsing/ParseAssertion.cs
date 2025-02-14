using Noa.Compiler.Diagnostics;
using Noa.Compiler.Syntax.Green;
using TokenKind = Noa.Compiler.Syntax.TokenKind;

namespace Noa.Compiler.Parsing.Tests;

internal sealed class ParseAssertion
{
    private readonly IEnumerator<SyntaxNode> nodes;

    public Source Source { get; }

    private ParseAssertion(IEnumerator<SyntaxNode> nodes, Source source)
    {
        this.nodes = nodes;
        Source = source;
    }

    public static ParseAssertion Create(string text, Func<Parser, SyntaxNode> parse)
    {
        var source = new Source(text, "test-input");
        var tokens = Lexer.Lex(source, default);

        var parser = new Parser(source, tokens, default);

        var root = parse(parser);

        var nodes = EnumerateNodes(root);

        return new(nodes.GetEnumerator(), source);
    }

    private static IEnumerable<SyntaxNode> EnumerateNodes(SyntaxNode root) =>
        root.Children
            .SelectMany(EnumerateNodes)
            .Prepend(root);
    
    public Token E(Action<Token>? assert = null)
    {
        nodes.MoveNext().ShouldBeTrue();

        var token = nodes.Current.ShouldBeOfType<Token>();
        token.Kind.ShouldBe(TokenKind.Error);
        assert?.Invoke(token);

        return token;
    }

    public void D(params IEnumerable<DiagnosticId> ids)
    {
        var node = nodes.Current.ShouldNotBeNull();
        node.Diagnostics.Select(x => x.Template.Id).ShouldBe(ids, ignoreOrder: true);
    }
    
    public Token T(TokenKind kind, Action<Token>? assert = null)
    {
        nodes.MoveNext().ShouldBeTrue();

        var token = nodes.Current.ShouldBeOfType<Token>();
        token.Kind.ShouldBe(kind);
        assert?.Invoke(token);

        return token;
    }

    public T N<T>(Action<T>? assert = null) where T : SyntaxNode
    {
        nodes.MoveNext().ShouldBeTrue();

        var node = nodes.Current.ShouldBeOfType<T>();
        assert?.Invoke(node);

        return node;
    }

    public void End() =>
        nodes.MoveNext().ShouldBeFalse();
}
