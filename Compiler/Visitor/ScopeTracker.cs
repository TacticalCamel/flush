namespace Compiler.Visitor;

internal sealed class ScopeTracker{
    private readonly struct Scope(int parentIndex){
        public int ParentIndex{ get; } = parentIndex;
    }

    private List<Scope> Scopes{ get; } = [new Scope(-1)];

    public int CurrentScopeIndex { get; private set; }

    public int EnterScope(){
        // create a new scope with the current scope as parent
        Scopes.Add(new Scope(CurrentScopeIndex));

        // set the current index to the index of the last element (the scope we just added) 
        CurrentScopeIndex = Scopes.Count - 1;

        // return the current index
        return CurrentScopeIndex;
    }

    public int ExitScope(){
        // get parent index
        int parentIndex = Scopes[CurrentScopeIndex].ParentIndex;

        // if current scope is not root, exit it
        if (parentIndex != -1) CurrentScopeIndex = parentIndex;

        // return current index
        return CurrentScopeIndex;
    }
}