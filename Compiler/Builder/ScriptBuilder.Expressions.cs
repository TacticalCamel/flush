namespace Compiler.Builder;

using Data;
using Analysis;
using Interpreter.Bytecode;
using static Grammar.ScrantonParser;

// ScriptBuilder.Expressions: methods related to visiting expressions
internal sealed partial class ScriptBuilder {
    /// <summary>
    /// Preprocesses an expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    public void PreprocessExpression(ExpressionContext context) {
        IsPreprocessorMode = true;

        VisitExpression(context);

        IsPreprocessorMode = false;
    }

    /// <summary>
    /// Visit an expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The address and type of the expression.</returns>
    public override ExpressionResult? VisitExpression(ExpressionContext context) {
        // visit expression subtype
        ExpressionResult? result = (ExpressionResult?)Visit(context);

        return result;
    }

    /// <summary>
    /// Visit a constant expression.
    /// Push the value to the stack and convert it to the desired type.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The address and type of the expression.</returns>
    public override ExpressionResult? VisitConstantExpression(ConstantExpressionContext context) {
        if (IsPreprocessorMode) {
            // get expression result
            ExpressionResult? result = VisitConstant(context.Constant);

            if (result is null) {
                return null;
            }

            // assign address and type
            context.Address = result.Address;
            context.OriginalType = result.Type;

            // assign secondary type if it's a constant result
            if (result is ConstantResult constantResult) {
                context.AlternativeType = constantResult.SecondaryType;
            }

            return null;
        }

        // error during first pass, should never happen
        if (context.Address is null || context.OriginalType is null || context.FinalType is null) {
            return null;
        }

        // push the constant to the stack if it's stored in the data section
        if (context.Address.Value.Location == MemoryLocation.Data) {
            CodeHandler.PushBytesFromData((int)context.Address.Value.Value, context.OriginalType.Size);
        }

        bool success = CastExpression(context);

        return success ? new ExpressionResult(context.Address.Value, context.FinalType) : null;
    }

    // TODO implement
    public override ExpressionResult? VisitIdentifierExpression(IdentifierExpressionContext context) {
        throw new NotImplementedException();

        /*string name = VisitId(context.Identifier);

        ExpressionResult? expressionResult = CodeHandler.GetVariable(name);

        if (expressionResult is null) {
            IssueHandler.Add(Issue.UnknownVariable(context, name));
            return null;
        }

        return expressionResult;*/
    }

    // TODO implement
    public override ExpressionResult? VisitMemberAccessOperatorExpression(MemberAccessOperatorExpressionContext context) {
        throw new NotImplementedException();

        /*
        ExpressionResult? left = VisitExpression(context.Type);

        if (left is null) {
            return null;
        }

        string name = VisitId(context.Member);

        IssueHandler.Add(Issue.FeatureNotImplemented(context, "member access"));
        return null;
        */
    }

    /// <summary>
    /// Visit a cast expression.
    /// Convert the value to the desired type.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The address and type of the expression.</returns>
    public override ExpressionResult? VisitCastExpression(CastExpressionContext context) {
        if (IsPreprocessorMode) {
            // get the target type
            TypeIdentifier? targetType = VisitType(context.Type);

            // stop if the type does not exist
            if (targetType is null) {
                return null;
            }

            // resolve the expression
            VisitExpression(context.Expression);

            // get the source type
            TypeIdentifier? sourceType = context.Expression.OriginalType;

            // stop if an error occured
            if (sourceType is null) {
                return null;
            }

            // check if the cast exists
            // allow both explicit and implicit
            if (TypeHandler.Casts.ArePrimitiveTypes(sourceType, targetType)) {
                PrimitiveCast cast = TypeHandler.Casts.GetPrimitiveCast(sourceType, targetType);

                if (cast == PrimitiveCast.None) {
                    IssueHandler.Add(Issue.InvalidCast(context, sourceType, targetType));
                    return null;
                }
            }

            context.Expression.FinalType = targetType;
            context.OriginalType = targetType;

            return null;
        }

        ExpressionResult? result = VisitExpression(context.Expression);

        if (result is null) {
            return null;
        }

        bool success = CastExpression(context);

        return success ? new ExpressionResult(result.Address, context.FinalType!) : null;
    }

    /// <summary>
    /// Visit a nested expression.
    /// Convert the value to the desired type.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The address and type of the expression.</returns>
    public override ExpressionResult? VisitNestedExpression(NestedExpressionContext context) {
        if (IsPreprocessorMode) {
            VisitExpression(context.Body);

            // assign types
            context.Body.FinalType = context.Body.OriginalType;
            context.OriginalType = context.Body.OriginalType;

            return null;
        }

        ExpressionResult? result = VisitExpression(context.Body);

        if (result is null) {
            return null;
        }

        bool success = CastExpression(context);

        return success ? new ExpressionResult(result.Address, context.FinalType!) : null;
    }

    // TODO implement
    public override ExpressionResult? VisitFunctionCallExpression(FunctionCallExpressionContext context) {
        throw new NotImplementedException();

        /*
        ExpressionResult? callerExpression = VisitExpression(context.Caller);

        // get parameter expressions
        ExpressionContext[] expressions = context.ExpressionList.expression();

        // create an array for results
        ExpressionResult[] results = new ExpressionResult[expressions.Length];

        // resolve parameters and return if any of them was null
        for (int i = 0; i < expressions.Length; i++) {
            ExpressionResult? result = VisitExpression(expressions[i]);

            if (result is null) {
                return null;
            }

            results[i] = result;
        }

        IssueHandler.Add(Issue.FeatureNotImplemented(context, "function call"));
        return null;
        */
    }

    /// <summary>
    /// Visits a left unary expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The address and type of the expression.</returns>
    public override ExpressionResult? VisitLeftUnaryOperatorExpression(LeftUnaryOperatorExpressionContext context) {
        return ResolveUnaryExpression(context, context.Expression, context.Operator.start.Type);
    }

    /// <summary>
    /// Visits a right unary expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The address and type of the expression.</returns>
    public override ExpressionResult? VisitRightUnaryOperatorExpression(RightUnaryOperatorExpressionContext context) {
        return ResolveUnaryExpression(context, context.Expression, context.Operator.start.Type);
    }

    /// <summary>
    /// Visits a multiplicative expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The address and type of the expression.</returns>
    public override ExpressionResult? VisitMultiplicativeOperatorExpression(MultiplicativeOperatorExpressionContext context) {
        return ResolveBinaryExpression(context, context.Left, context.Right, context.Operator.start.Type);
    }

    /// <summary>
    /// Visits a additive expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The address and type of the expression.</returns>
    public override ExpressionResult? VisitAdditiveOperatorExpression(AdditiveOperatorExpressionContext context) {
        return ResolveBinaryExpression(context, context.Left, context.Right, context.Operator.start.Type);
    }

    /// <summary>
    /// Visits a shift expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The address and type of the expression.</returns>
    public override ExpressionResult? VisitShiftOperatorExpression(ShiftOperatorExpressionContext context) {
        return ResolveBinaryExpression(context, context.Left, context.Right, context.Operator.start.Type);
    }

    /// <summary>
    /// Visits a comparison expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The address and type of the expression.</returns>
    public override ExpressionResult? VisitComparisonOperatorExpression(ComparisonOperatorExpressionContext context) {
        return ResolveBinaryExpression(context, context.Left, context.Right, context.Operator.start.Type);
    }

    /// <summary>
    /// Visits a logical expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The address and type of the expression.</returns>
    public override ExpressionResult? VisitLogicalOperatorExpression(LogicalOperatorExpressionContext context) {
        return ResolveBinaryExpression(context, context.Left, context.Right, context.Operator.start.Type);
    }

    /// <summary>
    /// Visits an assignment expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The address and type of the expression.</returns>
    public override ExpressionResult? VisitAssigmentOperatorExpression(AssigmentOperatorExpressionContext context) {
        return ResolveBinaryExpression(context, context.Left, context.Right, context.Operator.start.Type);
    }

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
                OP_SHIFT_LEFT_ASSIGN => true,
                OP_SHIFT_RIGHT_ASSIGN => true,
                _ => false
            };

            if (isShiftOperator) {
                leftContext.FinalType = leftContext.OriginalType;
                rightContext.FinalType = TypeHandler.CoreTypes.I32;
                context.OriginalType = leftContext.FinalType;

                return null;
            }

            // casting the left side is not valid for assignment operators
            bool allowLeftCast = operatorType switch {
                OP_PLUS => true,
                OP_MINUS => true,
                OP_MULTIPLY => true,
                OP_DIVIDE => true,
                OP_MODULUS => true,
                OP_EQ => true,
                OP_NOT_EQ => true,
                OP_LESS => true,
                OP_GREATER => true,
                OP_LESS_EQ => true,
                OP_GREATER_EQ => true,
                OP_AND => true,
                OP_OR => true,
                OP_XOR => true,
                OP_SHIFT_LEFT => true,
                OP_SHIFT_RIGHT => true,
                _ => false
            };

            // calculate results
            // we do not care if the operation for the common type exists or not
            TypeIdentifier? commonType = FindCommonType(leftContext, rightContext, allowLeftCast, true);

            // stop if no valid common type exists
            if (commonType is null) {
                IssueHandler.Add(Issue.InvalidBinaryOperation(context, leftContext.OriginalType, rightContext.OriginalType, DefaultVocabulary.GetDisplayName(operatorType)));
            }

            // return type is bool for comparison operators
            context.OriginalType = operatorType switch {
                OP_EQ => TypeHandler.CoreTypes.Bool,
                OP_NOT_EQ => TypeHandler.CoreTypes.Bool,
                OP_LESS => TypeHandler.CoreTypes.Bool,
                OP_GREATER => TypeHandler.CoreTypes.Bool,
                OP_LESS_EQ => TypeHandler.CoreTypes.Bool,
                OP_GREATER_EQ => TypeHandler.CoreTypes.Bool,
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
        if (IsPreprocessorMode) {
            // resolve the inner expression
            VisitExpression(innerContext);

            // stop if an error occured
            if (innerContext.OriginalType is null) {
                return null;
            }

            // unary operators do not change the type
            innerContext.FinalType = innerContext.OriginalType;
            context.OriginalType = innerContext.OriginalType;

            return null;
        }

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
}