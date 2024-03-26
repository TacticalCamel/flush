namespace Compiler.Builder;

using static Grammar.ScrantonParser;
using Handlers;
using Grammar;
using Data;

internal sealed class Preprocessor(CompilerOptions options, IssueHandler issueHandler, TypeHandler typeHandler): ScrantonBaseVisitor<object?> {
    private CompilerOptions Options { get; } = options;
    private IssueHandler IssueHandler { get; } = issueHandler;
    private TypeHandler TypeHandler { get; } = typeHandler;
    
    public override object? VisitProgram(ProgramContext context) {
        return VisitChildren(context);
    }
    
    public override object? VisitProgramHeader(ProgramHeaderContext context) {
        VisitChildren(context);

        TypeHandler.LoadTypes();

        return null;
    }
    
    public override object? VisitProgramBody(ProgramBodyContext context) {
        return VisitChildren(context);
    }
    
    public override object? VisitModuleSegment(ModuleSegmentContext context) {
        return VisitChildren(context);
    }
    
    public override object? VisitModuleStatement(ModuleStatementContext context) {
        string name = VisitNamespace(context.Name);

        TypeHandler.SetModule(name);

        return null;
    }

    public override object? VisitImportSegment(ImportSegmentContext context) {
        return VisitChildren(context);
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