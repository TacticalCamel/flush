namespace Compiler.Builder;

using Data;
using Analysis;
using Interpreter.Bytecode;
using static Grammar.ScrantonParser;

// ScriptBuilder.Expressions: methods related to visiting expressions
internal sealed partial class ScriptBuilder {
    #region Visit methods

    /// <summary>
    /// Visit an expression. Calls the visit method with the actual expression type.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The address and type of the expression.</returns>
    public override ExpressionResult? VisitExpression(ExpressionContext context) {
        // visit expression subtype
        return (ExpressionResult?)Visit(context);
    }

    /// <summary>
    /// Visit the null keyword.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>A null reference.</returns>
    public override ExpressionResult? VisitNullExpression(NullExpressionContext context) {
        // preprocessor mode on
        if (IsPreprocessorMode) {
            PreprocessNull(context);
            return null;
        }

        // return a null reference
        return new ExpressionResult(MemoryAddress.Null, TypeHandler.CoreTypes.Void);
    }

    /// <summary>
    /// Visit a constant expression.
    /// Push the value to the stack and convert it to the desired type.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The address and type of the expression.</returns>
    public override ExpressionResult? VisitConstantExpression(ConstantExpressionContext context) {
        // preprocessor mode on
        if (IsPreprocessorMode) {
            PreprocessConstant(context);
            return null;
        }

        // error during preprocessing
        if (context.Address is null || context.OriginalType is null || context.FinalType is null) {
            return null;
        }

        // push the constant to the stack
        MemoryAddress address = CodeHandler.PushBytesFromData(context.Address.Value, context.OriginalType.Size);

        // cast the constant
        bool success = CastExpression(context);

        // return a valid result if successful
        return success ? new ExpressionResult(address, context.FinalType) : null;
    }

    /// <summary>
    /// Visit an identifier expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The address and type of the expression.</returns>
    public override ExpressionResult? VisitIdentifierExpression(IdentifierExpressionContext context) {
        // preprocessor mode on
        if (IsPreprocessorMode) {
            PreprocessIdentifier(context);
            return null;
        }

        // error during preprocessing
        if (context.Address is null || context.OriginalType is null || context.FinalType is null) {
            return null;
        }

        // TODO not yet supported
        if (context.Address.Value.Location != MemoryLocation.Stack) {
            throw new NotImplementedException("Variable not on stack");
        }

        // copy to the top of the stack
        CodeHandler.PushBytesFromStack((int)context.Address.Value.Value, context.OriginalType.Size);

        // cast expression
        bool success = CastExpression(context);

        // return a valid result if successful
        return success ? new ExpressionResult(context.Address.Value, context.FinalType) : null;
    }

    // TODO implement
    public override ExpressionResult? VisitMemberAccessExpression(MemberAccessExpressionContext context) {
        // preprocessor mode on
        if (IsPreprocessorMode) {
            PreprocessMemberAccess(context);
            return null;
        }

        throw new NotImplementedException();
    }

    /// <summary>
    /// Visit a cast expression.
    /// Convert the value to the desired type.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The address and type of the expression.</returns>
    public override ExpressionResult? VisitCastExpression(CastExpressionContext context) {
        // preprocessor mode on
        if (IsPreprocessorMode) {
            PreprocessCast(context);
            return null;
        }

        // error during preprocessing
        if (context.OriginalType is null || context.FinalType is null) {
            return null;
        }

        // resolve inner expression
        ExpressionResult? result = VisitExpression(context.Expression);

        // could not resolve expression
        if (result is null) {
            return null;
        }

        // cast expression
        bool success = CastExpression(context);

        // return a valid result if successful
        return success ? new ExpressionResult(result.Address, context.FinalType) : null;
    }

    /// <summary>
    /// Visit a nested expression.
    /// Convert the value to the desired type.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The address and type of the expression.</returns>
    public override ExpressionResult? VisitNestedExpression(NestedExpressionContext context) {
        // preprocessor mode on
        if (IsPreprocessorMode) {
            PreprocessNested(context);
            return null;
        }

        // error during preprocessing
        if (context.OriginalType is null || context.FinalType is null) {
            return null;
        }

        // visit inner expression
        ExpressionResult? result = VisitExpression(context.Body);

        // could not resolve expression
        if (result is null) {
            return null;
        }

        // cast expression
        bool success = CastExpression(context);

        // return a valid result if successful
        return success ? new ExpressionResult(result.Address, context.FinalType) : null;
    }

