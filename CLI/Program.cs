namespace CLI;

using System.CommandLine;
using Commands;

/// <summary>
/// The starting point of the application, interacts with the public interfaces of the compiler and the interpreter.
/// </summary>
internal static class Program {
    /// <summary>
    /// The entry point of the application.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns>The exit code of the application.</returns>
    private static int Main(string[] args) {
        RootCommand root = new(description: "Root command.") {
            new BuildCommand(),
            new RunCommand()
        };

        return root.Invoke(args);
    }
}