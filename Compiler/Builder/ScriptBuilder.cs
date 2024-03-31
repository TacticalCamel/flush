namespace Compiler.Builder;

using static Grammar.ScrantonParser;
using Handlers;
using Data;
using Grammar;
using Analysis;
using Interpreter.Serialization;
using Interpreter.Bytecode;
using Microsoft.Extensions.Logging;

/// <summary>
/// Implements the second pass traversal of the syntax tree with the visitor pattern. 
/// </summary>
/// <param name="options">The setting to use during compilation.</param>
/// <param name="logger">The logger to use.</param>
internal sealed partial class ScriptBuilder(CompilerOptions options, ILogger logger) : ScrantonBaseVisitor<object?> {
    /// <summary>
    /// The setting to use during compilation.
    /// This is the only form of state the builder is initialized with.
    /// </summary>
    private CompilerOptions Options { get; } = options;

    /// <summary>
    /// The logger to use.
    /// </summary>
    private ILogger Logger { get; } = logger;

    /// <summary>
    /// This issue handler to manage compilation issues. 
    /// </summary>
    private IssueHandler IssueHandler { get; } = [];

    /// <summary>
    /// The type handler to manage loaded types.
    /// </summary>
    private TypeHandler TypeHandler { get; } = new();

    /// <summary>
    /// The data handler to keep track of constants.
    /// </summary>
    private DataHandler DataHandler { get; } = new();

    /// <summary>
    /// The collect of instructions.
    /// </summary>
    private InstructionHandler InstructionHandler { get; } = [];

    /// <summary>
    /// Visit a syntax tree and transform it to an executable program.
    /// </summary>
    /// <param name="programContext">The root of the syntax tree.</param>
    /// <returns>The compiled program.</returns>
    public Script Build(ProgramContext programContext) {
        // lexer or parser error
        CancelIfHasErrors();

        // create the preprocessor
        Preprocessor preprocessor = new(IssueHandler, TypeHandler, DataHandler);

        // traverse syntax tree: 1st pass
        preprocessor.VisitProgram(programContext);

        // check for errors in 1st pass
        CancelIfHasErrors();

        // traverse syntax tree: 2nd pass
        VisitProgram(programContext);

        // check for errors in 2nd pass
        CancelIfHasErrors();

        // assemble program 
        byte[] data = DataHandler.ToBytes();
        Instruction[] instructions = InstructionHandler.ToArray();
        Script script = new(data, instructions);

        // warn if the program is empty
        if (instructions.Length == 0) {
            IssueHandler.Add(Issue.ProgramEmpty(programContext));
        }

        return script;
    }

