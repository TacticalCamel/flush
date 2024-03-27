namespace Compiler.Data;

using Interpreter.Bytecode;

internal sealed class ExpressionResult(MemoryAddress address, TypeIdentifier type, TypeIdentifier? implicitType = null) {
    public MemoryAddress Address { get; } = address;
    public TypeIdentifier Type { get; } = type;
    public TypeIdentifier? ImplicitType { get; } = implicitType;
    public List<Instruction> InstructionsAfter { get; } = [];

    public override string ToString() {
        return $"{Type} at {Address}";
    }
}