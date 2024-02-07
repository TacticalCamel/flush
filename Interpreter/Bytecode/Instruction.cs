namespace Interpreter.Bytecode;

[StructLayout(LayoutKind.Sequential)]
public struct Instruction() {
    public OperationCode OperationCode = (OperationCode)0xff;
    public int LeftOperand = 1;
}