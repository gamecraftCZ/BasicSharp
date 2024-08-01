namespace BasicSharp.common;

public struct Position {
    public readonly int line;
    public readonly int column;

    public Position(int line, int column) {
        this.line = line;
        this.column = column;
    }

    public override string ToString() {
        return $"{line}:{column}";
    }
}
