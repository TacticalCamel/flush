namespace Compiler.Visitor;

internal sealed class ScopeTracker{
    private const int ID_UNASSIGNED = -1;
    private List<int> ParentIds{ get; } = new(){ID_UNASSIGNED};
    private int NextId{ get; set; } = 1;

    public int CurrentScope{ get; private set; } = 0;

    public int EnterScope(){
        // set the new scope's parent as the current scope
        ParentIds.Add(CurrentScope);
        
        // change the current id to the new scope's
        CurrentScope = NextId++;
        
        // return current scope id
        return CurrentScope;
    }

    public int ExitScope(){
        // get parent id
        int parentId = ParentIds[CurrentScope];
        
        // if current scope is not the root, exit it
        if(parentId != ID_UNASSIGNED) CurrentScope = parentId;
        
        // return current scope id
        return CurrentScope;
    }
}