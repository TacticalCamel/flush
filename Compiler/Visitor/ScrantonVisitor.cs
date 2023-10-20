namespace Compiler.Visitor;

using Analysis;
using Grammar;
using static Grammar.ScrantonParser;

internal sealed class ScrantonVisitor: ScrantonBaseVisitor<object?> {
    public Script Script { get; } = new();
    
    public override object? VisitModuleSegment(ModuleSegmentContext context) {
        Script.ModuleName = context.Name?.Text;

        return null;
    }

    public override object? VisitImportSegment(ImportSegmentContext context) {
        if (Script is { AutoImportEnabled: true, ImportedModules.Count: > 0 }) {
            Script.Warnings.Add(WarningFactory.ManualImportsRedundantHint(context));
        }
        
        return base.VisitImportSegment(context);;
    }

    public override object? VisitAutoImport(AutoImportContext context) {
        if (Script.AutoImportEnabled) {
            Script.Warnings.Add(WarningFactory.AutoImportAlreadyEnabledWarning(context));
        }
        else {
            Script.AutoImportEnabled = true;
        }
        
        return null;
    }

    public override object? VisitManualImport(ManualImportContext context) {
        bool success = Script.ImportedModules.Add(context.Name.Text);
        
        if (!success) {
            Script.Warnings.Add(WarningFactory.ModuleAlreadyImportedWarning(context));
        }
        
        return null;
    }

    public override object? VisitInParameters(InParametersContext context) {
        VarWithTypeContext[] v = context.ParameterList.varWithType();
        
        foreach(var vv in v) Script.Instructions.Add($"DECL {vv.Type.GetText()} {vv.Name.Text}");
        
        return base.VisitInParameters(context);
    }

    public override object? VisitOutParameters(OutParametersContext context) {
        VarWithTypeContext[] v = context.ParameterList.varWithType();
        
        foreach(var vv in v) Script.Instructions.Add($"DECL {vv.Type.GetText()} {vv.Name.Text}");
        
        return base.VisitOutParameters(context);
    }

    public override object? VisitRegularStatement(RegularStatementContext context) {
        Script.Instructions.Add($"ST {context.GetText()}");
        
        return base.VisitRegularStatement(context);
    }

    public override object? VisitControlStatement(ControlStatementContext context) {
        return base.VisitControlStatement(context);
    }

    public override object? VisitBlockStatement(BlockStatementContext context) {
        return base.VisitBlockStatement(context);
    }
    
    
}