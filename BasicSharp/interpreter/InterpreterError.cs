using BasicSharp.common;

namespace BasicSharp.interpreter;

public class InterpreterError : Exception {
    public readonly Position position;

    public InterpreterError(string message, int line, int column) : base(message) {
        position = new Position(line, column);
    }
    public InterpreterError(string message, Position position) : base(message) {
        this.position = position;
    }

    public override string ToString() {
        return $"Interpreter error (at {position}): {Message}";
    }
}
