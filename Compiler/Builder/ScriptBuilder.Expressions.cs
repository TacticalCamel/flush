namespace Compiler.Builder;

using Data;
using static Grammar.ScrantonParser;

internal sealed partial class ScriptBuilder {
    public override ExpressionResult? VisitAdditiveOperatorExpression(AdditiveOperatorExpressionContext context) {
        return ResolveBinaryExpression(context.Left, context.Right, context.Operator.start.Type);
    }

    public override ExpressionResult? VisitMultiplicativeOperatorExpression(MultiplicativeOperatorExpressionContext context) {
        return ResolveBinaryExpression(context.Left, context.Right, context.Operator.start.Type);
    }

    public override ExpressionResult? VisitShiftOperatorExpression(ShiftOperatorExpressionContext context) {
        return ResolveBinaryExpression(context.Left, context.Right, context.Operator.start.Type);
    }

    public override ExpressionResult? VisitComparisonOperatorExpression(ComparisonOperatorExpressionContext context) {
        return ResolveBinaryExpression(context.Left, context.Right, context.Operator.start.Type);
    }

    public override ExpressionResult? VisitLogicalOperatorExpression(LogicalOperatorExpressionContext context) {
        return ResolveBinaryExpression(context.Left, context.Right, context.Operator.start.Type);
    }

    public override ExpressionResult? VisitAssigmentOperatorExpression(AssigmentOperatorExpressionContext context) {
        return ResolveBinaryExpression(context.Left, context.Right, context.Operator.start.Type);
    }

    private ExpressionResult? ResolveBinaryExpression(ExpressionContext leftContext, ExpressionContext rightContext, int operatorType) {
        // resolve the expression on both sides
        ExpressionResult? left = VisitExpression(leftContext);
        ExpressionResult? right = VisitExpression(rightContext);

        // return if an error occured
        // the type of the expression result can still be null
        if (left is null || right is null) {
            return null;
        }
        
        Console.WriteLine($"{DefaultVocabulary.GetSymbolicName(operatorType)} \t{left} \t{right}");
        
        // TODO implement
        return new ExpressionResult(MemoryAddress.NULL, TypeIdentifier.Null);
    }

    public override ExpressionResult? VisitLeftUnaryOperatorExpression(LeftUnaryOperatorExpressionContext context) {
        // TODO implement
        return new ExpressionResult(MemoryAddress.NULL, TypeIdentifier.Null);
    }

    public override ExpressionResult? VisitRightUnaryOperatorExpression(RightUnaryOperatorExpressionContext context) {
        // TODO implement
        return new ExpressionResult(MemoryAddress.NULL, TypeIdentifier.Null);
    }

    public override ExpressionResult? VisitMemberAccessOperatorExpression(MemberAccessOperatorExpressionContext context) {
        // TODO implement
        return new ExpressionResult(MemoryAddress.NULL, TypeIdentifier.Null);
    }

    public override ExpressionResult? VisitIdentifierExpression(IdentifierExpressionContext context) {
        // we have no way of knowing the actual meaning of this identifier
        // it could be a variable or a member access
        
        // assume it is a variable and handle other cases elsewhere
        // since this node should not be visited in other cases,
        // we can throw an error if the variable was not found
        
        string name = VisitId(context.Identifier);
        
        // TODO implement
        return new ExpressionResult(MemoryAddress.NULL, TypeIdentifier.Null);
    }

    public override ExpressionResult? VisitNestedExpression(NestedExpressionContext context) {
        // nothing to do, nesting the expression was only necessary to change operator precedence
        return VisitExpression(context.Body);
    }
    
    #region Unused visit methods

    public override object? VisitOpLeftUnary(OpLeftUnaryContext context) {
        return null;
    }

    public override object? VisitOpRightUnary(OpRightUnaryContext context) {
        return null;
    }

    public override object? VisitOpMultiplicative(OpMultiplicativeContext context) {
        return null;
    }

    public override object? VisitOpAdditive(OpAdditiveContext context) {
        return null;
    }
    
    public override object? VisitOpShift(OpShiftContext context) {
        return null;
    }

    public override object? VisitOpComparison(OpComparisonContext context) {
        return null;
    }

    public override object? VisitOpLogical(OpLogicalContext context) {
        return null;
    }

    public override object? VisitOpAssignment(OpAssignmentContext context) {
        return null;
    }

    #endregion
}