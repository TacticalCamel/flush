namespace Compiler.Builder;

using Analysis;
using Data;
using static Grammar.ScrantonParser;

internal sealed partial class ScriptBuilder {
    public override object? VisitAdditiveOperatorExpression(AdditiveOperatorExpressionContext context) {
        // get operator name
        object? op = Visit(context.AdditiveOperator);

        // return if failed to get the method name, should never happen
        if (op is not string methodName) {
            return null;
        }

        ExpressionResult? left = VisitExpression(context.Left);
        ExpressionResult? right = VisitExpression(context.Right);
        
        
        // TODO
        // find matching method
        
        // TODO
        // method call
        Logger.Debug($"call {methodName}({left}, {right})");

        return null;
    }

    public override object? VisitOpAdditive(OpAdditiveContext context) {
        if (context.OP_PLUS() != null) {
            return "op_Addition";
        }

        if (context.OP_MINUS() != null) {
            return "op_Subtraction";
        }

        WarningHandler.Add(Warning.UnrecognizedOperator(context, context.start.Text));
        return null;
    }
}