namespace Compiler.Visitor;

using System.Text;
using Antlr4.Runtime;
using Analysis;

internal sealed class ScriptBuilder: IAntlrErrorListener<IToken>, IAntlrErrorListener<int> {
    #region Imports
    
    private string? Module { get; set; }
    private bool AutoImportEnabled{ get; set; }
    private HashSet<string> ModuleImports { get; } = [];
    
    public void SetModule(string module){
        Module = module;
    }

    public bool AddImport(string module){
        return ModuleImports.Add(module);
    }

    public bool EnableAutoImports(){
        bool alreadyEnabled = AutoImportEnabled;
        AutoImportEnabled = true;
        return !alreadyEnabled;
    }
    
    #endregion
    
    #region Warnings
    
    private List<Warning> Warnings { get; } = [];
    
    public void AddWarning(WarningType type, ParserRuleContext rule, string? message = null){
        Warnings.Add(new Warning(type, new Position(rule.start.Line, rule.start.Column), new Position(rule.stop.Line, rule.stop.Column), message));
    }
    
    public void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e) {
        Warning warning = new(WarningType.ParserInputMismatch, new Position(line, charPositionInLine), new Position(line, charPositionInLine), msg);
        Warnings.Add(warning);
    }

    public void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e) {
        Warning warning = new(WarningType.LexerTokenInvalid, new Position(line, charPositionInLine), new Position(line, charPositionInLine), msg);
        Warnings.Add(warning);
    }

    public bool HasWarningsWithLevel(WarningLevel level) {
        return Warnings.Any(x => x.Type.Level >= level);
    }
    
    #endregion

    #region Variables

    private ScopeTracker ScopeTracker{ get; } = new();
    private VariableTracker VariableTracker { get; } = new();
    
    

    #endregion
    
    public override string ToString(){
        StringBuilder sb = new();
        
        sb.AppendLine("Warnings: {");
        foreach (Warning warning in Warnings) sb.AppendLine($"    {warning}");
        sb.AppendLine("}");
        
        sb.AppendLine("Module: {");
        sb.AppendLine($"    {Module}");
        sb.AppendLine("}");
        
        sb.AppendLine($"Imports: (auto = {(AutoImportEnabled ? "true" : "false")}) {{");
        foreach (string module in ModuleImports) sb.AppendLine($"    {module}");
        sb.AppendLine("}");

        return sb.ToString();
    }
}