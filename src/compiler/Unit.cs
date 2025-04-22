namespace Noa.Compiler;

/// <summary>
/// A unit type, a type with only one possible value.
/// </summary>
public readonly record struct Unit
{
    public override string ToString() => "()";
}
