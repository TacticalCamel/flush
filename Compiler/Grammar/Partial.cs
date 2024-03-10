namespace Compiler.Grammar;

using Interpreter.Types;

public partial class ScrantonParser {
    public partial class ExpressionContext {
        public TypeInfo? ExpectedType { get; set; }
    }
    
    public partial class ConstantContext {
        public TypeInfo? ExpectedType { get; set; }
    }
}

