namespace Compiler.Visitor;

internal sealed class TypeInfo(string typeName, TypeInfo? containedType){
    public string TypeName{ get; } = typeName;
    public TypeInfo? ContainedType{ get; } = containedType;
    public bool IsGeneric => ContainedType is not null;

    public override string ToString(){
        return ContainedType is null ? TypeName : $"{typeName}<{ContainedType}>";
    }
}