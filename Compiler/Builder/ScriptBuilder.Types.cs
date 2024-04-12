namespace Compiler.Builder;

using Analysis;
using Data;
using Types;
using Interpreter;
using Interpreter.Bytecode;
using Interpreter.Types;
using static Grammar.ScrantonParser;

// ScriptBuilder.Types: methods related to visiting type definitions and type names
internal sealed partial class ScriptBuilder {
    private void ProcessTypeDefinitions(TypeDefinitionContext[] typeDefinitions) {
        // create array for type drafts
        TypeDraft[] drafts = new TypeDraft[typeDefinitions.Length];

        // create type drafts 
        for (int i = 0; i < typeDefinitions.Length; i++) {
            TypeDraft? draft = CreateTypeDraft(typeDefinitions[i]);

            if (draft is null) {
                return;
            }

            drafts[i] = draft;
        }

        // process types recursively
        for (int i = 0; i < typeDefinitions.Length; i++) {
            ProcessType(typeDefinitions, drafts, i);
        }
        
        foreach (TypeDraft type in drafts) {
            Console.WriteLine(type.Name);

            foreach (FieldDraft field in type.Fields) {
                Console.WriteLine($"    {field.Type ?? $"<{field.GenericIndex}>"} {field.Name}{(field.GenericIndex >= 0 ? "" : $": {field.Size}")}");
            }
            
            Console.WriteLine();
        }
        
    }

    private TypeDraft? CreateTypeDraft(TypeDefinitionContext context) {
        // the modifiers of the type
        if (VisitModifierList(context.Modifiers) is not Modifier modifiers) return null;

        // whether the type is a reference type
        bool isReference = context.Keyword.Type == KW_CLASS;

        // the name of the type
        string name = VisitId(context.TypeName);

        // the name of the generic parameters
        string[] genericParameterNames = context.GenericParameters is null ? [] : VisitGenericParameters(context.GenericParameters);

        // create a draft
        TypeDraft draft = new() {
            Modifiers = modifiers,
            IsReference = isReference,
            Name = name,
            GenericParameterNames = genericParameterNames
        };

        return draft;
    }

    private void ProcessType(TypeDefinitionContext[] typeDefinitions, TypeDraft[] drafts, int index) {
        // get current context and draft from the arrays
        TypeDefinitionContext context = typeDefinitions[index];
        TypeDraft draft = drafts[index];

        // already done processing
        if (draft.IsComplete) {
            return;
        }

        // already in progress, which means there is a layout circle
        if (draft.InProgress) {
            draft.IsComplete = true;

            IssueHandler.Add(Issue.StructLayoutCircle(context, draft.Name));

            return;
        }

        // begin processing
        draft.InProgress = true;
        
        // visit each field
        foreach (FieldDefinitionContext field in typeDefinitions[index].Body.fieldDefinition()) {
            // process field
            FieldDraft? fieldDraft = ProcessField(field, typeDefinitions, drafts, index);

            // failed to process
            if (fieldDraft is null) {
                draft.IsComplete = true;
                return;
            }
            
            // add draft to fields
            draft.Fields.Add(fieldDraft);
        }

        // finish processing
        draft.IsComplete = true;
        draft.InProgress = false;
        
        // to actual type
        TypeDefinition definition = ToTypeDefinition(draft);

        TypeHandler.LoadType(definition);
    }

    private unsafe FieldDraft? ProcessField(FieldDefinitionContext field, TypeDefinitionContext[] typeDefinitions, TypeDraft[] drafts, int index) {
        // get current type draft from the array
        TypeDraft draft = drafts[index];

        // get field modifiers
        if (VisitModifierList(field.Modifiers) is not Modifier modifiers) {
            return null;
        }

        // get field name
        string fieldName = VisitId(field.Name);

        // check the field name matches any generic parameter names
        int genericIndex = Array.IndexOf(draft.GenericParameterNames, field.Type.GetText());

        if (genericIndex >= 0) {
            return new FieldDraft {
                Modifiers = modifiers,
                Type = null,
                Name = fieldName,
                GenericIndex = genericIndex,
                Size = -1
            };
        }

        // check if the field name matches any type drafts
        int draftIndex = Array.FindIndex(drafts, x => x.Name == VisitId(field.Type.Name));

        if (draftIndex >= 0) {
            ProcessType(typeDefinitions, drafts, draftIndex);

            return new FieldDraft {
                Modifiers = modifiers,
                Type = drafts[draftIndex],
                Name = fieldName,
                GenericIndex = -1,
                Size = drafts[draftIndex].IsReference ? sizeof(ObjectReference) : drafts[draftIndex].Fields.Sum(x => x.Size)
            };
        }

        // check if the field is of a loaded type
        TypeIdentifier? type = VisitType(field.Type);

        if (type is not null) {
            return new FieldDraft {
                Modifiers = modifiers,
                Type = type,
                Name = fieldName,
                GenericIndex = -1,
                Size = type.Size
            };
        }

        return null;
    }

    private TypeDefinition ToTypeDefinition(TypeDraft draft) {
        return new TypeDefinition {
            Id = ClassLoader.GetTypeIdentifier(TypeHandler.ProgramModule ?? string.Empty, draft.Name),
            Modifiers = draft.Modifiers,
            IsReference = draft.IsReference,
            Name = draft.Name,
            GenericParameterCount = draft.GenericParameterNames.Length,
            Fields = [],
            Methods = [],
            Size = 8
        };
    }

    /// <summary>
    /// Visit a list of generic parameters.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The names of the parameters as an array of strings.</returns>
    public override string[] VisitGenericParameters(GenericParametersContext context) {
        return context.id().Select(VisitId).ToArray();
    }

    /// <summary>
    /// Visit a return type.
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
    /// <returns>A modifier enum with the correct bit flags set.</returns>
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
                continue;
            }

            // flag already set
            if ((result & modifier.Value) > 0) {
                IssueHandler.Add(Issue.DuplicateModifier(modifierContext, modifierContext.start.Text));
                continue;
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