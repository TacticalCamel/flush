namespace Interpreter.Types;

public readonly struct TypeConnection(int parent, int child) {
    public readonly int Parent = parent;
    public readonly int Child = child;
}