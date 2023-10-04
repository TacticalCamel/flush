namespace ConsoleTest;

using Compiler;

internal static class Program {
    private static void Main() {
        string code = File.ReadAllText("CodeSamples/example.sra");
        TestParser.Parse(code);
    }
}
