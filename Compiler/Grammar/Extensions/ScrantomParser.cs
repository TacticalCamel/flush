// ReSharper disable CheckNamespace

namespace Compiler.Grammar;

using Interpreter.Bytecode;

public partial class ScrantonParser {
    public partial class ExpressionContext {
        internal List<Instruction> InstructionsAfterTraversal { get; } = [];
    }
}