namespace Compiler.Builder;

using Data;
using Analysis;
using Interpreter.Types;
using static Grammar.ScrantonParser;

internal sealed partial class ScriptBuilder {
    public override object? VisitVariableDeclaration(VariableDeclarationContext context) {
        ExpressionResult? result = VisitExpression(context.Expression);

        /*VariableIdentifier? variable = VisitVariableWithType(context.VariableWithType);

        if (variable is null) {
            return null;
        }

        Logger.Debug($"{variable} = {result?.ToString() ?? "null"}");*/

        return null;
    }

    /*public override VariableIdentifier? VisitVariableWithType(VariableWithTypeContext context) {
        TypeIdentifier? type = VisitType(context.Type);
        string name = VisitId(context.Name);

        if (type is null) {
            return null;
        }

        return new VariableIdentifier(type, name);
    }*/
}