    // TODO implement
    public override ExpressionResult? VisitFunctionCallExpression(FunctionCallExpressionContext context) {
        // preprocessor mode on
        if (IsPreprocessorMode) {
            PreprocessFunctionCall(context);
            return null;
        }

        throw new NotImplementedException();
    }

    /// <summary>
    /// Visits a left unary expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The address and type of the expression.</returns>
    public override ExpressionResult? VisitLeftUnaryExpression(LeftUnaryExpressionContext context) {
        // preprocessor mode on
        if (IsPreprocessorMode) {
            PreprocessLeftUnary(context);
            return null;
        }

        return ResolveUnaryExpression(context, context.Expression, context.Operator.start.Type);
    }

    /// <summary>
    /// Visits a right unary expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The address and type of the expression.</returns>
    public override ExpressionResult? VisitRightUnaryExpression(RightUnaryExpressionContext context) {
        // preprocessor mode on
        if (IsPreprocessorMode) {
            PreprocessRightUnary(context);
            return null;
        }

        return ResolveUnaryExpression(context, context.Expression, context.Operator.start.Type);
    }

    /// <summary>
    /// Visits a multiplicative expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The address and type of the expression.</returns>
    public override ExpressionResult? VisitMultiplicativeExpression(MultiplicativeExpressionContext context) {
        return ResolveBinaryExpression(context, context.Left, context.Right, context.Operator.start.Type);
    }

    /// <summary>
    /// Visits a additive expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The address and type of the expression.</returns>
    public override ExpressionResult? VisitAdditiveExpression(AdditiveExpressionContext context) {
        return ResolveBinaryExpression(context, context.Left, context.Right, context.Operator.start.Type);
    }

    /// <summary>
    /// Visits a shift expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The address and type of the expression.</returns>
    public override ExpressionResult? VisitShiftExpression(ShiftExpressionContext context) {
        return ResolveBinaryExpression(context, context.Left, context.Right, context.Operator.start.Type);
    }

    /// <summary>
    /// Visits a comparison expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The address and type of the expression.</returns>
    public override ExpressionResult? VisitComparisonExpression(ComparisonExpressionContext context) {
        return ResolveBinaryExpression(context, context.Left, context.Right, context.Operator.start.Type);
    }

    /// <summary>
    /// Visits a logical expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The address and type of the expression.</returns>
    public override ExpressionResult? VisitLogicalExpression(LogicalExpressionContext context) {
        return ResolveBinaryExpression(context, context.Left, context.Right, context.Operator.start.Type);
    }

    /// <summary>
    /// Visits an assignment expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The address and type of the expression.</returns>
    public override ExpressionResult? VisitAssigmentExpression(AssigmentExpressionContext context) {
        // preprocessor mode on
        if (IsPreprocessorMode) {
            VisitExpression(context.Left);
            VisitExpression(context.Right);
            
            context.OriginalType = TypeHandler.CoreTypes.Void;
            context.Left.FinalType = context.Left.OriginalType;
            context.Right.FinalType = context.Left.OriginalType;

            return null;
        }
        
        // error during preprocessing
        if (context.Left.FinalType is null || context.Right.FinalType is null) {
            return null;
        }
        
        // calculate result
        ExpressionResult? result = context.Operator.start.Type switch {
            OP_ASSIGN => VisitExpression(context.Right),
            OP_MULTIPLY_ASSIGN => ResolveBinaryExpression(context, context.Left, context.Right, OP_MULTIPLY),
            OP_DIVIDE_ASSIGN => ResolveBinaryExpression(context, context.Left, context.Right, OP_DIVIDE),
            OP_MODULUS_ASSIGN => ResolveBinaryExpression(context, context.Left, context.Right, OP_MODULUS),
            OP_PLUS_ASSIGN => ResolveBinaryExpression(context, context.Left, context.Right, OP_PLUS),
            OP_MINUS_ASSIGN => ResolveBinaryExpression(context, context.Left, context.Right, OP_MINUS),
            OP_SHIFT_LEFT_ASSIGN => ResolveBinaryExpression(context, context.Left, context.Right, OP_SHIFT_LEFT),
            OP_SHIFT_RIGHT_ASSIGN => ResolveBinaryExpression(context, context.Left, context.Right, OP_SHIFT_RIGHT),
            OP_AND_ASSIGN => ResolveBinaryExpression(context, context.Left, context.Right, OP_AND),
            OP_OR_ASSIGN => ResolveBinaryExpression(context, context.Left, context.Right, OP_OR),
            OP_XOR_ASSIGN => ResolveBinaryExpression(context, context.Left, context.Right, OP_XOR),
            _ => null
        };

        if (result is null || context.Left is not IdentifierExpressionContext variable) {
            return null;
        }
        
        // assign value
        return AssignmentOperation(variable, context.Right.FinalType);
    }

