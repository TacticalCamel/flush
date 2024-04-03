namespace Compiler.Types;

using static Grammar.ScrantonParser;

internal sealed class TypeDraft: ILoadedType {
    public required TypeDefinitionContext Context { get; init; }
    
    public required Modifier Modifiers { get; init; }
    public required bool IsReference { get; init; }
    public required string Name { get; init; }
    public ushort? Size { get; set; }
}