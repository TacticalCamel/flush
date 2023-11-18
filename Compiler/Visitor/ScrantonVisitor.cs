namespace Compiler.Visitor;

using Analysis;
using Grammar;
using static Grammar.ScrantonParser;

internal sealed class ScrantonVisitor(ProgramContext programContext) : ScrantonBaseVisitor<object?>{
    private ScriptBuilder ScriptBuilder{ get; } = new();

    public ScriptBuilder TraverseAst(){
        VisitProgram(programContext);
        return ScriptBuilder;
    }

    public override object? VisitModuleSegment(ModuleSegmentContext context){
        ScriptBuilder.SetModule(context.Name.GetText());
        return null;
    }
    
    public override object? VisitManualImport(ManualImportContext context){
        bool success = ScriptBuilder.AddImport(context.Name.GetText());

        if (!success){
            ScriptBuilder.AddWarning(WarningType.ModuleAlreadyImported, context);
        }

        return null;
    }
    
    public override object? VisitAutoImport(AutoImportContext context){
        bool success = ScriptBuilder.EnableAutoImports();
        
        if (!success){
            ScriptBuilder.AddWarning(WarningType.AutoImportAlreadyEnabled, context);
        }

        return null;
    }

    public override object? VisitInParameters(InParametersContext context){
        VarWithTypeContext[]? parameters = context.ParameterList?.varWithType();
        if (parameters is null) return null;
        
        foreach (VarWithTypeContext parameter in parameters){
            //ScriptBuilder.Instructions.Add($"DECL {parameter.Name.Text}:{parameter.Type.GetText()}, VALUE args[\"{parameter.Name.Text}\"]");
        }

        return null;
    }
    
    public override object? VisitOutParameters(OutParametersContext context){
        VarWithTypeContext[]? parameters = context.ParameterList?.varWithType();
        if (parameters is null) return null;

        foreach (VarWithTypeContext parameter in parameters){
            //ScriptBuilder.Instructions.Add($"DECL {parameter.Name.Text}:{parameter.Type.GetText()}, VALUE null");
        }

        return null;
    }
}