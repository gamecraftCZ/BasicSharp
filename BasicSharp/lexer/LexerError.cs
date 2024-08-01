namespace BasicSharp.lexer;

public class LexerError : Exception {
    public int Line { get; }
    public int Column { get; }

    public LexerError(string message, int line, int column) : base(message) {
        Line = line;
        Column = column;
    }
}
