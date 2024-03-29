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
    private InstructionHandler Instructions { get; } = [];

    /// <summary>
    /// Visit a syntax tree and transform it to an executable program.
    /// </summary>
    /// <param name="programContext">The root of the syntax tree.</param>
    /// <returns>The compiled program.</returns>
    public Script Build(ProgramContext programContext) {
        // lexer or parser error before traversing tree
        CancelIfHasErrors();

        // create a preprocessor
        Preprocessor preprocessor = new(IssueHandler, TypeHandler, DataHandler);

        // traverse AST: 1st pass
        preprocessor.VisitProgram(programContext);

        // check for errors in 1st pass
        CancelIfHasErrors();

        // traverse AST: 2nd pass
        VisitProgram(programContext);

        // check for errors in 2nd pass
        CancelIfHasErrors();


        byte[] data = DataHandler.ToBytes();
        Instruction[] instructions = Instructions.ToArray();

        return new Script(data, instructions);
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
            OP_PLUS => isPrimitiveType ? PrimitiveAddition(left.Type, right.Type) : null,
            OP_MINUS => null,
            OP_MULTIPLY => null,
            OP_DIVIDE => null,
            OP_MODULUS => null,
            OP_SHIFT_LEFT => null,
            OP_SHIFT_RIGHT => null,
            OP_EQ => null,
            OP_NOT_EQ => null,
            OP_LESS => null,
            OP_GREATER => null,
            OP_LESS_EQ => null,
            OP_GREATER_EQ => null,
            OP_AND => null,
            OP_OR => null,
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

        return result;
    }

    private ExpressionResult? ResolveUnaryExpression(ExpressionContext context, ExpressionContext innerContext, int operatorType) {
        // resolve the expression
        ExpressionResult? expression = VisitExpression(innerContext);

        // return if an error occured
        if (expression is null) {
            return null;
        }

        // true if the expression type is a primitive type or null
        // in this case we can use a simple instruction instead of a function call
        bool isPrimitiveType = TypeHandler.Casts.IsPrimitiveType(expression.Type);

        // calculate results
        ExpressionResult? result = operatorType switch {
            OP_PLUS => null,
            OP_MINUS => null,
            OP_NOT => null,
            OP_INCREMENT => null,
            OP_DECREMENT => null,
            _ => throw new ArgumentException($"Method cannot handle the provided operator type {operatorType}")
        };

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
        bool success = Instructions.Cast(context.OriginalType, context.FinalType, cast);

        // stop if failed
        // TODO return value should be void, but not every cast type is handled yet
        if (!success) {
            IssueHandler.Add(Issue.InvalidCast(context, context.OriginalType, context.FinalType));
        }
        
        return success;
    }
}