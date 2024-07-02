using Noa.Compiler.Nodes;

namespace Noa.Compiler.ControlFlow.Tests;

public class ControlFlowMarkerTests
{
    [Fact]
    public void Return_Produces_CannotFallThrough_And_CanReturn()
    {
        var text = """
        return;
        let x = 0;
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);
        
        ControlFlowMarker.Mark(ast);

        var x = (LetDeclaration)ast.Root.FindNodeAt(9)!;
        
        x.Reachability.Value.CanFallThrough.ShouldBeFalse();
        x.Reachability.Value.CanReturn.ShouldBeTrue();
    }

    [Fact]
    public void If_MergesReachabilities()
    {
        var text = """
        if (true) {
            return;
        } else {}
        let x = 0;
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        ControlFlowMarker.Mark(ast);

        var x = (LetDeclaration)ast.Root.FindNodeAt(37)!;
        
        x.Reachability.Value.CanFallThrough.ShouldBeTrue();
        x.Reachability.Value.CanReturn.ShouldBeTrue();
    }

    [Fact]
    public void Loop_WithoutBreak_Produces_CannotFallThrough()
    {
        var text = """
        loop {}
        let x = 0;
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        ControlFlowMarker.Mark(ast);

        var x = (LetDeclaration)ast.Root.FindNodeAt(9)!;
        
        x.Reachability.Value.CanFallThrough.ShouldBeFalse();
    }

    [Fact]
    public void Loop_WithContinue_Produces_CannotFallThrough_And_CanContinue_And_CannotFallThrough()
    {
        var text = """
        loop {
            continue;
            let a = 0;
        }
        let b = 0;
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        ControlFlowMarker.Mark(ast);

        var a = (LetDeclaration)ast.Root.FindNodeAt(25)!;
        var b = (LetDeclaration)ast.Root.FindNodeAt(38)!;
        
        a.Reachability.Value.CanFallThrough.ShouldBeFalse();
        a.Reachability.Value.CanContinue.ShouldBeTrue();
        
        b.Reachability.Value.CanFallThrough.ShouldBeFalse();
    }
    
    [Fact]
    public void ConditionalContinue_Produces_CanFallThrough_And_CanContinue()
    {
        var text = """
        loop {
            if (true) {
                continue;
            } else {}
            let x = 0;
        }
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        ControlFlowMarker.Mark(ast);

        var x = (LetDeclaration)ast.Root.FindNodeAt(59)!;
        
        x.Reachability.Value.CanFallThrough.ShouldBeTrue();
        x.Reachability.Value.CanContinue.ShouldBeTrue();
    }
    
    [Fact]
    public void Loop_WithBreak_Produces_CannotFallThrough_And_CanBreak()
    {
        var text = """
        loop {
            break;
            let a = 0;
        }
        let b = 0;
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        ControlFlowMarker.Mark(ast);

        var a = (LetDeclaration)ast.Root.FindNodeAt(22)!;
        var b = (LetDeclaration)ast.Root.FindNodeAt(35)!;
        
        a.Reachability.Value.CanFallThrough.ShouldBeFalse();
        a.Reachability.Value.CanBreak.ShouldBeTrue();
        
        b.Reachability.Value.CanFallThrough.ShouldBeTrue();
    }

