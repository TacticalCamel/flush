namespace Compiler.Builder;

using Analysis;
using Data;
using Types;
using Interpreter;
using Interpreter.Types;
using Interpreter.Structs;
using System.Diagnostics;
using static Grammar.FlushParser;

// ScriptBuilder.Types: methods related to visiting type definitions and type names
internal sealed partial class ScriptBuilder {
    /// <summary>
    /// Process a collection of type definitions.
    /// </summary>
    /// <param name="typeDefinitions">The type definitions.</param>
    private void ProcessTypeDefinitions(TypeDefinitionContext[] typeDefinitions) {
        // the steps of processing a type
        Func<TypeDefinitionContext, bool>[] steps = [
            CreateDraft,
            CreateMemberDrafts,
            LoadType
        ];

        // set processed types
        ContextHandler.ProcessedTypes = typeDefinitions;

        // call each step on every type
        foreach (Func<TypeDefinitionContext, bool> step in steps) {
            bool success = true;

            // process each type definition
            foreach (TypeDefinitionContext typeDefinition in typeDefinitions) {
                success &= step(typeDefinition);
            }

            // if failed, stop type processing
            if (!success) {
                return;
            }
        }

        // finished type processing
        ContextHandler.ProcessedTypes = null;
    }

    /// <summary>
    /// Create a type draft for a type definition.
    /// </summary>
    /// <param name="context">The type definition to visit.</param>
    /// <remarks>True if the operation was successful, false otherwise.</remarks>
    private bool CreateDraft(TypeDefinitionContext context) {
        // the modifiers of the type
        if (VisitModifierList(context.Modifiers) is not Modifier modifiers) {
            return false;
        }

        // whether the type is a reference type
        bool isReference = context.Keyword.Type == KW_CLASS;

        // the name of the type
        string name = VisitId(context.TypeName);

        // the unique id of the type
        Guid id = ClassLoader.GenerateTypeId(TypeHandler.ProgramModule, name);

        // the name of the generic parameters
        string[]? genericParameterNames = VisitGenericParameters(context.GenericParameters);

        if (genericParameterNames is null) {
            return false;
        }

        // create the draft
        context.TypeDraft = new TypeDraft {
            Id = id,
            Modifiers = modifiers,
            IsReference = isReference,
            Name = name,
            GenericParameterNames = genericParameterNames
        };

        return true;
    }

    /// <summary>
    /// Creates drafts for the members of a type definition.
    /// </summary>
    /// <param name="context">The type definition to visit.</param>
    /// <returns>True if the operation was successful, false otherwise.</returns>
    private bool CreateMemberDrafts(TypeDefinitionContext context) {
        ContextHandler.GenericParameterNames = context.TypeDraft.GenericParameterNames;

        // process fields
        foreach (FieldDefinitionContext field in context.Body.fieldDefinition()) {
            FieldDraft? fieldDraft = CreateFieldDraft(field);

            if (fieldDraft is null) {
                return false;
            }

            context.TypeDraft.Fields.Add(fieldDraft);
        }

        // process methods
        foreach (MethodDefinitionContext method in context.Body.methodDefinition()) {
            MethodDraft? methodDraft = CreateMethodDraft(method);

            if (methodDraft is null) {
                return false;
            }

            context.TypeDraft.Methods.Add(methodDraft);
        }

        // process constructors
        foreach (ConstructorDefinitionContext constructor in context.Body.constructorDefinition()) {
            MethodDraft? constructorDraft = CreateConstructorDraft(constructor, context.TypeDraft.Id);

            if (constructorDraft is null) {
                return false;
            }

            context.TypeDraft.Methods.Add(constructorDraft);
        }

        ContextHandler.GenericParameterNames = null;

        return true;

        FieldDraft? CreateFieldDraft(FieldDefinitionContext field) {
            // get modifiers
            if (VisitModifierList(field.Modifiers) is not Modifier modifiers) {
                return null;
            }

            // get type
            if (GetTypeNode(field.Type) is not { } type) {
                return null;
            }

            return new FieldDraft {
                Modifiers = modifiers,
                Type = type,
                Name = VisitId(field.Name)
            };
        }

        MethodDraft? CreateMethodDraft(MethodDefinitionContext method) {
            // get modifiers
            if (VisitModifierList(method.Modifiers) is not Modifier modifiers) {
                return null;
            }

            // get return type
            if (GetTypeNode(method.ReturnType) is not { } returnType) {
                return null;
            }

            // get parameter types
            VariableWithTypeContext[] parameters = method.ParameterList.variableWithType();
            TypeNode[] parameterTypes = new TypeNode[parameters.Length];

            for (int i = 0; i < parameters.Length; i++) {
                if (GetTypeNode(parameters[i].Type) is not { } parameterType) {
                    return null;
                }

                parameterTypes[i] = parameterType;
            }

            return new MethodDraft {
                Modifiers = modifiers,
                ReturnType = returnType,
                Name = VisitId(method.Name),
                ParameterTypes = parameterTypes
            };
        }

        MethodDraft? CreateConstructorDraft(ConstructorDefinitionContext constructor, Guid returnTypeId) {
            // get modifiers
            if (VisitModifierList(constructor.Modifiers) is not Modifier modifiers) {
                return null;
            }

            // get parameter types
            VariableWithTypeContext[] parameters = constructor.ParameterList.variableWithType();
            TypeNode[] parameterTypes = new TypeNode[parameters.Length];

            for (int i = 0; i < parameters.Length; i++) {
                if (GetTypeNode(parameters[i].Type) is not { } parameterType) {
                    return null;
                }

                parameterTypes[i] = parameterType;
            }

            return new MethodDraft {
                Modifiers = modifiers,
                ReturnType = new TypeNode {
                    GenericIndex = -1,
                    Id = returnTypeId
                },
                Name = ".ctor",
                ParameterTypes = parameterTypes
            };
        }
    }

