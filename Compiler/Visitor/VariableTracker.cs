namespace Compiler.Visitor;

internal sealed class VariableTracker{
    private sealed class Variable{
        public int ScopeId{ get; }
        public TypeInfo Type{ get; }
        public string Identifier{ get; }
    }

    private List<int> Separators{ get; } = [];
    private List<Variable> Variables{ get; }

    public void EnterScope(){
        
    }
}