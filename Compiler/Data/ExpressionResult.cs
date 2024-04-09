namespace Compiler.Data;

/// <summary>
/// Represents a value that can be returned after traversing an expression node in the AST.
/// </summary>
/// <param name="address">The address of the result.</param>
/// <param name="type">The type of the result.</param>
internal class ExpressionResult(MemoryAddress address, TypeIdentifier type) {
    /// <summary>
    /// The address of the result on the stack.
    /// </summary>
    public MemoryAddress Address { get; } = address;

    /// <summary>
    /// The type of the result.
    /// </summary>
    public TypeIdentifier Type { get; } = type;

    public override string ToString() {
        return $"{Type} at {Address}";
    }
}