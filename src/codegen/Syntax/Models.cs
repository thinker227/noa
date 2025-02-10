using System.Diagnostics;

namespace Noa.CodeGen.Syntax;

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

    public required bool IsInherited { get; init; }

    public required bool IsPrimitive { get; init; }
    
    public bool IsList => ListKind is not ListKind.None;

    public required ListKind ListKind { get; init; }

    public override string ToString()
    {
        var type = $"{Type}{(IsOptional ? "?" : "")}";
        return ListKind switch
        {
            ListKind.None => $"{type} {Name}",
            ListKind.Normal => $"ImmutableArray<{type}> {Name}",
            ListKind.Separated => $"SeparatedSyntaxList<{type}> {Name}",
            _ => throw new UnreachableException()
        };
    }
}

public enum ListKind
{
    None,
    Normal,
    Separated,
}

public static class DtoExtensions
{
    public static Root ToModel(RootDto rootDto)
    {
        var rootNode = new NodeLike() { Name = rootDto.rootName };
        var nodes = rootDto.nodes
            .ToDictionary(
                x => x.name,
                x => (dto: x, node: new Node() { Name = x.name, IsAbstract = x is VariantDto }));
        
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
                var type = memberDto is ValueDto x
                    ? x.type
                    : "Token";
                
                var member = new Member()
                {
                    Name = memberDto.name,
                    Type = type,
                    IsOptional = memberDto.isOptional,
                    IsInherited = false,
                    IsPrimitive = memberDto is TokenDto,
                    ListKind = memberDto switch
                    {
                        ListDto => ListKind.Normal,
                        SeparatedListDto => ListKind.Separated,
                        _ => ListKind.None
                    }
                };
                
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
    }
}
