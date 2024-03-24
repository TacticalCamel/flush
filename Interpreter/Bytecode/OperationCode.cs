namespace Interpreter.Bytecode;

public enum OperationCode: byte {
    PushFromData, // push:1 data-address:4 size:4
    AddInt, // add-int:1 size:1
    AddFloat, // add-float:1 size:1
}