[assembly: CLSCompliant(false)]

namespace Compiler;

using Grammar;
using Visitor;
using Antlr4.Runtime;

public sealed class CompilerService(TextWriter? outputStream = null) {
	private TextWriter Out { get; } = outputStream ?? Console.Out;

	public void Compile(string code) {
		//CompilerSettings settings = ProcessCompilerOptions(args ?? Array.Empty<string>());
		
		ProcessSourceCode(code);
	}

	/*private CompilerSettings ProcessCompilerOptions(string[] options) {
		const string SHORT_NAME_PREFIX = "-";
		const string LONG_NAME_PREFIX = "--";
		
		CompilerSettings results = new() {
			IsStatic = false,
			TreatWarningsAsErrors = false,
			IgnoredWarningIds = [],
			IncludeMetaData = true,
			CompileToPlainText = false,
			OutputDirectory = "",
			SourceCode = ""
		};
		
		return results;
	}*/

	private void ProcessSourceCode(string code) {
		ScriptBuilder scriptBuilder = new();
		
		AntlrInputStream inputStream = new(code);
		ScrantonLexer lexer = new(inputStream);
		lexer.AddErrorListener(scriptBuilder);

		CommonTokenStream tokenStream = new(lexer);
		ScrantonParser parser = new(tokenStream);
		parser.AddErrorListener(scriptBuilder);

		ScrantonVisitor visitor = new(parser.program(), scriptBuilder);

		try {
			visitor.TraverseAst();
			Out.WriteLine("Successful compilation");
		}
		catch (OperationCanceledException) {
			Out.WriteLine("Compilation cancelled after error");
		}
		catch (Exception e) {
			Out.WriteLine($"Unexpected compilation error: {e}");
		}
		
		Out.WriteLine(scriptBuilder);
	}
}
