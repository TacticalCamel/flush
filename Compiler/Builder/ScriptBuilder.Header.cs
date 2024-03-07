namespace Compiler.Builder;

using Analysis;
using static Grammar.ScrantonParser;

internal sealed partial class ScriptBuilder {
    public override object? VisitProgramHeader(ProgramHeaderContext context) {
        VisitChildren(context);
        
        ClassLoader.LoadModules(ImportHandler.GetVisibleModules(), ImportHandler.AutoImportEnabled);
        
        return null;
    }
    
    public override object? VisitModuleStatement(ModuleStatementContext context){
        ImportHandler.Module = context.Name.Start.Text;
        
        return null;
    }
    
    public override object? VisitManualImport(ManualImportContext context) {
        string name = context.Name.Start.Text;

        if (ImportHandler.Module != name) {
            bool success = ImportHandler.Imports.Add(name);

            if (success) {
                return null;
            }
        }

        Warnings.Add(Warning.ModuleAlreadyImported(context, name));
        return null;
    }
    
    public override object? VisitAutoImport(AutoImportContext context){
        if (ImportHandler.AutoImportEnabled){
            Warnings.Add(Warning.AutoImportAlreadyEnabled(context));
        }
        else {
            ImportHandler.AutoImportEnabled = true;
        }

        return null;
    }

    public override object? VisitInParameters(InParametersContext context){
        // TODO not implemented 
        Warnings.Add(Warning.FeatureNotImplemented(context, "in parameters"));
        
        return null;
    }
    
    public override object? VisitOutParameters(OutParametersContext context){
        // TODO not implemented 
        Warnings.Add(Warning.FeatureNotImplemented(context, "out parameters"));
        
        return null;
    }
}