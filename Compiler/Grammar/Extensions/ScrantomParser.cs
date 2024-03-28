// ReSharper disable CheckNamespace

namespace Compiler.Grammar;

using Data;
using Interpreter.Bytecode;

public partial class ScrantonParser {
    public partial class ExpressionContext {
        internal List<Instruction> InstructionsAfterTraversal { get; } = [];
        internal TypeIdentifier? OverrideType { get; set; }
    }

    public partial class ConstantExpressionContext {
        internal ConstantResult? Result { get; set; }
    }
}