    private ExpressionResult? ResolveBinaryExpression(ExpressionContext context, ExpressionContext leftContext, ExpressionContext rightContext, int operatorType) {
        // resolve left side
        ExpressionResult? left = VisitExpression(leftContext);

        // return if there was an error
        if (left is null) {
            return null;
        }

        // resolve right side
        ExpressionResult? right = VisitExpression(rightContext);

        // return if there was an error
        if (right is null) {
            return null;
        }

        bool isLeftPrimitive = TypeHandler.Casts.IsPrimitiveType(left.Type);
        bool isRightPrimitive = TypeHandler.Casts.IsPrimitiveType(right.Type);

        // true if both expression types are primitive types or null
        // in this case we can use a simple instruction instead of a function call
        bool isPrimitiveType = isLeftPrimitive && isRightPrimitive;

        // calculate results
        ExpressionResult? result = operatorType switch {
            OP_PLUS => isPrimitiveType ? BinaryNumberOperation(left.Type, right.Type, OperationCode.addi, OperationCode.addf) : null,
            OP_MINUS => isPrimitiveType ? BinaryNumberOperation(left.Type, right.Type, OperationCode.subi, OperationCode.subf) : null,
            OP_MULTIPLY => isPrimitiveType ? BinaryNumberOperation(left.Type, right.Type, OperationCode.muli, OperationCode.mulf) : null,
            OP_DIVIDE => isPrimitiveType ? BinaryNumberOperation(left.Type, right.Type, OperationCode.divi, OperationCode.divf) : null,
            OP_MODULUS => isPrimitiveType ? BinaryNumberOperation(left.Type, right.Type, OperationCode.modi, OperationCode.modf) : null,
            OP_SHIFT_LEFT => isPrimitiveType ? BinaryShiftOperation(left.Type, right.Type, OperationCode.shfl) : null,
            OP_SHIFT_RIGHT => isPrimitiveType ? BinaryShiftOperation(left.Type, right.Type, OperationCode.shfr) : null,
            OP_EQ => isPrimitiveType ? BinaryComparisonOperation(left.Type, right.Type, OperationCode.eq) : null,
            OP_NOT_EQ => isPrimitiveType ? BinaryComparisonOperation(left.Type, right.Type, OperationCode.neq) : null,
            OP_LESS => null,
            OP_GREATER => null,
            OP_LESS_EQ => null,
            OP_GREATER_EQ => null,
            OP_AND => isPrimitiveType ? BinaryBitwiseOperation(left.Type, right.Type, OperationCode.and) : null,
            OP_OR => isPrimitiveType ? BinaryBitwiseOperation(left.Type, right.Type, OperationCode.or) : null,
            OP_XOR => isPrimitiveType ? BinaryBitwiseOperation(left.Type, right.Type, OperationCode.xor) : null,
            OP_ASSIGN => null,
            OP_MULTIPLY_ASSIGN => null,
            OP_DIVIDE_ASSIGN => null,
            OP_MODULUS_ASSIGN => null,
            OP_PLUS_ASSIGN => null,
            OP_MINUS_ASSIGN => null,
            OP_SHIFT_LEFT_ASSIGN => null,
            OP_SHIFT_RIGHT_ASSIGN => null,
            OP_AND_ASSIGN => null,
            OP_OR_ASSIGN => null,
            _ => throw new ArgumentException($"Method cannot handle the provided operator type {operatorType}")
        };

        // operation invalid
        if (result is null) {
            IssueHandler.Add(Issue.InvalidBinaryOperation(context, left.Type, right.Type, DefaultVocabulary.GetDisplayName(operatorType)));
            return null;
        }

        // return value can be discarded
        if (context.FinalType is null) {
            return result;
        }

        // need the return value, cast it to the target type
        return CastExpression(context) ? new ExpressionResult(result.Address, context.FinalType) : null;
    }

    private ExpressionResult? ResolveUnaryExpression(ExpressionContext context, ExpressionContext innerContext, int operatorType) {
        // resolve the expression
        ExpressionResult? inner = VisitExpression(innerContext);

        // return if an error occured
        if (inner is null) {
            return null;
        }

        // true if the expression type is a primitive type or null
        // in this case we can use a simple instruction instead of a function call
        bool isPrimitiveType = TypeHandler.Casts.IsPrimitiveType(inner.Type);

        // calculate results
        ExpressionResult? result = operatorType switch {
            OP_NOT => isPrimitiveType ? UnaryBoolOperation(inner.Type, OperationCode.negb) : null,
            OP_PLUS => isPrimitiveType ? inner : null,
            OP_MINUS => isPrimitiveType ? UnaryNumberOperation(inner.Type, OperationCode.sswi, OperationCode.sswf) : null,
            OP_INCREMENT => isPrimitiveType ? UnaryNumberOperation(inner.Type, OperationCode.inci, OperationCode.incf) : null,
            OP_DECREMENT => isPrimitiveType ? UnaryNumberOperation(inner.Type, OperationCode.deci, OperationCode.decf) : null,
            _ => throw new ArgumentException($"Method cannot handle the provided operator type {operatorType}")
        };
        
        // operation invalid
        if (result is null) {
            IssueHandler.Add(Issue.InvalidUnaryOperation(context, inner.Type, DefaultVocabulary.GetDisplayName(operatorType)));
            return null;
        }

        return result;
    }

