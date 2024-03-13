namespace Compiler.Builder;

using Analysis;
using static Grammar.ScrantonParser;

internal sealed partial class ScriptBuilder {
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

    public override object? VisitManualImport(ManualImportContext context) {
        string name = VisitNamespace(context.Name);

        bool success = TypeHandler.Add(name);

        if (!success) {
            WarningHandler.Add(Warning.ModuleAlreadyImported(context, name));
        }

        return null;
    }

    public override object? VisitAutoImport(AutoImportContext context) {
        bool success = TypeHandler.EnableAutoImport();
        
        if (!success) {
            WarningHandler.Add(Warning.AutoImportAlreadyEnabled(context));
        }

        return null;
    }

    public override string VisitNamespace(NamespaceContext context) {
        return context.GetText();
    }

    public override string VisitId(IdContext context) {
        return context.Start.Text;
    }

    public override object? VisitInParameters(InParametersContext context) {
        // TODO not implemented 
        WarningHandler.Add(Warning.FeatureNotImplemented(context, "in parameters"));

        return null;
    }

    public override object? VisitOutParameters(OutParametersContext context) {
        // TODO not implemented 
        WarningHandler.Add(Warning.FeatureNotImplemented(context, "out parameters"));

        return null;
    }
}