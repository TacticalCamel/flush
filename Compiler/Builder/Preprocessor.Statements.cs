namespace Compiler.Builder;

using Analysis;
using Data;
using Grammar;
using Interpreter.Types;

internal sealed partial class Preprocessor {
    /// <summary>
    /// Visit a type name.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The identifier of the type if it exists, null otherwise.</returns>
    public override TypeIdentifier? VisitType(ScrantonParser.TypeContext context) {
        return (TypeIdentifier?)Visit(context);
    }

    /// <summary>
    /// Visit a non-generic type.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The identifier of the type if it exists, null otherwise.</returns>
    public override TypeIdentifier? VisitSimpleType(ScrantonParser.SimpleTypeContext context) {
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
    public override TypeIdentifier? VisitGenericType(ScrantonParser.GenericTypeContext context) {
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
        ScrantonParser.TypeContext[] typeContexts = context.type();

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