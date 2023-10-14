namespace Compiler.Analysis; 

using Antlr4.Runtime;

internal static class WarningFactory {
    public static CompilerWarning ManualImportsRedundantHint(ParserRuleContext rule) {
        return new CompilerWarning(WarningLevel.Hint, 101, "Auto imports is enabled, using manual imports is redundant", rule);
    }

    public static CompilerWarning AutoImportAlreadyEnabledWarning(ParserRuleContext rule) {
        return new CompilerWarning(WarningLevel.Warning,201, "Auto import is already enabled", rule);
    }
    
    public static CompilerWarning ModuleAlreadyImportedError(ParserRuleContext rule) {
        return new CompilerWarning(WarningLevel.Error, 301, "Module is already imported", rule);
    }
}