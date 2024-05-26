// ReSharper disable CheckNamespace

namespace Compiler.Grammar;

using Types;
using Data;

public partial class FlushParser {
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
        
        /// <summary>
        /// Whether to allow explicit casting of the expression.
        /// Assigned when the parent node is visited.
        /// </summary>
        internal bool AllowExplicitCast { get; set; }

        public override string ToString() {
            return $"{GetText()} [{OriginalType?.ToString() ?? "null"}]{(Equals(OriginalType, FinalType) ? "" : $">>[{FinalType?.ToString() ?? "null"}]")}";
        }
    }

    public partial class ConstantExpressionContext {
        /// <summary>
        /// The address of the constant in the data section.
        /// Assigned when the expression is visited.
        /// </summary>
        internal int? Address { get; set; }

        /// <summary>
        /// An alternative type of the constant.
        /// Assigned when the expression is visited.
        /// </summary>
        internal TypeIdentifier? AlternativeType { get; set; }
    }

    public partial class IdentifierExpressionContext {
        /// <summary>
        /// The address of the variable.
        /// Assigned when the expression is visited.
        /// </summary>
        internal int? Address { get; set; }
    }

    public partial class TypeDefinitionContext {
        /// <summary>
        /// The draft associated to the type definition.
        /// Assumed to not be null after the type definition is visited.
        /// </summary>
        internal TypeDraft TypeDraft { get; set; }
    }
}