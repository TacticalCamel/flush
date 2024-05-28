namespace Interpreter.Structs;

public readonly struct BytecodeVersion(ushort major = 1, ushort minor = 0, ushort build = 0, ushort revision = 0) {
    private readonly ushort Major = major;
    private readonly ushort Minor = minor;
    private readonly ushort Build = build;
    private readonly ushort Revision = revision;
    
    public static bool operator ==(BytecodeVersion left, BytecodeVersion right) {
        return left.Major == right.Major && left.Minor == right.Minor && left.Build == right.Build && left.Revision == right.Revision;
    }

    public static bool operator !=(BytecodeVersion left, BytecodeVersion right) {
        return !(left == right);
    }
    
    public override bool Equals(object? obj) {
        return obj is BytecodeVersion other && this == other;
    }

    public override int GetHashCode() {
        return HashCode.Combine(Major, Minor, Build, Revision);
    }

    public override string ToString() {
        return $"{Major}.{Minor}.{Build}.{Revision}";
    }
}