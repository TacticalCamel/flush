namespace Compiler; 

using Grammar;
using Antlr4.Runtime.Tree;

internal sealed class ScrantonVisitor: ScrantonBaseVisitor<object?> {
    public override object? VisitTerminal(ITerminalNode node) {
        Console.WriteLine($"{node.Symbol.Line}:{node.Symbol.Column}\t{node.GetText()}");
        return base.VisitTerminal(node);
    }
}