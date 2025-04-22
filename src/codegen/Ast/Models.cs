namespace Noa.CodeGen.Ast;

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
    public required string Concrete { get; init; }

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

public static class DtoExtensions
{
    public static Root ToModel(RootDto rootDto)
    {
        var rootNode = new NodeLike() { Name = rootDto.rootName };
        var nodes = rootDto.nodes
            .ToDictionary(
                x => x.name,
                x => (dto: x, node: new Node()
                    {
                        Name = x.name,
                        Concrete = x.concrete,
                        IsAbstract = x is VariantDto
                    }));
        
        foreach (var (nodeDto, node) in nodes.Values)
        {
            var parent = nodeDto.parent is not null
                ? nodes[nodeDto.parent].node
                : rootNode;
            
            node.Parent = parent;
            parent.Children.Add(node);
        }

        var ordered = rootNode.Children.SelectMany(GetOrderedNodes);

        foreach (var node in ordered)
        {
            var nodeDto = nodes[node.Name].dto;

            if (nodeDto is not NodeDto { members: var dtoMembers }) continue;
            
            foreach (var memberDto in dtoMembers)
            {
                var member = memberDto is ValueDto x
                    ? new Member()
                    {
                        Name = x.name,
                        Type = x.type,
                        IsOptional = x.isOptional,
                        IsPrimitive = x.isPrimitive,
                        IsInherited = false,
                        IsList = x is ListDto
                    }
                    : FindMember(memberDto.name, node) with { IsInherited = true };
                
                node.Members.Add(member);
            }
        }

        return new Root()
        {
            RootNode = rootNode,
            Nodes = nodes.Values.Select(x => x.node).ToList()
        };

        static IEnumerable<Node> GetOrderedNodes(Node node) =>
            node.Children.SelectMany(GetOrderedNodes).Prepend(node);

        static Member FindMember(string name, Node node)
        {
            if (node.Members.FirstOrDefault(x => x.Name == name) is { } member) return member;
            if (node.Parent is Node parent) return FindMember(name, parent);
            throw new InvalidOperationException($"No member with name {name}");
        }
    }
}
