namespace Compiler.Builder;

using Analysis;
using Data;
using Grammar;
using static Grammar.ScrantonParser;

internal sealed partial class ScriptBuilder: ScrantonBaseVisitor<object?> {
    #region program head

    public override object? VisitModuleStatement(ModuleStatementContext context){
        SetModule(context.Name.Start.Text);
        return null;
    }
    
    public override object? VisitManualImport(ManualImportContext context){
        bool success = AddImport(context.Name.Start.Text);

        if (!success){
            AddWarning(WarningType.ModuleAlreadyImported, context);
        }

        return null;
    }
    
    public override object? VisitAutoImport(AutoImportContext context){
        bool success = EnableAutoImports();
        
        if (!success){
            AddWarning(WarningType.AutoImportAlreadyEnabled, context);
        }

        return null;
    }

    public override object? VisitInParameters(InParametersContext context){
        AddWarning(WarningType.FeatureNotImplemented, context);
        
        VarWithTypeContext[]? parameters = context.ParameterList?.varWithType();
        if (parameters is null) return null;
        
        foreach (VarWithTypeContext parameter in parameters){
            //Console.WriteLine($"DECL {parameter.Name.GetText()}:{parameter.Type.GetText()}, VALUE args[\"{parameter.Name.GetText()}\"]");
        }

        return null;
    }
    
    public override object? VisitOutParameters(OutParametersContext context){
        AddWarning(WarningType.FeatureNotImplemented, context);
        
        VarWithTypeContext[]? parameters = context.ParameterList?.varWithType();
        if (parameters is null) return null;
        
        foreach (VarWithTypeContext parameter in parameters){
            //Console.WriteLine($"DECL {parameter.Name.GetText()}:{parameter.Type.GetText()}, VALUE args[\"{parameter.Name.GetText()}\"]");
        }

        return null;
    }

    #endregion

    #region Definitions

    public override object? VisitFunctionDefinition(FunctionDefinitionContext context) {
        AddWarning(WarningType.FeatureNotImplemented, context, "FunctionDefinition");
        return null;
    }

    public override object? VisitShortTypeDefinition(ShortTypeDefinitionContext context) {
        AddWarning(WarningType.FeatureNotImplemented, context, "ShortTypeDefinition");

        Modifier modifiers = (Modifier)VisitModifierList(context.Header.Modifiers)!;
        
        string name = context.Header.Name.start.Text;

        return null;
    }

    public override object? VisitModifierList(ModifierListContext context) {
        Modifier modifiers = Modifier.None;
        
        foreach (ModifierContext modifierContext in context.modifier()) {
            int type = modifierContext.start.Type;

            Modifier modifier = type switch {
                KW_PUBLIC => Modifier.Public,
                KW_PRIVATE => Modifier.Private,
                _ => Modifier.None
            };

            if ((modifiers & modifier) > 0) {
                AddWarning(WarningType.DuplicateModifier, modifierContext);
            }
            else {
                modifiers |= modifier;
            }
        }

        return modifiers;
    }

    public override object? VisitLongTypeDefinition(LongTypeDefinitionContext context) {
        AddWarning(WarningType.FeatureNotImplemented, context, "LongTypeDefinition");
        return null;
    }

    #endregion
    
    #region Statements

    public override object? VisitStatement(StatementContext context) {
        AddWarning(WarningType.FeatureNotImplemented, context, "Statement");
        return null;
    }

    #endregion
}