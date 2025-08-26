using System.Text.Json;

namespace Noa.Cli;

public sealed record Config
{
    private static JsonSerializerOptions jsonOptions = new()
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public const string DirectoryName = ".noa";
    public const string FileName = "config.json";

    public string? RuntimePath { get; init; }

    public static Config? FromJson(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<Config>(json, jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static Config? TryGetEnvironmentConfig()
    {
        if (FindConfiguration() is not {} file) return null;

        var text = File.ReadAllText(file.FullName);
        return FromJson(text);
    }

    private static FileInfo? FindConfiguration()
    {
        for (var current = new DirectoryInfo(Environment.CurrentDirectory); current is not null; current = current.Parent)
        {
            var configPath = Path.Combine(current.FullName, DirectoryName);
            if (!Directory.Exists(configPath)) continue;

            var runtimePath = Path.Combine(configPath, FileName);
            if (File.Exists(runtimePath)) return new FileInfo(runtimePath);
        }

        return null;
    }
}
