using System.Xml;
using System.Xml.Serialization;
using Cocona;
using Noa.CodeGen;
using Scriban;
using Spectre.Console;

var console = AnsiConsole.Create(new() { Out = new AnsiConsoleOutput(Console.Out) });
var error = AnsiConsole.Create(new() { Out = new AnsiConsoleOutput(Console.Error) });

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
        error.MarkupLine($"[red]Input folder [/][aqua]{inputFolder.FullName}[/][red] does not exist[/]");
        return 1;
    }

    var outputFolder = new DirectoryInfo(outputFolderPath);
    if (!outputFolder.Exists) outputFolder.Create();
    
    var xmlFilePath = Path.Combine(inputFolderPath, "nodes.xml");
    var xmlFile = new FileInfo(xmlFilePath);
    if (!xmlFile.Exists)
    {
        error.MarkupLine($"[red]Cannot find input XML file at [/][aqua]{xmlFile.FullName}[/]");
        return 1;
    }

    var templates = Directory
        .EnumerateFiles(inputFolder.FullName, "*.sbncs")
        .Select(templatePath =>
        {
            var templateText = File.ReadAllText(templatePath);
            var template = Template.Parse(templateText, templatePath);
            var name = Path.GetFileNameWithoutExtension(templatePath);

            if (!template.HasErrors) return (name, template) as (string, Template)?;
            
            error.MarkupLine($"[red]Template [/][aqua]{name}[/][red] has errors:[/]");
            foreach (var message in template.Messages) error.MarkupLine($"  [gray]{message}[/]");
            error.MarkupLine($"[aqua]{name}[/][red] will not be rendered.[/]");
            error.WriteLine();
            
            return null;

        })
        .Where(x => x is not null)
        .Select(x => x!.Value)
        .ToList();

    console.MarkupLine($"Parsing input [aqua]{xmlFile.FullName}[/]");
    var rootDto = ReadXml<RootDto>(xmlFile);
    if (rootDto is null) return 1;
    var root = ToModel(rootDto);

    console.MarkupLine($"Rendering [yellow]{templates.Count}[/] templates");
    console.MarkupLine($"Outputting to folder [aqua]{outputFolder.FullName}[/]");
    
    foreach (var (name, template) in templates)
    {
        console.WriteLine();
        
        console.MarkupLine($"  Rendering template [aqua]{name}[/]");
        var text = template.Render(root);

        var fileName = $"{name}.g.cs";
        var outputPath = Path.Combine(outputFolder.FullName, fileName);
        console.MarkupLine($"  Outputting to [aqua]{fileName}[/]");
        File.WriteAllText(outputPath, text);
    }

    return 0;
});

TRoot? ReadXml<TRoot>(FileInfo file) where TRoot : class
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
        error.MarkupLine($"[red]Error deserializing XML: [/][gray]{xmlException.Message}[/]");
        return null;
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
