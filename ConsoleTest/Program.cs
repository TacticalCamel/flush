namespace ConsoleTest;

using Compiler;

internal static class Program {
    private static void Main() {
        string code;
        
        try {
            code = File.ReadAllText("CodeSamples/example.sra");
        }
        catch (Exception e) {
            Console.WriteLine(e.Message);
            return;
        }
        
        CompilerService.Compile(code);
    }
}
