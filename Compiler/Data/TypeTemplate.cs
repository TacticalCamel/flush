namespace Compiler.Data;

internal sealed class TypeTemplate(string typeName, TypeTemplate[] genericParameters){
    public string TypeName{ get; } = typeName;
    public TypeTemplate[] GenericParameters{ get; } = genericParameters;
    
    public bool IsGeneric => GenericParameters.Length > 0;

    public override string ToString(){
        return IsGeneric ? $"{TypeName}<{string.Join(',', (IEnumerable<TypeTemplate>)GenericParameters)}>" : TypeName;
    }
}