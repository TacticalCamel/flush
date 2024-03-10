namespace Compiler.Analysis; 

internal readonly struct Position(int line, int column): IComparable<Position> {
    public int Line { get; } = line;
    public int Column { get; } = column;

    public override string ToString() {
        return $"({Line},{Column})";
    }
    
    public int CompareTo(Position other) {
        return Line - other.Line == 0 ? Column - other.Column : Line - other.Line;
    }
}