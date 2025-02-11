using System.Runtime.CompilerServices;

namespace Noa.Compiler.Syntax.Green;

internal abstract class SyntaxNode
{
    private static readonly ConditionalWeakTable<SyntaxNode, List<PartialDiagnostic>> diagnostics = [];

    public IReadOnlyCollection<PartialDiagnostic> Diagnostics =>
        diagnostics.TryGetValue(this, out var ds)
            ? ds
            : [];
    
    public void AddDiagnostic(PartialDiagnostic diagnostic)
    {
        if (!diagnostics.TryGetValue(this, out var ds))
        {
            ds = [];
            diagnostics.Add(this, ds);
        }

        ds.Add(diagnostic);
    }

    public abstract int GetWidth();

    public abstract Syntax.SyntaxNode ToRed(int position, Syntax.SyntaxNode parent);
}
