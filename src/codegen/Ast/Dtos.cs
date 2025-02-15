using System.Xml.Serialization;

namespace Noa.CodeGen.Ast;

[XmlRoot("Root")]
public sealed class RootDto
{
    [XmlAttribute("Name")]
    public required string rootName;

    [XmlElement("Node", typeof(NodeDto))]
    [XmlElement("Variant", typeof(VariantDto))]
    public required List<NodeBaseDto> nodes = [];
}

public abstract class NodeBaseDto
{
    [XmlAttribute("Name")]
    public required string name;

    [XmlAttribute("Parent")]
    public string? parent;

    [XmlAttribute("Concrete")]
    public required string concrete;
}

public sealed class VariantDto : NodeBaseDto;

public class NodeDto : NodeBaseDto
{
    [XmlElement("Value", typeof(ValueDto))]
    [XmlElement("List", typeof(ListDto))]
    [XmlElement("Inherited", typeof(InheritedDto))]
    public required List<MemberDto> members = [];
}

public abstract class MemberDto
{
    [XmlAttribute("Name")]
    public required string name;
}

public class ValueDto : MemberDto
{
    [XmlAttribute("Type")]
    public required string type;

    [XmlAttribute("Optional")]
    public bool isOptional = false;

    [XmlAttribute("Primitive")]
    public bool isPrimitive = false;
}

public sealed class ListDto : ValueDto;

public sealed class InheritedDto : MemberDto;
