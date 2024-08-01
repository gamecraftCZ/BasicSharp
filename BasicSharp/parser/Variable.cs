namespace BasicSharp.interpreter;

public enum VariableType {
    NUMBER,
    STRING,
    BOOL,
}
public abstract class Variable {
    public VariableType Type { get; }
    public abstract object Value { get; }
    protected Variable(VariableType type) {
        Type = type;
    }
}

public class NumberVariable(double value) : Variable(VariableType.NUMBER) {
    public override object Value { get; } = value;

    public override string ToString() {
        // Print whole number as integer
        double val = (double)Value;
        if (Math.Abs(val % 1) < double.Epsilon * 100) {
            return $"{val:N0}";
        }

        // Else format using {:.2f} to print 2 decimal places
        return $"{val:0.00}";
    }
}

public class StringVariable(string value) : Variable(VariableType.STRING) {
    public override object Value { get; } = value;
    public override string ToString() { return (string)Value; }
}

public class BoolVariable(bool value) : Variable(VariableType.BOOL) {
    public override object Value { get; } = value;
    public override string ToString() { return (bool)Value ? "TRUE" : "FALSE"; }
}
