namespace Compiler; 

[Flags]
public enum CompilerOptions: byte {
    None = 0x00,
    Static = 0x01,
    WarningsAsErrors = 0x02,
    IgnoreErrors = 0x04
}