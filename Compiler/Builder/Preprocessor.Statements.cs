﻿namespace Compiler.Builder;

using static Grammar.ScrantonParser;
using Antlr4.Runtime;
using Analysis;
using Data;
using Interpreter.Types;

internal sealed partial class Preprocessor {
    /// <summary>
    /// Visit a type name.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The identifier of the type if it exists, null otherwise.</returns>
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
        TypeInfo? type = TypeHandler.TryGetByName(name);

        // stop if the type does not exist
        if (type is null) {
            IssueHandler.Add(Issue.UnrecognizedType(context, name));
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
        TypeInfo? type = TypeHandler.TryGetByName(name);

        // stop if the type does not exist
        if (type is null) {
            IssueHandler.Add(Issue.UnrecognizedType(context, name));
            return null;
        }

        // TODO no checks are performed for number of generic parameters

        // get generic parameter nodes
        TypeContext[] typeContexts = context.type();

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
    
    public override TypeIdentifier? VisitVariableWithType(VariableWithTypeContext context) {
        return VisitType(context.Type);
    }
    
    public override object? VisitVariableDeclaration(VariableDeclarationContext context) {
        TypeIdentifier? type = VisitVariableWithType(context.VariableWithType);

        if (type is null) {
            return null;
        }
        
        VisitExpression(context.Expression);

        context.Expression.FinalType = type;
        
        return null;
    }
    
    // TODO just here for debug
    public override object? VisitStatement(StatementContext context) {
        VisitChildren(context);

        DebugToTree(context);

        return null;

        void DebugToTree(ParserRuleContext? root, int depth = 0) {
            if (root is null) {
                return;
            }

            if (root is ExpressionContext) {
                Console.WriteLine($"{new string(' ', depth * 4)}{root}");
                depth++;
            }

            for (int i = 0; i < root.ChildCount; i++) {
                if (root.GetChild(i) is not ParserRuleContext child) {
                    continue;
                }

                if (root is ExpressionContext && child is not ExpressionContext) {
                    continue;
                }

                DebugToTree(child, depth);
            }
        }
    }
}