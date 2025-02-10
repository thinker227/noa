using System.Collections;
using TextMappingUtils;

namespace Noa.Compiler.Syntax;

public readonly struct SyntaxList<T> : IReadOnlyList<T> where T : SyntaxNode
{
    private readonly int position;
    private readonly IReadOnlyList<Green.SyntaxNode> green;

    public SyntaxNode Parent { get; }

    public TextSpan Span => TextSpan.FromLength(position, green.GetWidth());

    public int Count => green.Count;


    public T this[int index] => throw new NotImplementedException();

    internal SyntaxList(int position, SyntaxNode parent, IReadOnlyList<Green.SyntaxNode> green)
    {
        this.position = position;
        this.Parent = parent;
        this.green = green;
    }

    public IEnumerator<T> GetEnumerator() => throw new NotImplementedException();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
