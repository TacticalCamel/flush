namespace Compiler.Visitor;

internal sealed class VariableTracker{
    private sealed class Variable{
        public int Address{ get; }
        public TypeInfo Type{ get; }
        public string Identifier{ get; }
        
    }
}