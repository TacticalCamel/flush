namespace Compiler.Data;

internal sealed class TypeInfo(string typeName, TypeInfo[] genericTypes){
    public string TypeName{ get; } = typeName;
    public TypeInfo[] GenericTypes{ get; } = genericTypes;
    public bool IsGeneric => GenericTypes.Length > 0;

    public override string ToString(){
        return IsGeneric ? $"{TypeName}<{string.Join(',', (IEnumerable<TypeInfo>)GenericTypes)}>" : TypeName;
    }
}