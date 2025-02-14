using Noa.Compiler.Diagnostics;
using Noa.Compiler.Syntax.Green;
using TokenKind = Noa.Compiler.Syntax.TokenKind;

namespace Noa.Compiler.Parsing.Tests;

internal sealed class ParseAssertion
{
    private readonly IEnumerator<SyntaxNode> nodes;
    private Dictionary<DiagnosticId, IPartialDiagnostic> currentNodeDiagnostics;

    public Source Source { get; }

    private ParseAssertion(IEnumerator<SyntaxNode> nodes, Source source)
    {
        this.nodes = nodes;
        currentNodeDiagnostics = [];
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
    
    private void UpdateCurrentNodeDiagnostics(SyntaxNode node) =>
        currentNodeDiagnostics = node.Diagnostics.ToDictionary(x => x.Template.Id);
    
    private void EnsureDiagnosticsChecked()
    {
        if (nodes.Current is not { } node) return;

        if (currentNodeDiagnostics.Count != 0)
        {
            throw new Exception($"""
            Current node ({node.GetType().Name}) had unchecked diagnostics.
            Diagnostics: {string.Join(", ", currentNodeDiagnostics.Keys)}
            """);
        }
    }
    
    public Token E(Action<Token>? assert = null)
    {
        EnsureDiagnosticsChecked();

        nodes.MoveNext().ShouldBeTrue();

        var token = nodes.Current.ShouldBeOfType<Token>();
        token.Kind.ShouldBe(TokenKind.Error);
        assert?.Invoke(token);

        return token;
    }

    public void D(DiagnosticId id)
    {
        var node = nodes.Current.ShouldNotBeNull();

        if (!currentNodeDiagnostics.Remove(id))
        {
            throw new Exception($"""
            A diagnostic with the ID {id} was expected on the current node ({node.GetType().Name}) but none was found.
            """);
        }
    }
    
    public Token T(TokenKind kind, Action<Token>? assert = null)
    {
        EnsureDiagnosticsChecked();

        nodes.MoveNext().ShouldBeTrue();

        var token = nodes.Current.ShouldBeOfType<Token>();
        token.Kind.ShouldBe(kind);
        assert?.Invoke(token);

        UpdateCurrentNodeDiagnostics(token);

        return token;
    }

    public T N<T>(Action<T>? assert = null) where T : SyntaxNode
    {
        EnsureDiagnosticsChecked();

        nodes.MoveNext().ShouldBeTrue();

        var node = nodes.Current.ShouldBeOfType<T>();
        assert?.Invoke(node);

        UpdateCurrentNodeDiagnostics(node);

        return node;
    }

    public void End()
    {
        EnsureDiagnosticsChecked();

        nodes.MoveNext().ShouldBeFalse();
    }
}
