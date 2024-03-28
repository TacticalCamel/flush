namespace Compiler.Builder;

using Analysis;
using static Grammar.ScrantonParser;

internal partial class Preprocessor {
    /// <summary>
    /// Visits the program header and loads available types before returning.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitProgramHeader(ProgramHeaderContext context) {
        VisitChildren(context);

        TypeHandler.LoadTypes();

        return null;
    }

    /// <summary>
    /// Visits the module statement and sets the module of the program. 
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitModuleStatement(ModuleStatementContext context) {
        string name = VisitNamespace(context.Name);

        TypeHandler.SetModule(name);

        return null;
    }

    /// <summary>
    /// Visits the subtype of an import statement.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitImportStatement(ImportStatementContext context) {
        Visit(context);

        return null;
    }

    /// <summary>
    /// Visits a general import statement and adds it to the loaded namespaces.
    /// Throws a warning if the namespace is already added.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitManualImport(ManualImportContext context) {
        string name = VisitNamespace(context.Name);

        bool success = TypeHandler.Add(name);

        if (!success) {
            IssueHandler.Add(Issue.ModuleAlreadyImported(context, name));
        }

        return null;
    }

    /// <summary>
    /// Visits an auto import statement and enables auto imports.
    /// Throws a warning if auto imports are already enabled.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitAutoImport(AutoImportContext context) {
        bool success = TypeHandler.EnableAutoImport();

        if (!success) {
            IssueHandler.Add(Issue.AutoImportAlreadyEnabled(context));
        }

        return null;
    }

    /// <summary>
    /// Visits a namespace and returns its value as a string.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The namespace as a string.</returns>
    public override string VisitNamespace(NamespaceContext context) {
        return context.GetText();
    }

    /// <summary>
    /// Visits an identifier and returns its value as a string.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The identifier as a string.</returns>
    public override string VisitId(IdContext context) {
        return context.Start.Text;
    }
}