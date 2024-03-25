namespace Compiler.Data;

/// <summary>
/// Represents a character position within a file
/// </summary>
/// <param name="line">The line number of the file</param>
/// <param name="column">The column number of the line</param>
internal readonly struct FilePosition(int line, int column): IComparable<FilePosition> {
    /// <summary> The line number of the file, indexed from 1 </summary>
    public int Line { get; } = line;
    
    /// <summary> The index of the position within the line, indexed from 0 </summary>
    public int Column { get; } = column;

    public override string ToString() {
        return $"({Line},{Column})";
    }
    
    public int CompareTo(FilePosition other) {
        return Line - other.Line == 0 ? Column - other.Column : Line - other.Line;
    }
}