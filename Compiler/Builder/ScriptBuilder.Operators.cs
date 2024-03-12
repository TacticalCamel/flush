namespace Compiler.Builder;

using Antlr4.Runtime;
using Analysis;
using Grammar;
using Data;
using System.Globalization;
using static Grammar.ScrantonParser;

internal sealed partial class ScriptBuilder {
    public override object? VisitAdditiveOperatorExpression(AdditiveOperatorExpressionContext context) {
        // get operator name
        object? op = Visit(context.AdditiveOperator);

        // should never happen, but throw an error if it does
        if (op is not string methodName) {
            return null;
        }

        // TODO
        // find matching method

        /*object? left =*/
        Visit(context.Left);
        /*object? right =*/
        Visit(context.Right);

        // TODO
        // method call
        Console.WriteLine($"call {methodName}<{2}>");

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