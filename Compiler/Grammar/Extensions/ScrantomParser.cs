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

   /*
    public partial class TypeDefinitionContext {
        /// <summary>
        /// The draft of the type definition.
        /// Assigned when the context is visited the first time.
        /// </summary>
        /// <remarks>
        /// We need this because the language allows using types before
        /// there are defined, so they must be loaded in parallel.
        /// </remarks>
        internal TypeDraft? TypeDraft { get; set; }
        
        /// <summary>
        /// Indicates the state of the type loading.
        /// True = successful, false = failed, null = in progress.
        /// </summary>
        internal bool? LoadingState { get; set; }
    }
    
    public partial class FieldDefinitionContext {
        internal FieldDraft? FieldDraft { get; set; }
    }*/
}