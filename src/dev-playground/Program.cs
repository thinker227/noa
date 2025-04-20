using Noa.Compiler;

var text = """
let s = "hewwo world uwu";
""";

var ast = Ast.Create(new Source(text, "sauce"));

;
