namespace Compiler.Visitor;

using Analysis;
using Grammar;
using static Grammar.ScrantonParser;

internal sealed class ScrantonVisitor(ScriptBuilder scriptBuilder) : ScrantonBaseVisitor<object?> {
    public ScriptBuilder ScriptBuilder { get; } = scriptBuilder;

    public void TraverseAst(ProgramContext context){
        
        VisitProgram(context);
    }

    public void Abort(){
        throw new OperationCanceledException();
    }
    
    public override object? VisitModuleSegment(ModuleSegmentContext context) {
        ScriptBuilder.ModuleName = context.Name.Text;
        return null;
    }

    public override object? VisitImportSegment(ImportSegmentContext context) {
        if (ScriptBuilder is { AutoImportEnabled: true, ImportedModules.Count: > 0 }) {
            ScriptBuilder.Warnings.Add(WarningFactory.ManualImportsRedundantHint(context));
        }
        
        return base.VisitImportSegment(context);;
    }

    public override object? VisitAutoImport(AutoImportContext context) {
        if (ScriptBuilder.AutoImportEnabled) {
            ScriptBuilder.Warnings.Add(WarningFactory.AutoImportAlreadyEnabledWarning(context));
        }
        else {
            ScriptBuilder.AutoImportEnabled = true;
        }
        
        return null;
    }

    public override object? VisitManualImport(ManualImportContext context) {
        bool success = ScriptBuilder.ImportedModules.Add(context.Name.Text);
        
        if (!success) {
            ScriptBuilder.Warnings.Add(WarningFactory.ModuleAlreadyImportedWarning(context));
        }
        
        return null;
    }

    public override object? VisitInParameters(InParametersContext context) {
        VarWithTypeContext[]? parameters = context.ParameterList?.varWithType();
        if(parameters is null) return null;

        foreach (VarWithTypeContext parameter in parameters) {
            ScriptBuilder.Instructions.Add($"DECL {parameter.Name.Text}:{parameter.Type.GetText()}, VALUE args[\"{parameter.Name.Text}\"]");
        }
        
        return base.VisitInParameters(context);
    }

    public override object? VisitOutParameters(OutParametersContext context) {
        VarWithTypeContext[]? parameters = context.ParameterList?.varWithType();
        if(parameters is null) return null;

        foreach (VarWithTypeContext parameter in parameters) {
            ScriptBuilder.Instructions.Add($"DECL {parameter.Name.Text}:{parameter.Type.GetText()}, VALUE null");
        }
        
        return base.VisitOutParameters(context);
    }

    public override object? VisitFunctionDefinition(FunctionDefinitionContext context) {
        ScriptBuilder.Instructions.Add($"SKIP FUNC DEF {context.varWithType().Name.Text}");
        return null;
    }

    public override object? VisitClassDefinition(ClassDefinitionContext context) {
        ScriptBuilder.Instructions.Add($"SKIP TYPE DEF {context.Header.Name.Text}");
        return null;
    }
    
    public override object? VisitVariableDeclaration(VariableDeclarationContext context){
        ScriptBuilder.Instructions.Add($"SKIP DECL {context.GetText()}");
        return null;
    }

    public override object? VisitExpression(ExpressionContext context){
        ScriptBuilder.Instructions.Add($"SKIP EXPR {context.GetText()}");
        return null;
    }

    public override object? VisitControlStatement(ControlStatementContext context) {
        ScriptBuilder.Instructions.Add($"SKIP CTR_ST {context.GetText()}");
        return null;
    }

    public override object? VisitBlock(BlockContext context) {
        ScriptBuilder.Instructions.Add("SKIP BLOCK");
        return null;
    }

    public override object? VisitIfBlock(IfBlockContext context) {
        ScriptBuilder.Instructions.Add("SKIP IF BLOCK");
        return null;
    }

    public override object? VisitForBlock(ForBlockContext context) {
        ScriptBuilder.Instructions.Add("SKIP FOR BLOCK");
        return null;
    }

    public override object? VisitWhileBlock(WhileBlockContext context) {
        ScriptBuilder.Instructions.Add("SKIP WHILE BLOCK");
        return null;
    }

    public override object? VisitTryBlock(TryBlockContext context) {
        ScriptBuilder.Instructions.Add("SKIP TRY BLOCK");
        return null;
    }
}