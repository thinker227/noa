namespace Noa.CodeGen;

public sealed class Root
{
    public required NodeLike RootNode { get; init; }
    
    public required IReadOnlyCollection<Node> Nodes { get; init; }
}

public class NodeLike
{
    public required string Name { get; init; }
    
    public override string ToString() => Name;
}

public sealed class Node : NodeLike
{
    public NodeLike Parent { get; set; } = null!;
    
    public required bool IsAbstract { get; init; }

    public List<Node> Children { get; } = [];
    
    public List<Member> Members { get; } = [];
}

public abstract class Member
{
    public required string Name { get; init; }
    
    public required string Type { get; init; }
    
    public required bool IsOptional { get; init; }

    public bool IsValue => this is Value;

    public bool IsList => this is List;
}

public sealed class Value : Member
{
    public override string ToString() => $"{Type}{(IsOptional ? "?" : "")} {Name}";
}

public sealed class List : Member
{
    public override string ToString() => $"ImmutableArray<{Type}{(IsOptional ? "?" : "")}> {Name}";
}
