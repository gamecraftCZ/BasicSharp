using BasicSharp.common;

namespace BasicSharp.parser;

public class ParserError : Exception {
    public readonly Position position;

    public ParserError(string message, int line, int column) : base(message) {
        position = new Position(line, column);
    }
    public ParserError(string message, Position position) : base(message) {
        this.position = position;
    }

    public override string ToString() {
        return $"Parser error (at {position}): {Message}";
    }
}
