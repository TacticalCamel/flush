namespace Compiler.Data;

/// <summary>
/// Represents a value that can be returned after traversing a constant node in the AST.
/// </summary>
/// <param name="address">The address of the constant in the data section.</param>
/// <param name="type">The type of the constant.</param>
/// <param name="alternativeType">An optional second type which the constant can be implicitly converted to.</param>
internal sealed class ConstantResult(int address, TypeIdentifier type, TypeIdentifier? alternativeType = null): ExpressionResult(address, type) {
    /// <summary>
    /// An optional second type which the constant can be implicitly converted to.
    /// </summary>
    public TypeIdentifier? AlternativeType { get; } = alternativeType;
}