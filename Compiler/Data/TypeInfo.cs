namespace Compiler.Data;

internal sealed class TypeTemplate(string typeName, TypeTemplate[] genericTypes){
    public string TypeName{ get; } = typeName;
    public TypeTemplate[] GenericTypes{ get; } = genericTypes;
    public bool IsGeneric => GenericTypes.Length > 0;

    public override string ToString(){
        return IsGeneric ? $"{TypeName}<{string.Join(',', (IEnumerable<TypeTemplate>)GenericTypes)}>" : TypeName;
    }
}