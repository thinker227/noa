using System.Xml;
using System.Xml.Serialization;
using Cocona;
using Noa.CodeGen;
using Scriban;

var app = CoconaLiteApp.Create();

app.Run((
    [Argument("input-folder", Description = "The path to the folder containing the input XML file and template files")]
    string inputFolderPath,
    [Argument("output-folder", Description = "The path to the folder to output generated files to")]
    string outputFolderPath) =>
{
    var inputFolder = new DirectoryInfo(inputFolderPath);
    if (!inputFolder.Exists)
    {
        Console.Error.WriteLine("Input folder does not exist");
        return 1;
    }

    var outputFolder = new DirectoryInfo(outputFolderPath);
    if (!outputFolder.Exists) outputFolder.Create();
    
    var xmlFilePath = Path.Combine(inputFolderPath, "nodes.xml");
    var xmlFile = new FileInfo(xmlFilePath);
    if (!xmlFile.Exists)
    {
        Console.Error.WriteLine($"Cannot find input XML file at {xmlFile.FullName}");
        return 1;
    }

    var templates = Directory
        .EnumerateFiles(inputFolder.FullName, "*.sbncs")
        .Select(templatePath =>
        {
            var templateText = File.ReadAllText(templatePath);
            var template = Template.Parse(templateText, templatePath);
            
            if (template.HasErrors) {
                Console.Error.WriteLine($"Template {templatePath} has errors:");
                foreach (var message in template.Messages) Console.Error.WriteLine(message);
                Environment.Exit(1);
            }

            return (name: Path.GetFileNameWithoutExtension(templatePath), template);
        })
        .ToList();

    var rootDto = ReadXml<RootDto>(xmlFile);
    var root = ToModel(rootDto);

    foreach (var (name, template) in templates)
    {
        Console.WriteLine($"Rendering template {name}");
        var text = template.Render(root);

        var outputPath = Path.Combine(outputFolder.FullName, $"{name}.g.cs");
        Console.WriteLine($"Outputting to {outputPath}");
        File.WriteAllText(outputPath, text);
    }

    return 0;
});

static TRoot ReadXml<TRoot>(FileInfo file)
{
    try
    {
        using var stream = file.OpenText();
        var reader = XmlReader.Create(stream);
        var serializer = new XmlSerializer(typeof(TRoot));
        return (TRoot)serializer.Deserialize(reader)!;
    }
    catch (InvalidOperationException e) when (e.InnerException is XmlException xmlException)
    {
        Console.Error.WriteLine($"Error deserializing XML: {xmlException.Message}");
        Environment.Exit(1);
        return default!;
    }
}

static Root ToModel(RootDto rootDto)
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
