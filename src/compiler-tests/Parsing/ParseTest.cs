using System.Runtime.CompilerServices;
using Noa.Compiler.Diagnostics;
using Noa.Compiler.Syntax.Green;

namespace Noa.Compiler.Parsing.Tests;

internal static class ParseTest
{
    private sealed class DiagnosticConverter : WriteOnlyJsonConverter<IPartialDiagnostic>
    {
        public override void Write(VerifyJsonWriter writer, IPartialDiagnostic value)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("Id");
            writer.WriteValue(value.Template.Id.ToString());

            writer.WritePropertyName("Offset");
            writer.WriteValue(value.Offset);

            writer.WritePropertyName("Width");
            writer.WriteValue(value.Width);

            writer.WriteEndObject();
        }
    }

    private static VerifySettings? baseSettings;

    private static VerifySettings MakeBaseSettings()
    {
        var settings = new VerifySettings();

        settings.UseDirectory("results");
        settings.UseStrictJson();
        settings.AddExtraSettings(json =>
        {
            json.Converters.Add(new DiagnosticConverter());
        });
        settings.AlwaysIncludeMembersWithType<int>();
        settings.IgnoreMembers<SyntaxNode>(
            x => x.Children,
            x => x.FirstToken,
            x => x.LastToken);
        
        return settings;
    }

    /// <summary>
    /// Runs a parse test.
    /// </summary>
    /// <typeparam name="T">The type of the root node to parse.</typeparam>
    /// <param name="text">The text to parse.</param>
    /// <param name="parse">The parse function to invoke on the parser.</param>
    /// <param name="sourceFile"></param>
    public static SettingsTask Test<T>(
        string text,
        Func<Parser, T> parse,
        [CallerFilePath] string sourceFile = "")
    {
        var source = new Source(text, "test-input");

        var node = Parser.Parse(parse, source, default);

        var settings = baseSettings ??= MakeBaseSettings();

        return Verify(node, settings, sourceFile);
    }
}
