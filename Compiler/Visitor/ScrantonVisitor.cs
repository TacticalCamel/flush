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

    #region program head

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
        ScriptBuilder.AddWarning(WarningType.FeatureNotImplemented, context);
        
        VarWithTypeContext[]? parameters = context.ParameterList?.varWithType();
        if (parameters is null) return null;
        
        foreach (VarWithTypeContext parameter in parameters){
            Console.WriteLine($"DECL {parameter.Name.GetText()}:{parameter.Type.GetText()}, VALUE args[\"{parameter.Name.GetText()}\"]");
        }

        return null;
    }
    
    public override object? VisitOutParameters(OutParametersContext context){
        ScriptBuilder.AddWarning(WarningType.FeatureNotImplemented, context);
        return null;
        
        VarWithTypeContext[]? parameters = context.ParameterList?.varWithType();
        if (parameters is null) return null;

        foreach (VarWithTypeContext parameter in parameters){
            //ScriptBuilder.Instructions.Add($"DECL {parameter.Name.Text}:{parameter.Type.GetText()}, VALUE null");
        }

        return null;
    }

    #endregion

    
    public override object? VisitFunctionDefinition(FunctionDefinitionContext context){
        ScriptBuilder.AddWarning(WarningType.FeatureNotImplemented, context);
        return null;
    }

    public override object? VisitTypeDefinition(TypeDefinitionContext context){
        ScriptBuilder.AddWarning(WarningType.FeatureNotImplemented, context);
        return null;
    }

    public override object? VisitVariableDeclaration(VariableDeclarationContext context){
        ScriptBuilder.AddWarning(WarningType.FeatureNotImplemented, context);
        return null;
    }

    public override object? VisitExpression(ExpressionContext context){
        ScriptBuilder.AddWarning(WarningType.FeatureNotImplemented, context);
        return null;
    }

    public override object? VisitControlStatement(ControlStatementContext context){
        ScriptBuilder.AddWarning(WarningType.FeatureNotImplemented, context);
        return null;
    }

    public override object? VisitBlockStatement(BlockStatementContext context){
        ScriptBuilder.AddWarning(WarningType.FeatureNotImplemented, context);
        return null;
    }

    public override object? VisitConstantExpression(ConstantExpressionContext context){
        return base.VisitConstantExpression(context);
    }

    public override object? VisitFunctionCallExpression(FunctionCallExpressionContext context){
        return base.VisitFunctionCallExpression(context);
    }

    public override object? VisitLambdaExpression(LambdaExpressionContext context){
        return base.VisitLambdaExpression(context);
    }

    public override object? VisitObjectConstructorExpression(ObjectConstructorExpressionContext context){
        return base.VisitObjectConstructorExpression(context);
    }

    public override object? VisitCollectionConstructorExpression(CollectionConstructorExpressionContext context){
        return base.VisitCollectionConstructorExpression(context);
    }

    public override object? VisitIdentifierExpression(IdentifierExpressionContext context){
        return base.VisitIdentifierExpression(context);
    }

    public override object? VisitNestedExpression(NestedExpressionContext context){
        return base.VisitNestedExpression(context);
    }

    public override object? VisitMemberAccessOperatorExpression(MemberAccessOperatorExpressionContext context){
        return base.VisitMemberAccessOperatorExpression(context);
    }

    public override object? VisitLeftUnaryOperatorExpression(LeftUnaryOperatorExpressionContext context){
        return base.VisitLeftUnaryOperatorExpression(context);
    }

    public override object? VisitRightUnaryOperatorExpression(RightUnaryOperatorExpressionContext context){
        return base.VisitRightUnaryOperatorExpression(context);
    }

    public override object? VisitMultiplicativeOperatorExpression(MultiplicativeOperatorExpressionContext context){
        return base.VisitMultiplicativeOperatorExpression(context);
    }

    public override object? VisitAdditiveOperatorExpression(AdditiveOperatorExpressionContext context){
        return base.VisitAdditiveOperatorExpression(context);
    }

    public override object? VisitShiftOperatorExpression(ShiftOperatorExpressionContext context){
        return base.VisitShiftOperatorExpression(context);
    }

    public override object? VisitComparisonOperatorExpression(ComparisonOperatorExpressionContext context){
        return base.VisitComparisonOperatorExpression(context);
    }

    public override object? VisitLogicalOperatorExpression(LogicalOperatorExpressionContext context){
        return base.VisitLogicalOperatorExpression(context);
    }

    public override object? VisitAssigmentOperatorExpression(AssigmentOperatorExpressionContext context){
        return base.VisitAssigmentOperatorExpression(context);
    }
}