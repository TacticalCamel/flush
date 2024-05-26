namespace CLI.Options;

public class OptionsRoot {
    [Display(Name = "help", ShortName = "h", Description = "Show command line help.")]
    public bool ShowHelp { get; init; }
    
    [Display(Name = "version", Description = "Show language version.")]
    public bool ShowVersion { get; init; }
}

/*
.fl -> .flc / .txt

flush --help
flush --version
flush new
flush run
flush build



*/