    private bool CastExpression(ExpressionContext context) {
        if (context.OriginalType is null || context.FinalType is null) {
            return false;
        }

        // TODO implement non-primitive casts

        // get the type of cast required
        PrimitiveCast cast = TypeHandler.Casts.GetPrimitiveCast(context.OriginalType, context.FinalType);

        // emit a cast instruction
        bool success = InstructionHandler.Cast(context.OriginalType.Size, context.FinalType.Size, cast);

        // stop if failed
        if (!success) {
            IssueHandler.Add(Issue.InvalidCast(context, context.OriginalType, context.FinalType));
        }

        return success;
    }

    #region Operator methods

    private ExpressionResult? UnaryNumberOperation(TypeIdentifier type, OperationCode integerCode, OperationCode floatCode) {
        // must be an integer or float
        if (TypeHandler.Casts.IsIntegerType(type)) {
            MemoryAddress address = InstructionHandler.PrimitiveUnaryOperation(type.Size, integerCode);
            return new ExpressionResult(address, type);
        }

        if (TypeHandler.Casts.IsFloatType(type)) {
            MemoryAddress address = InstructionHandler.PrimitiveUnaryOperation(type.Size, floatCode);
            return new ExpressionResult(address, type);
        }

        return null;
    }
    
    private ExpressionResult? UnaryBoolOperation(TypeIdentifier type, OperationCode code) {
        // must be bool
        if (type == TypeHandler.CoreTypes.Bool) {
            MemoryAddress address = InstructionHandler.PrimitiveUnaryOperation(type.Size, code);
            return new ExpressionResult(address, type);
        }

        return null;
    }

    private ExpressionResult? BinaryNumberOperation(TypeIdentifier leftType, TypeIdentifier rightType, OperationCode integerCode, OperationCode floatCode) {
        // must operate on the same types
        if (leftType != rightType) {
            return null;
        }

        // must be an integer or float
        if (TypeHandler.Casts.IsIntegerType(leftType)) {
            MemoryAddress address = InstructionHandler.PrimitiveBinaryOperation(leftType.Size, integerCode);
            return new ExpressionResult(address, leftType);
        }

        if (TypeHandler.Casts.IsFloatType(leftType)) {
            MemoryAddress address = InstructionHandler.PrimitiveBinaryOperation(leftType.Size, floatCode);
            return new ExpressionResult(address, leftType);
        }

        return null;
    }

    private ExpressionResult? BinaryBitwiseOperation(TypeIdentifier leftType, TypeIdentifier rightType, OperationCode code) {
        // must operate on the same types
        if (leftType != rightType) {
            return null;
        }

        // must be a bool or integer
        if (leftType != TypeHandler.CoreTypes.Bool && !TypeHandler.Casts.IsIntegerType(leftType)) {
            return null;
        }

        MemoryAddress address = InstructionHandler.PrimitiveBinaryOperation(leftType.Size, code);

        return new ExpressionResult(address, leftType);
    }

    private ExpressionResult? BinaryComparisonOperation(TypeIdentifier leftType, TypeIdentifier rightType, OperationCode code) {
        // must operate on the same types
        if (leftType != rightType) {
            return null;
        }

        // can be any type
        MemoryAddress address = InstructionHandler.PrimitiveComparisonOperation(leftType.Size, code);
        return new ExpressionResult(address, TypeHandler.CoreTypes.Bool);
    }
    
    private ExpressionResult? BinaryShiftOperation(TypeIdentifier leftType, TypeIdentifier rightType, OperationCode code) {
        // left side must be an integer type
        if (!TypeHandler.Casts.IsIntegerType(leftType)) {
            return null;
        }
        
        // right side must be i32
        if (rightType != TypeHandler.CoreTypes.I32) {
            return null;
        }

        // can be any type
        MemoryAddress address = InstructionHandler.PrimitiveComparisonOperation(leftType.Size, code);
        return new ExpressionResult(address, TypeHandler.CoreTypes.Bool);
    }
    
    

    #endregion
}