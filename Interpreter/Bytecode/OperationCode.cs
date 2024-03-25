// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace Interpreter.Bytecode;

public enum OperationCode: byte {
    pshd, // push:1 data-address:4 size:4
    addi, // add-int:1 size:1
    addf, // add-float:1 size:1
    extd // extend:1 size:1
}