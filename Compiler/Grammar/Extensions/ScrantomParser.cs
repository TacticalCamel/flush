// ReSharper disable CheckNamespace

namespace Compiler.Grammar;

using Data;

public partial class ScrantonParser {
    public partial class ExpressionContext {
        /// <summary>
        /// The type of the expression.
        /// Assigned when the expression is visited.
        /// </summary>
        internal TypeIdentifier? OriginalType { get; set; }
        
        /// <summary>
        /// The target type of the expression.
        /// Assigned when the parent node is visited.
        /// </summary>
        internal TypeIdentifier? FinalType { get; set; }

        public override string ToString() {
            return $"{GetText()} [{OriginalType?.ToString() ?? "null"}]{(Equals(OriginalType, FinalType) ? "" : $">>[{FinalType?.ToString() ?? "null"}]")}";
        }
    }

    public partial class ConstantExpressionContext {
        /// <summary>
        /// The address of the constant.
        /// Assigned when the expression is visited.
        /// </summary>
        internal MemoryAddress? Address { get; set; }
        
        /// <summary>
        /// An alternative type of the constant.
        /// Assigned when the expression is visited.
        /// </summary>
        internal TypeIdentifier? AlternativeType { get; set; }
    }
}