    private bool LoadType(TypeDefinitionContext context) {
        ContextHandler.GenericParameterNames = context.TypeDraft.GenericParameterNames;
        
        TypeDefinition definition = new() {
            Id = context.TypeDraft.Id,
            Modifiers = context.TypeDraft.Modifiers,
            IsReference = context.TypeDraft.IsReference,
            Name = context.TypeDraft.Name,
            GenericParameterCount = context.TypeDraft.GenericParameterNames.Length,
            Fields = [],
            Methods = [],
            Size = 0
        };

        bool success = TypeHandler.RegisterTypeDefinition(definition);

        if (!success) {
            IssueHandler.Add(Issue.DuplicateTypeId(context, definition.Name));
            return false;
        }

        // debug

        Console.WriteLine($"{context.TypeDraft.Name}: {context.TypeDraft.Id}");

        foreach (FieldDraft field in context.TypeDraft.Fields) {
            Console.WriteLine($"    {field.Name}: {field.Type}");
        }

        Console.WriteLine();

        ContextHandler.GenericParameterNames = null;

        return true;
    }
    
    private TypeNode? GetTypeNode(TypeContext context) {
        string name = context switch {
            SimpleTypeContext simpleType => VisitId(simpleType.Name),
            GenericTypeContext genericType => VisitId(genericType.Name),
            ArrayTypeContext => "Array",
            _ => throw new UnreachableException()
        };

        TypeNode? node = null;

        int genericIndex = ContextHandler.GenericParameterNames is null ? -1 : Array.IndexOf(ContextHandler.GenericParameterNames, name);

        if (genericIndex >= 0) {
            node = new TypeNode {
                Id = default,
                GenericIndex = genericIndex
            };
        }

        TypeDefinitionContext? processedContext = ContextHandler.ProcessedTypes?.FirstOrDefault(x => x.TypeDraft.Name == name);

        if (processedContext != null) {
            node = new TypeNode {
                Id = processedContext.TypeDraft.Id,
                GenericIndex = -1
            };
        }

        if (node is null) {
            TypeDefinition? typeDefinition = TypeHandler.GetTypeByName(name);

            if (typeDefinition is null) {
                IssueHandler.Add(Issue.UnrecognizedType(context, name));
                return null;
            }

            node = new TypeNode {
                Id = typeDefinition.Id,
                GenericIndex = -1
            };
        }

        {
            if (context is not GenericTypeContext genericType) {
                return node;
            }

            foreach (TypeContext type in genericType.type()) {
                TypeNode? childNode = GetTypeNode(type);

                if (childNode is null) {
                    return null;
                }

                node.Children.Add(childNode);
            }

            return node;
        }
    }

    private TypeNode? GetTypeNode(ReturnTypeContext context) {
        if (context.Void is not null) {
            return new TypeNode {
                Id = TypeHandler.CoreTypes.Void.Definition.Id,
                GenericIndex = -1,
            };
        }

        return GetTypeNode(context.Type);
    }

    /// <summary>
    /// Visit a list of generic parameters.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The names of the parameters as an array of strings if successful, null otherwise.</returns>
    public override string[]? VisitGenericParameters(GenericParametersContext? context) {
        if (context is null) {
            return [];
        }

        string[] names = context.id().Select(VisitId).ToArray();

        if (names.Length != names.Distinct().Count()) {
            IssueHandler.Add(Issue.DuplicateTypeParameterName(context));
            return null;
        }

        return names;
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
    /// Visit a type name.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The type if it exists, null otherwise.</returns>
    public override TypeIdentifier? VisitType(TypeContext context) {
        return (TypeIdentifier?)Visit(context);
    }

    /// <summary>
    /// Visit a simple type name.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The type if it exists, null otherwise.</returns>
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

        return new TypeIdentifier(type, []);
    }

    /// <summary>
    /// Visit a generic type name.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The type if it exists, null otherwise.</returns>
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

    /// <summary>
    /// Visit an array type name.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The type if it exists, null otherwise.</returns>
    public override TypeIdentifier? VisitArrayType(ArrayTypeContext context) {
        // get the item type of the array
        TypeIdentifier? type = VisitType(context.Type);

        // stop if the type does not exist
        if (type is null) {
            IssueHandler.Add(Issue.UnrecognizedType(context, context.Type.GetText()));
            return null;
        }

        throw new NotImplementedException();
    }
}