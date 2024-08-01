using BasicSharp.common;

namespace BasicSharp.lexer;

public class LexerError : Exception {
    public readonly Position position;

    public LexerError(string message, int line, int column) : base(message) {
        position = new Position(line, column);
    }
    public LexerError(string message, Position position) : base(message) {
        this.position = position;
    }

    public override string ToString() {
        return $"Lexer error (at {position}): {Message}";
    }
}
