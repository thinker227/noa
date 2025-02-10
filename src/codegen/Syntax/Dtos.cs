using System.Xml.Serialization;

namespace Noa.CodeGen.Syntax;

[XmlRoot("Root")]
public sealed class RootDto
{
    [XmlAttribute("Name")]
    public required string rootName;

    [XmlElement("Variant", typeof(VariantDto))]
    [XmlElement("Node", typeof(NodeDto))]
    public required List<NodeBaseDto> nodes = [];
}

public abstract class NodeBaseDto
{
    [XmlAttribute("Name")]
    public required string name;

    [XmlAttribute("Parent")]
    public string? parent;
}

public sealed class VariantDto : NodeBaseDto;

public class NodeDto : NodeBaseDto
{
    [XmlElement("Token", typeof(TokenDto))]
    [XmlElement("Value", typeof(ValueDto))]
    [XmlElement("List", typeof(ListDto))]
    [XmlElement("SeparatedList", typeof(SeparatedListDto))]
    public required List<MemberDto> members = [];
}

public abstract class MemberDto
{
    [XmlAttribute("Name")]
    public required string name;

    [XmlAttribute("Optional")]
    public bool isOptional = false;
}

public sealed class TokenDto : MemberDto;

public class ValueDto : MemberDto
{
    [XmlAttribute("Type")]
    public required string type;
}

public sealed class ListDto : ValueDto;

public sealed class SeparatedListDto : ValueDto;
