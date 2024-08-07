namespace Noa.CodeGen;

public sealed class Root
{
    public required NodeLike RootNode { get; init; }
    
    public required IReadOnlyCollection<Node> Nodes { get; init; }
}

public class NodeLike
{
    public required string Name { get; init; }

    public List<Member> Members { get; } = [];

    public List<Member> NonPrimitiveMembers => Members.Where(x => !x.IsPrimitive).ToList();

    public List<Member> NonInheritedMembers => Members.Where(x => !x.IsInherited).ToList();

    public List<Node> Children { get; } = [];

    public List<Node> TerminalDescendants => Children
        .SelectMany(x => !x.IsAbstract
            ? x.TerminalDescendants
            : [])
        .Concat(Children)
        .ToList();
    
    public override string ToString() => Name;
}

public sealed class Node : NodeLike
{
    public NodeLike Parent { get; set; } = null!;
    
    public required bool IsAbstract { get; init; }
}

public record Member
{
    public required string Name { get; init; }
    
    public required string Type { get; init; }
    
    public required bool IsOptional { get; init; }
    
    public required bool IsPrimitive { get; init; }

    public required bool IsInherited { get; init; }
    
    public required bool IsList { get; init; }

    public override string ToString()
    {
        var type = $"{Type}{(IsOptional ? "?" : "")}";
        return IsList
            ? $"ImmutableArray<{type}> {Name}"
            : $"{type} {Name}";
    }
}
