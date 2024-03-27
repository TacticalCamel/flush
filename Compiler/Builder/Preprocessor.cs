namespace Compiler.Builder;

using static Grammar.ScrantonParser;
using Handlers;
using Grammar;
using Analysis;

/// <summary>
/// Implements the first pass traversal of the AST. This loads types, methods and
/// determines the actual types of expressions, but not yet emits any instructions.
/// </summary>
/// <param name="options">The compiler options to use.</param>
/// <param name="issueHandler">The issue handler to use.</param>
/// <param name="typeHandler">The type handler to use.</param>
/// <param name="dataHandler">The data handler to use.</param>
internal sealed partial class Preprocessor(CompilerOptions options, IssueHandler issueHandler, TypeHandler typeHandler, DataHandler dataHandler) : ScrantonBaseVisitor<object?> {
    private CompilerOptions Options { get; } = options;
    private IssueHandler IssueHandler { get; } = issueHandler;
    private TypeHandler TypeHandler { get; } = typeHandler;
    private DataHandler DataHandler { get; } = dataHandler;

    // TODO temporary
    private static void DebugNode(ExpressionContext context) {
        Console.WriteLine($"{context.GetType().Name.Replace("Context", ""),-33} {context.GetText(),-20} {context.Result}");
    }

    public override object? VisitProgramHeader(ProgramHeaderContext context) {
        VisitChildren(context);

        TypeHandler.LoadTypes();

        return null;
    }

    public override object? VisitModuleStatement(ModuleStatementContext context) {
        string name = VisitNamespace(context.Name);

        TypeHandler.SetModule(name);

        return null;
    }

    public override object? VisitImportStatement(ImportStatementContext context) {
        // subtypes must be visited
        return Visit(context);
    }

    public override object? VisitManualImport(ManualImportContext context) {
        string name = VisitNamespace(context.Name);

        bool success = TypeHandler.Add(name);

        if (!success) {
            IssueHandler.Add(Issue.ModuleAlreadyImported(context, name));
        }

        return null;
    }

    public override object? VisitAutoImport(AutoImportContext context) {
        bool success = TypeHandler.EnableAutoImport();

        if (!success) {
            IssueHandler.Add(Issue.AutoImportAlreadyEnabled(context));
        }

        return null;
    }

    public override string VisitNamespace(NamespaceContext context) {
        return context.GetText();
    }

    public override string VisitId(IdContext context) {
        return context.Start.Text;
    }

    public override string VisitContextualKeyword(ContextualKeywordContext context) {
        return context.start.Text;
    }
}