// ReSharper disable CheckNamespace

namespace Compiler.Grammar;

using Data;
using Interpreter.Bytecode;

public partial class ScrantonParser {
    public partial class ExpressionContext {
        internal TypeIdentifier? ExpressionType { get; set; }
        internal List<Instruction> EmitOnVisit { get; } = [];
    }

    public partial class ConstantExpressionContext {
        /// <summary>
        /// The result of visiting a constant.
        /// Storing the type and location in memory is important, because we need both
        /// to emit an instruction in the second pass.
        /// </summary>
        internal ConstantResult? Result { get; set; }
    }
}