    #endregion

    #region Preprocess methods

    /// <summary>
    /// Preprocess an expression.
    /// Turns preprocessor mode on for the duration of visiting the expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    private void PreprocessExpression(ExpressionContext context) {
        IsPreprocessorMode = true;

        VisitExpression(context);

        IsPreprocessorMode = false;
    }

    /// <summary>
    /// Preprocess a null expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    private void PreprocessNull(NullExpressionContext context) {
        // assign the null type
        context.OriginalType = TypeHandler.CoreTypes.Null;
    }

    /// <summary>
    /// Preprocess a constant.
    /// Store the constant in the data section and assign its type and address.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    private void PreprocessConstant(ConstantExpressionContext context) {
        // store the constant
        ConstantResult? result = VisitConstant(context.Constant);

        // error in the format of the constant
        if (result is null) {
            return;
        }

        // assign address and types
        context.Address = result.Address;
        context.OriginalType = result.Type;
        context.AlternativeType = result.AlternativeType;
    }

    /// <summary>
    /// Preprocess a variable identifier.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    private void PreprocessIdentifier(IdentifierExpressionContext context) {
        // get variable name
        string name = VisitId(context.Identifier);

        // get variable type and address
        ExpressionResult? variable = CodeHandler.GetVariable(name);

        // variable does not exist in the current context
        if (variable is null) {
            IssueHandler.Add(Issue.UnknownVariable(context, name));
            return;
        }

        // assign type and address
        context.OriginalType = variable.Type;
        context.Address = variable.Address;
    }

    // TODO implement
    private void PreprocessMemberAccess(MemberAccessExpressionContext context) {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Preprocess a type cast. Find the type we need to cast to.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    private void PreprocessCast(CastExpressionContext context) {
        // get the target type
        TypeIdentifier? targetType = VisitType(context.Type);

        // stop if the type does not exist
        if (targetType is null) {
            return;
        }

        // resolve the expression
        VisitExpression(context.Expression);

        // get the source type
        TypeIdentifier? sourceType = context.Expression.OriginalType;

        // stop if an error occured
        if (sourceType is null) {
            return;
        }

        // check if the cast exists
        // allow both explicit and implicit
        if (TypeHandler.Casts.ArePrimitiveTypes(sourceType, targetType)) {
            PrimitiveCast cast = TypeHandler.Casts.GetPrimitiveCast(sourceType, targetType);

            if (cast == PrimitiveCast.None) {
                IssueHandler.Add(Issue.InvalidCast(context, sourceType, targetType));
                return;
            }
        }

        // assign type
        context.Expression.FinalType = targetType;
        context.OriginalType = targetType;
    }

    /// <summary>
    /// Preprocess a nested expression. Forward the types of the inner expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    private void PreprocessNested(NestedExpressionContext context) {
        // visit inner expression
        VisitExpression(context.Body);

        // assign types
        context.Body.FinalType = context.Body.OriginalType;
        context.OriginalType = context.Body.OriginalType;
    }

    // TODO implement
    private void PreprocessFunctionCall(FunctionCallExpressionContext context) {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Preprocess a left unary expression. Forward the types of the inner expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    private void PreprocessLeftUnary(LeftUnaryExpressionContext context) {
        // visit inner expression
        VisitExpression(context.Expression);

        // forward types
        context.Expression.FinalType = context.Expression.OriginalType;
        context.OriginalType = context.Expression.OriginalType;
    }

    /// <summary>
    /// Preprocess a right unary expression. Forward the types of the inner expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    private void PreprocessRightUnary(RightUnaryExpressionContext context) {
        // visit inner expression
        VisitExpression(context.Expression);

        // forward types
        context.Expression.FinalType = context.Expression.OriginalType;
        context.OriginalType = context.Expression.OriginalType;
    }

    #endregion

    #region Helper methods

    private bool CastExpression(ExpressionContext context) {
        if (context.OriginalType is null || context.FinalType is null) {
            return false;
        }

        // TODO implement non-primitive casts

        // get the type of cast required
        PrimitiveCast cast = TypeHandler.Casts.GetPrimitiveCast(context.OriginalType, context.FinalType);

        // emit a cast instruction
        bool success = CodeHandler.Cast(context.OriginalType.Size, context.FinalType.Size, cast);

        // stop if failed
        if (!success) {
            IssueHandler.Add(Issue.InvalidCast(context, context.OriginalType, context.FinalType));
        }

        return success;
    }

    private ExpressionResult? ResolveBinaryExpression(ExpressionContext context, ExpressionContext leftContext, ExpressionContext rightContext, int operatorType) {
        if (IsPreprocessorMode) {
            // resolve left and right side
            VisitExpression(leftContext);
            VisitExpression(rightContext);

            // stop if an error occured
            if (leftContext.OriginalType is null || rightContext.OriginalType is null) {
                return null;
            }

            // for shift operators, the left cast is not converted and the right side is converted to i32
            bool isShiftOperator = operatorType switch {
                OP_SHIFT_LEFT => true,
                OP_SHIFT_RIGHT => true,
                _ => false
            };

            if (isShiftOperator) {
                leftContext.FinalType = leftContext.OriginalType;
                rightContext.FinalType = TypeHandler.CoreTypes.I32;
                context.OriginalType = leftContext.FinalType;

                return null;
            }

            // calculate results
            // we do not care if the operation for the common type exists or not
            TypeIdentifier? commonType = FindCommonType(leftContext, rightContext, true, true);

            // no valid common type exists
            if (commonType is null) {
                IssueHandler.Add(Issue.InvalidBinaryOperation(context, leftContext.OriginalType, rightContext.OriginalType, DefaultVocabulary.GetDisplayName(operatorType)));
            }

            // return type is bool for comparison operators
            // return type is void for assignment operators
            context.OriginalType = operatorType switch {
                OP_EQ => TypeHandler.CoreTypes.Bool,
                OP_NOT_EQ => TypeHandler.CoreTypes.Bool,
                OP_LESS => TypeHandler.CoreTypes.Bool,
                OP_GREATER => TypeHandler.CoreTypes.Bool,
                OP_LESS_EQ => TypeHandler.CoreTypes.Bool,
                OP_GREATER_EQ => TypeHandler.CoreTypes.Bool,
                OP_MULTIPLY_ASSIGN => TypeHandler.CoreTypes.Void,
                OP_DIVIDE_ASSIGN => TypeHandler.CoreTypes.Void,
                OP_MODULUS_ASSIGN => TypeHandler.CoreTypes.Void,
                OP_PLUS_ASSIGN => TypeHandler.CoreTypes.Void,
                OP_MINUS_ASSIGN => TypeHandler.CoreTypes.Void,
                OP_SHIFT_LEFT_ASSIGN => TypeHandler.CoreTypes.Void,
                OP_SHIFT_RIGHT_ASSIGN => TypeHandler.CoreTypes.Void,
                OP_AND_ASSIGN => TypeHandler.CoreTypes.Void,
                OP_OR_ASSIGN => TypeHandler.CoreTypes.Void,
                OP_XOR_ASSIGN => TypeHandler.CoreTypes.Void,
                _ => commonType
            };

            leftContext.FinalType = commonType;
            rightContext.FinalType = commonType;

            return null;
        }

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
            OP_LESS => isPrimitiveType ? BinaryComparisonOperation(left.Type, right.Type, OperationCode.lt) : null,
            OP_GREATER => isPrimitiveType ? BinaryComparisonOperation(left.Type, right.Type, OperationCode.gt) : null,
            OP_LESS_EQ => isPrimitiveType ? BinaryComparisonOperation(left.Type, right.Type, OperationCode.lte) : null,
            OP_GREATER_EQ => isPrimitiveType ? BinaryComparisonOperation(left.Type, right.Type, OperationCode.gte) : null,
            OP_AND => isPrimitiveType ? BinaryBitwiseOperation(left.Type, right.Type, OperationCode.and) : null,
            OP_OR => isPrimitiveType ? BinaryBitwiseOperation(left.Type, right.Type, OperationCode.or) : null,
            OP_XOR => isPrimitiveType ? BinaryBitwiseOperation(left.Type, right.Type, OperationCode.xor) : null,
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
        // resolve the inner expression
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

    private ExpressionResult? UnaryNumberOperation(TypeIdentifier type, OperationCode integerCode, OperationCode floatCode) {
        // must be an integer or float
        if (TypeHandler.Casts.IsIntegerType(type)) {
            MemoryAddress address = CodeHandler.PrimitiveUnaryOperation(type.Size, integerCode);
            return new ExpressionResult(address, type);
        }

        if (TypeHandler.Casts.IsFloatType(type)) {
            MemoryAddress address = CodeHandler.PrimitiveUnaryOperation(type.Size, floatCode);
            return new ExpressionResult(address, type);
        }

        return null;
    }

    private ExpressionResult? UnaryBoolOperation(TypeIdentifier type, OperationCode code) {
        // must be bool
        if (type == TypeHandler.CoreTypes.Bool) {
            MemoryAddress address = CodeHandler.PrimitiveUnaryOperation(type.Size, code);
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
            MemoryAddress address = CodeHandler.PrimitiveBinaryOperation(leftType.Size, integerCode);
            return new ExpressionResult(address, leftType);
        }

        if (TypeHandler.Casts.IsFloatType(leftType)) {
            MemoryAddress address = CodeHandler.PrimitiveBinaryOperation(leftType.Size, floatCode);
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

        MemoryAddress address = CodeHandler.PrimitiveBinaryOperation(leftType.Size, code);

        return new ExpressionResult(address, leftType);
    }

    private ExpressionResult? BinaryComparisonOperation(TypeIdentifier leftType, TypeIdentifier rightType, OperationCode code) {
        // must operate on the same types
        if (leftType != rightType) {
            return null;
        }

        // can be any type
        MemoryAddress address = CodeHandler.PrimitiveComparisonOperation(leftType.Size, code);
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
        MemoryAddress address = CodeHandler.PrimitiveShiftOperation(leftType.Size, code);
        return new ExpressionResult(address, leftType);
    }

    private ExpressionResult? AssignmentOperation(IdentifierExpressionContext left, TypeIdentifier rightType) {
        // must operate on the same types
        if (left.FinalType is null || left.Address is null || rightType != left.FinalType) {
            return null;
        }

        // can be any type
        CodeHandler.PrimitiveAssignmentOperation(left.FinalType.Size, (int)left.Address.Value.Value);

        // assignment returns void
        return new ExpressionResult(MemoryAddress.Null, TypeHandler.CoreTypes.Void);
    }

    /// <summary>
    /// Finds the best common type for a binary expression.
    /// </summary>
    /// <param name="left">The expression of the left side.</param>
    /// <param name="right">The expression of the right side.</param>
    /// <param name="allowLeftCast">Allow changing the type of the left side.</param>
    /// <param name="allowRightCast">Allow changing the type of the right side.</param>
    /// <returns></returns>
    private TypeIdentifier? FindCommonType(ExpressionContext left, ExpressionContext right, bool allowLeftCast, bool allowRightCast) {
        TypeIdentifier?[] leftTypes = left is ConstantExpressionContext leftConstant ? [leftConstant.OriginalType, leftConstant.AlternativeType] : [left.OriginalType];
        TypeIdentifier?[] rightTypes = right is ConstantExpressionContext rightConstant ? [rightConstant.OriginalType, rightConstant.AlternativeType] : [right.OriginalType];

        PrimitiveCast bestCast = PrimitiveCast.None;
        TypeIdentifier? bestType = null;

        // cast left to right
        if (allowLeftCast) {
            foreach (TypeIdentifier? sourceType in leftTypes) {
                foreach (TypeIdentifier? targetType in rightTypes) {
                    if (sourceType is null || targetType is null) {
                        continue;
                    }

                    // TODO implement non-primitive cast
                    if (!TypeHandler.Casts.ArePrimitiveTypes(sourceType, targetType)) {
                        continue;
                    }

                    PrimitiveCast cast = TypeHandler.Casts.GetPrimitiveCast(sourceType, targetType);

                    if (bestCast < cast) {
                        bestCast = cast;
                        bestType = targetType;
                    }
                }
            }
        }

        // cast right to left
        if (allowRightCast) {
            foreach (TypeIdentifier? sourceType in rightTypes) {
                foreach (TypeIdentifier? targetType in leftTypes) {
                    if (sourceType is null || targetType is null) {
                        continue;
                    }

                    // TODO implement non-primitive cast
                    if (!TypeHandler.Casts.ArePrimitiveTypes(sourceType, targetType)) {
                        continue;
                    }

                    PrimitiveCast cast = TypeHandler.Casts.GetPrimitiveCast(sourceType, targetType);

                    if (bestCast < cast) {
                        bestCast = cast;
                        bestType = targetType;
                    }
                }
            }
        }

        if (!bestCast.IsImplicit()) {
            return null;
        }

        return bestType;
    }

    #endregion
}