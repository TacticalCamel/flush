namespace Compiler.Visitor;

using Analysis;
using Grammar;
using static Grammar.ScrantonParser;

internal sealed class ScrantonVisitor: ScrantonBaseVisitor<object?> {
    private string? ModuleName { get; set; } = null;
    private HashSet<string> ImportedModules { get; } = new();
    private bool AutoImportEnabled { get; set; } = false;
    public List<CompilerWarning> Messages { get; } = new();
    public List<string> Instructions { get; } = new();

    public override object? VisitModuleSegment(ModuleSegmentContext context) {
        ModuleName = context.Name?.Text;

        return null;
    }

    public override object? VisitImportSegment(ImportSegmentContext context) {
        base.VisitImportSegment(context);

        if (AutoImportEnabled && ImportedModules.Count > 0) {
            Messages.Add(WarningFactory.ManualImportsRedundantHint(context));
        }
        
        return null;
    }

    public override object? VisitAutoImport(AutoImportContext context) {
        if(AutoImportEnabled) Messages.Add(WarningFactory.AutoImportAlreadyEnabledWarning(context));
        else AutoImportEnabled = true;
        
        return null;
    }

    public override object? VisitManualImport(ManualImportContext context) {
        bool success = ImportedModules.Add(context.Name.Text);
        if(!success) Messages.Add(WarningFactory.ModuleAlreadyImportedError(context));
        
        return null;
    }

    public override object? VisitInParameters(InParametersContext context) {
        VarWithTypeContext[] v = context.ParameterList.varWithType();
        
        foreach(var vv in v) Instructions.Add($"DECL {vv.Type.Text} {vv.Name.Text}");
        
        return base.VisitInParameters(context);
    }

    public override object? VisitOutParameters(OutParametersContext context) {
        VarWithTypeContext[] v = context.ParameterList.varWithType();
        
        foreach(var vv in v) Instructions.Add($"DECL {vv.Type.Text} {vv.Name.Text}");
        
        return base.VisitOutParameters(context);
    }

    public override object? VisitRegularStatement(RegularStatementContext context) {
        Instructions.Add($"ST {context.GetText()}");
        
        return base.VisitRegularStatement(context);
    }

    public override object? VisitControlStatement(ControlStatementContext context) {
        return base.VisitControlStatement(context);
    }

    public override object? VisitBlockStatement(BlockStatementContext context) {
        return base.VisitBlockStatement(context);
    }
}