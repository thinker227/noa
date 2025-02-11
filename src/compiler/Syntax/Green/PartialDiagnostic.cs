using Noa.Compiler.Diagnostics;

namespace Noa.Compiler.Syntax.Green;

internal readonly record struct PartialDiagnostic(
    Func<Location, IDiagnostic> MakeDiagnostic,
    int Offset,
    int Width);