    [Fact]
    public void Loop_WithConditionalBreak_Produces_CanFallThrough_And_CanBreak_And_CanFallThrough()
    {
        var text = """
        loop {
            if (true) {
                break;
            } else {}
            let a = 0;
        }
        let b = 0;
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        ControlFlowMarker.Mark(ast);

        var a = (LetDeclaration)ast.Root.FindNodeAt(56)!;
        var b = (LetDeclaration)ast.Root.FindNodeAt(69)!;
        
        a.Reachability.Value.CanFallThrough.ShouldBeTrue();
        a.Reachability.Value.CanBreak.ShouldBeTrue();
        
        b.Reachability.Value.CanFallThrough.ShouldBeTrue();
    }
    
    [Fact]
    public void Return_Shadows_Break()
    {
        var text = """
        loop {
            return;
            break;
            let a = 0;
        }
        let b = 0;
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        ControlFlowMarker.Mark(ast);

        var @break = (BreakExpression)ast.Root.FindNodeAt(23)!;
        var a = (LetDeclaration)ast.Root.FindNodeAt(34)!;
        var b = (LetDeclaration)ast.Root.FindNodeAt(47)!;
        
        @break.Reachability.Value.CanFallThrough.ShouldBeFalse();
        @break.Reachability.Value.CanReturn.ShouldBeTrue();
        
        a.Reachability.Value.CanFallThrough.ShouldBeFalse();
        a.Reachability.Value.CanReturn.ShouldBeTrue();
        a.Reachability.Value.CanBreak.ShouldBeFalse();
        
        b.Reachability.Value.CanFallThrough.ShouldBeFalse();
        b.Reachability.Value.CanReturn.ShouldBeTrue();
    }
    
    [Fact]
    public void Break_Shadows_Return()
    {
        var text = """
        loop {
            break;
            return;
            let a = 0;
        }
        let b = 0;
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        ControlFlowMarker.Mark(ast);

        var @return = (ReturnExpression)ast.Root.FindNodeAt(22)!;
        var a = (LetDeclaration)ast.Root.FindNodeAt(34)!;
        var b = (LetDeclaration)ast.Root.FindNodeAt(47)!;
        
        @return.Reachability.Value.CanFallThrough.ShouldBeFalse();
        @return.Reachability.Value.CanBreak.ShouldBeTrue();
        
        a.Reachability.Value.CanFallThrough.ShouldBeFalse();
        a.Reachability.Value.CanReturn.ShouldBeFalse();
        a.Reachability.Value.CanBreak.ShouldBeTrue();
        
        b.Reachability.Value.CanFallThrough.ShouldBeTrue();
        b.Reachability.Value.CanReturn.ShouldBeFalse();
    }
    
    [Fact]
    public void Continue_Shadows_Break()
    {
        var text = """
        loop {
            continue;
            break;
            let a = 0;
        }
        let b = 0;
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        ControlFlowMarker.Mark(ast);
        
        var @break = (BreakExpression)ast.Root.FindNodeAt(25)!;
        var a = (LetDeclaration)ast.Root.FindNodeAt(36)!;
        var b = (LetDeclaration)ast.Root.FindNodeAt(49)!;
        
        @break.Reachability.Value.CanFallThrough.ShouldBeFalse();
        @break.Reachability.Value.CanContinue.ShouldBeTrue();
        
        a.Reachability.Value.CanFallThrough.ShouldBeFalse();
        a.Reachability.Value.CanBreak.ShouldBeFalse();
        a.Reachability.Value.CanContinue.ShouldBeTrue();
        
        b.Reachability.Value.CanFallThrough.ShouldBeFalse();
    }

    [Fact]
    public void UnreachableReturn_DoesNotReset_CanReturn()
    {
        var text = """
        loop {
            return;
            return;
            let x = 0;
        }
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        ControlFlowMarker.Mark(ast);
        
        var @return = (LetDeclaration)ast.Root.FindNodeAt(35)!;

        @return.Reachability.Value.CanReturn.ShouldBeTrue();
    }
    
    [Fact]
    public void UnreachableBreak_DoesNotReset_CanBreak()
    {
        var text = """
        loop {
            break;
            break;
            let x = 0;
        }
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        ControlFlowMarker.Mark(ast);
        
        var @break = (LetDeclaration)ast.Root.FindNodeAt(33)!;

        @break.Reachability.Value.CanBreak.ShouldBeTrue();
    }
    
    [Fact]
    public void UnreachableContinue_DoesNotReset_CanContinue()
    {
        var text = """
        loop {
            continue;
            continue;
            let x = 0;
        }
        """;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        ControlFlowMarker.Mark(ast);
        
        var @continue = (LetDeclaration)ast.Root.FindNodeAt(39)!;

        @continue.Reachability.Value.CanContinue.ShouldBeTrue();
    }
}
