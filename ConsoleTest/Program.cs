namespace ConsoleTest;

using Compiler;

internal static class Program {
    private static void Main() {
        string code = File.ReadAllText("../../../../Code examples/example.sra");

        TestParser.Parse(code);
        
    }
}
