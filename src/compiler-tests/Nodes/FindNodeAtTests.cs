namespace Noa.Compiler.Nodes.Tests;

public class FindNodeAtTests
{
    private static Node? Run(
        string text,
        int position,
        bool stickToEnd = false)
    {
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        var node = ast.Root.FindNodeAt(position, stickToEnd);
        return node;
    }
    
    private static T Run<T>(
        string text,
        int position,
        bool stickToEnd = false)
        where T : Node
    {
        var node = Run(text, position, stickToEnd);
        return node.ShouldBeOfType<T>();
    }
    
    [Fact]
    public void FindsNode_WithinChildren()
    {
        var node = Run<NumberExpression>(
            """
            let x = 12 + 34 * 56;
            """,
            //           ^
            13);

        node.Value.ShouldBe(34);
    }

    [Fact]
    public void ReturnsNull_ForPositionOutsideNode()
    {
        var node = Run(
            """
            let x = 0;
            """,
            20);

        node.ShouldBeNull();
    }

    [Fact]
    public void ReturnsContainingNode_ForPositionInWhitespace() =>
        Run<LetDeclaration>(
            """
            let x = 0;
            """,
            //   ^
            5);

    [Fact]
    public void ReturnsPreviousNode_ForPositionAfterNode_WithAtEndStickiness() =>
        Run<Identifier>(
            """
            let x = 0;
            """,
            //   ^
            5,
            true);
}
