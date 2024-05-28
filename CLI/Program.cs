namespace CLI;

using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Parsing;
using Commands;

internal static class Program {
    /// <summary>
    /// The entry point of the application.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns>The exit code of the application.</returns>
    private static int Main(string[] args) {
        // create the root command
        RootCommand root = new(description: "Root command.") {
            new BuildCommand(),
            new RunCommand()
        };

        // override settings
        Parser parser = new CommandLineBuilder(root)
            .UseDefaults()
            .UseHelp(customize: helpContext => {
                root.Name = "flush";
                helpContext.HelpBuilder.CustomizeLayout(_ => HelpBuilder.Default.GetLayout().Skip(1));
            })
            .Build();

        // invoke with the current command line arguments
        return parser.Invoke(args);
    }
}