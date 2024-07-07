using System.Xml.Serialization;

namespace Noa.CodeGen;

[XmlRoot("Root")]
public sealed class RootDto
{
    [XmlAttribute("Name")]
    public required string rootName;

    [XmlElement("Node", typeof(NodeDto))]
    public required List<NodeDto> nodes = [];
}

public class NodeDto
{
    [XmlAttribute("Name")]
    public required string name;

    [XmlAttribute("Parent")]
    public string? parent;

    [XmlAttribute("Abstract")]
    public bool isAbstract = false;

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
