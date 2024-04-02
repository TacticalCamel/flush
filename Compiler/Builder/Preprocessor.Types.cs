namespace Compiler.Builder;

using Data;
using Analysis;
using Interpreter.Types;
using static Grammar.ScrantonParser;

internal sealed partial class Preprocessor {
    /// <summary>
    /// Visit a type definition.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitTypeDefinition(TypeDefinitionContext context) {
        Visit(context);

        return null;
    }

    /// <summary>
    /// Visit a class definition.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitClassDefinition(ClassDefinitionContext context) {
        string name = VisitId(context.TypeName);

        VisitTypeBody(context.Body);

        return null;
    }

    /// <summary>
    /// Visit a struct definition.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitStructDefinition(StructDefinitionContext context) {
        string name = VisitId(context.TypeName);

        VisitTypeBody(context.Body);

        return null;
    }

    public override object? VisitTypeBody(TypeBodyContext context) {
        VisitChildren(context);

        return null;
    }

    public override object? VisitFieldDefinition(FieldDefinitionContext context) {
        return base.VisitFieldDefinition(context);
    }

    public override object? VisitMethodDefinition(MethodDefinitionContext context) {
        return base.VisitMethodDefinition(context);
    }

    public override object? VisitConstructorDefinition(ConstructorDefinitionContext context) {
        return base.VisitConstructorDefinition(context);
    }

    /// <summary>
    /// Visits a return type.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The identifier of the type if it exists, null otherwise.</returns>
    public override TypeIdentifier? VisitReturnType(ReturnTypeContext context) {
        // void return type
        if (context.Void is not null) {
            return TypeHandler.CoreTypes.Void;
        }

        // regular return type
        return VisitType(context.Type);
    }

    /// <summary>
    /// Visit a modifier list.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>A modifier struct if successful, null otherwise.</returns>
    public override object? VisitModifierList(ModifierListContext context) {
        // set no flags initially
        Modifier result = default;

        // visit each modifier in order
        foreach (ModifierContext modifierContext in context.modifier()) {
            // get the modifier value
            Modifier? modifier = context.start.Type switch {
                KW_PRIVATE => Modifier.Private,
                _ => null
            };

            // invalid value
            if (modifier is null) {
                IssueHandler.Add(Issue.InvalidModifier(modifierContext, modifierContext.start.Text));
                return null;
            }

            // flag already set
            if ((result & modifier.Value) > 0) {
                IssueHandler.Add(Issue.DuplicateModifier(modifierContext, modifierContext.start.Text));
                return null;
            }

            // set the flag
            result |= modifier.Value;
        }

        return result;
    }
}