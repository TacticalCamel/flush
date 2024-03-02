namespace Compiler.Builder;

using Analysis;
using Data;
using Grammar;
using Interpreter;
using Antlr4.Runtime.Tree;
using static Grammar.ScrantonParser;

internal sealed partial class ScriptBuilder {
    #region program head
    
    public override object? VisitModuleStatement(ModuleStatementContext context){
        ImportHandler.Module = context.Name.Start.Text;
        return null;
    }
    
    public override object? VisitManualImport(ManualImportContext context){
        bool success = ImportHandler.Imports.Add(context.Name.Start.Text);

        if (!success){
            AddWarning(WarningType.ModuleAlreadyImported, context);
        }

        return null;
    }
    
    public override object? VisitAutoImport(AutoImportContext context){
        if (ImportHandler.AutoImportEnabled){
            AddWarning(WarningType.AutoImportAlreadyEnabled, context);
        }
        else {
            ImportHandler.AutoImportEnabled = true;
        }

        return null;
    }

    public override object? VisitInParameters(InParametersContext context){
        // TODO not implemented 
        AddWarning(WarningType.FeatureNotImplemented, context, "in parameters");
        return null;
    }
    
    public override object? VisitOutParameters(OutParametersContext context){
        // TODO not implemented 
        AddWarning(WarningType.FeatureNotImplemented, context, "out parameters");
        return null;
    }

    #endregion

    #region Definitions

    public override object? VisitFunctionDefinition(FunctionDefinitionContext context) {
        // TODO not implemented 
        AddWarning(WarningType.FeatureNotImplemented, context, "function def");
        return null;
    }

    public override object? VisitTypeDefinition(TypeDefinitionContext context) {
        // TODO not implemented 
        AddWarning(WarningType.FeatureNotImplemented, context, "type def");
        return null;
    }
    
    #endregion
}