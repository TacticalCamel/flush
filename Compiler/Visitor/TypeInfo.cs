namespace Compiler.Visitor;

internal sealed class TypeInfo(string typeName, TypeInfo[] containedTypes){
    public string TypeName{ get; } = typeName;
    public TypeInfo[] ContainedTypes{ get; } = containedTypes;
    public bool IsGeneric => ContainedTypes.Length > 0;

    public override string ToString(){
        return IsGeneric ? $"{TypeName}<{string.Join(',', (IEnumerable<TypeInfo>)ContainedTypes)}>" : TypeName;
    }
}