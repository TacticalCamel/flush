namespace ConsoleTest;

using Compiler;

internal static class Program {
    private static void Main() {
        string code;
        
        try {
            code = File.ReadAllText("CodeSamples/debug.sra");
        }
        catch (Exception e) {
            Console.WriteLine(e.Message);
            return;
        }
        
        CompilerService.Compile(code);
    }
}
