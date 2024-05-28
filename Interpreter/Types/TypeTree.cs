namespace Interpreter.Types;

using Structs;

public readonly struct TypeTree(ArrayReference<ATypeNode> nodes, ArrayReference<TypeConnection> connections) {
    public readonly ArrayReference<ATypeNode> Nodes = nodes;
    public readonly ArrayReference<TypeConnection> Connections = connections;
}