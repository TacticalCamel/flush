namespace Compiler.Builder;

using Analysis;
using Data;
using Interpreter.Types;
using static Grammar.ScrantonParser;

// ScriptBuilder.Types: methods related to visiting type definitions and type names
internal sealed partial class ScriptBuilder {
    
    private void ProcessTypeDefinitions(TypeDefinitionContext[] _) {
        // TODO implement
        // create array for type drafts
        /*TypeDraft[] drafts = new TypeDraft[typeDefinitions.Length];

        // create type drafts 
        for (int i = 0; i < typeDefinitions.Length; i++) {
            TypeDraft? draft = CreateTypeDraft(typeDefinitions[i]);

            if (draft is null) {
                return;
            }

            drafts[i] = draft;
        }

        //

        foreach (TypeDraft draft in drafts) {
            Console.WriteLine($"{draft.Name} {draft.Size?.ToString() ?? "null"}");
        }*/
    }

    
    /*
    private TypeDraft? CreateTypeDraft(TypeDefinitionContext context) {
        // the modifiers of the type
        if (VisitModifierList(context.Modifiers) is not Modifier modifiers) {
            return null;
        }

        // whether the type is a reference type
        bool isReference = context.Keyword.Type == KW_CLASS;

        // the name of the type
        string name = VisitId(context.TypeName);

        // create a draft
        TypeDraft draft = new() {
            Modifiers = modifiers,
            IsReference = isReference,
            Name = name
        };
        
        return draft;
    }

    private bool ProcessTypeDefinition(TypeDefinitionContext context) {
        return true;

        // loading finished
        if (context.LoadingState is not null) {
            return true;
        }

        // 1st pass: create draft
        if (context.TypeDraft is null) {
            // the modifiers of the type
            if (VisitModifierList(context.Modifiers) is not Modifier modifiers) {
                // set state to failed
                context.LoadingState = false;

                // finished loading
                return true;
            }

            // whether the type is a reference type
            bool isReference = context.Keyword.Type == KW_CLASS;

            // the name of the type
            string name = VisitId(context.TypeName);

            // create a draft
            context.TypeDraft = new TypeDraft {
                Modifiers = modifiers,
                IsReference = isReference,
                Name = name
            };

            // register that the type exists
            //TypeHandler.AddDraft(context.TypeDraft);

            // loading continues, do not visit type body
            // type members might contain a type that will be loaded after this one
            return false;
        }

        // 2nd pass: get size
        // issue: struct layouts can have circles in them
        // solution: explore the dependencies of a type recursively and detect circles

        return true;
    }

    /// <summary>
    /// Visit a type definition body.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitTypeBody(TypeBodyContext context) {
        // visit type members
        VisitChildren(context);

        return null;
    }

    /// <summary>
    /// Visit a field definition.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitFieldDefinition(FieldDefinitionContext context) {
        if (VisitModifierList(context.Modifiers) is not Modifier modifiers) return null;

        return null;
    }
    
    */

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
    
    /// <summary>
    /// Visit a type name.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The type if it exists, null otherwise.</returns>
    public override TypeIdentifier? VisitType(TypeContext context) {
        return (TypeIdentifier?)Visit(context);
    }

    /// <summary>
    /// Visit a non-generic type.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The identifier of the type if it exists, null otherwise.</returns>
    public override TypeIdentifier? VisitSimpleType(SimpleTypeContext context) {
        // get the name of the type
        string name = VisitId(context.Name);

        // search the type by name
        TypeDefinition? type = TypeHandler.GetTypeByName(name);
        
        // stop if the type does not exist
        if (type is null) {
            IssueHandler.Add(Issue.UnrecognizedType(context, name));
            return null;
        }
        
        // generic parameter count doesn't match
        if (type.GenericParameterCount != 0) {
            IssueHandler.Add(Issue.GenericParameterCountMismatch(context, type.Name, type.GenericParameterCount, 0));
            return null;
        }

        return new TypeIdentifier(type, []);
    }

    /// <summary>
    /// Visit a generic type.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The identifier of the type if it exists, null otherwise.</returns>
    public override TypeIdentifier? VisitGenericType(GenericTypeContext context) {
        // get the name of the type
        string name = VisitId(context.Name);

        // search the type by name
        TypeDefinition? type = TypeHandler.GetTypeByName(name);

        // stop if the type does not exist
        if (type is null) {
            IssueHandler.Add(Issue.UnrecognizedType(context, name));
            return null;
        }

        // get generic parameter nodes
        TypeContext[] typeContexts = context.type();

        // generic parameter count doesn't match
        if (type.GenericParameterCount != typeContexts.Length) {
            IssueHandler.Add(Issue.GenericParameterCountMismatch(context, type.Name, type.GenericParameterCount, typeContexts.Length));
            return null;
        }

        // create an array for results
        TypeIdentifier[] genericParameters = new TypeIdentifier[typeContexts.Length];

        // for every generic parameter
        for (int i = 0; i < typeContexts.Length; i++) {
            // get the type identifier
            TypeIdentifier? typeIdentifier = VisitType(typeContexts[i]);

            // stop if the type does not exist
            if (typeIdentifier is null) {
                return null;
            }

            // assign array element
            genericParameters[i] = typeIdentifier;
        }

        return new TypeIdentifier(type, genericParameters);
    }
}