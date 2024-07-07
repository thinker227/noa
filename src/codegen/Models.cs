namespace Noa.CodeGen;

public sealed class Root
{
    public required NodeLike RootNode { get; init; }
    
    public required IReadOnlyCollection<Node> Nodes { get; init; }
}

public class NodeLike
{
    public required string Name { get; init; }

    public virtual IEnumerable<Member> Members { get; } = [];

    public List<Node> Children { get; } = [];
    
    public override string ToString() => Name;
}

public sealed class Node(List<Member> members) : NodeLike
{
    public NodeLike Parent { get; set; } = null!;
    
    public required bool IsAbstract { get; init; }

    public override IEnumerable<Member> Members => members;
}

public sealed class Member
{
    public required string Name { get; init; }
    
    public required string Type { get; init; }
    
    public required bool IsOptional { get; init; }
    
    public required bool IsPrimitive { get; init; }

    public required bool IsList { get; init; }

    public override string ToString()
    {
        var type = $"{Type}{(IsOptional ? "?" : "")}";
        return IsList
            ? $"ImmutableArray<{type}> {Name}"
            : $"{type} {Name}";
    }
}
