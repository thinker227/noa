using Draco.Lsp.Model;
using Noa.Compiler;

namespace Noa.LangServer;

public readonly record struct NoaDocument(Ast Ast, LineMap LineMap, DocumentUri Uri);
