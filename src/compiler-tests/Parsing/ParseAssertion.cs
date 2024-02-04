using Noa.Compiler.Nodes;

namespace Noa.Compiler.Parsing.Tests;

internal sealed class ParseAssertion : IDisposable
{
    private readonly IEnumerator<Node> nodes;

    public Source Source { get; }

    private ParseAssertion(IEnumerator<Node> nodes, Source source)
    {
        this.nodes = nodes;
        Source = source;
    }

    public static ParseAssertion Create(string text, Func<Parser, Node> parse)
    {
        var source = new Source(text, "test-input");
        var ast = new Ast();
        var tokens = Lexer.Lex(source);

        var parser = new Parser(source, ast, tokens);

        var root = parse(parser);

        var nodes = EnumerateNodes(root);

        return new(nodes.GetEnumerator(), source);
    }

    private static IEnumerable<Node> EnumerateNodes(Node root) =>
        root.Children
            .SelectMany(EnumerateNodes)
            .Prepend(root);

    public T N<T>(Action<T>? assert = null) where T : Node
    {
        nodes.MoveNext().ShouldBeTrue();

        var node = nodes.Current.ShouldBeOfType<T>();
        if (assert is not null) assert(node);
        
        return node;
    }

    public void Dispose()
    {
        nodes.MoveNext().ShouldBeFalse();
        nodes.Dispose();
    }
}
