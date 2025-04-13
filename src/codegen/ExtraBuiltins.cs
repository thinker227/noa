namespace Noa.CodeGen;

public static class ExtraBuiltins
{
    public static string CamelCase(string s) =>
        $"{char.ToLower(s[0])}{s[1..]}";
}
