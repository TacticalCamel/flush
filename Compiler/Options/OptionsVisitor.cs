namespace Compiler.Options;

internal sealed class OptionsVisitor: CompilerOptionsBaseVisitor<object?> {
	public override object? VisitArguments(CompilerOptionsParser.ArgumentsContext context) {
		return base.VisitArguments(context);
	}
	
	public override object? VisitOption(CompilerOptionsParser.OptionContext context) {
		return base.VisitOption(context);
	}
	
	public override object? VisitBoolOption(CompilerOptionsParser.BoolOptionContext context) {
		return base.VisitBoolOption(context);
	}

	public override object? VisitStringOption(CompilerOptionsParser.StringOptionContext context) {
		return base.VisitStringOption(context);
	}

	public override object? VisitIntOption(CompilerOptionsParser.IntOptionContext context) {
		return base.VisitIntOption(context);
	}

	public override object? VisitKey(CompilerOptionsParser.KeyContext context) {
		return base.VisitKey(context);
	}
	
	public override object? VisitShortKey(CompilerOptionsParser.ShortKeyContext context) {
		return base.VisitShortKey(context);
	}

	public override object? VisitLongKey(CompilerOptionsParser.LongKeyContext context) {
		return base.VisitLongKey(context);
	}
}