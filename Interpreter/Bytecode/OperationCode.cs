namespace Interpreter.Bytecode;

public enum OperationCode: byte {
    Exit,
    Return,
    Call,
    Push,
    Pop,
    Jump,
    ConditionalJump
}