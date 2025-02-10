using System.Xml;
using System.Xml.Serialization;
using Cocona;
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

    var astToModel = Noa.CodeGen.Ast.DtoExtensions.ToModel;
    Render(
        new(Path.Combine(inputFolder.FullName, "ast")),
        new(Path.Combine(outputFolder.FullName, "Ast")),
        "ast.xml",
        astToModel);

    console.WriteLine();
    
    var syntaxToModel = Noa.CodeGen.Syntax.DtoExtensions.ToModel;
    Render(
        new(Path.Combine(inputFolder.FullName, "syntax")),
        new(Path.Combine(outputFolder.FullName, "Syntax")),
        "syntax.xml",
        syntaxToModel);

    return 0;
});

bool Render<TDto, TModel>(
    DirectoryInfo inputFolder,
    DirectoryInfo outputFolder,
    string xmlName,
    Func<TDto, TModel> toModel)
    where TDto : class
    where TModel : class
{
    var xmlFilePath = Path.Combine(inputFolder.FullName, xmlName);
    var xmlFile = new FileInfo(xmlFilePath);
    if (!xmlFile.Exists)
    {
        error.MarkupLine($"[red]Cannot find input XML file at [/][aqua]{xmlFile.FullName}[/]");
        return false;
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
    var rootDto = ReadXml<TDto>(xmlFile);
    if (rootDto is null) return false;
    var root = toModel(rootDto);

    console.MarkupLine($"Rendering [yellow]{templates.Count}[/] templates");
    console.MarkupLine($"Outputting to folder [aqua]{outputFolder.FullName}[/]");

    Directory.CreateDirectory(outputFolder.FullName);
    
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

    return true;
}

T? ReadXml<T>(FileInfo file) where T : class
{
    try
    {
        using var stream = file.OpenText();
        var reader = XmlReader.Create(stream);
        var serializer = new XmlSerializer(typeof(T));
        return (T)serializer.Deserialize(reader)!;
    }
    catch (InvalidOperationException e) when (e.InnerException is XmlException xmlException)
    {
        error.MarkupLine($"[red]Error deserializing XML: [/][gray]{xmlException.Message}[/]");
        return null;
    }
}
