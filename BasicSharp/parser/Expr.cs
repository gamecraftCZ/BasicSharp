using BasicSharp.common;

namespace BasicSharp.parser;

public interface IExprVisitor<T> {
    T visitUnaryExpr(UnaryExpr expr);
    T visitBinaryExpr(BinaryExpr expr);
    T visitGroupingExpr(GroupingExpr expr);
    T visitLiteralExpr(LiteralExpr expr);
    T visitVarExpr(VarExpr expr);
}

/// <summary>
/// Expression is a part of the Abstract Syntax Tree (AST) that does something without returning a value.
/// </summary>
public abstract class Expr {
    public readonly Position pos;
    public abstract T accept<T>(IExprVisitor<T> visitor);
    protected Expr(Position pos) { this.pos = pos; }
}

public enum UnaryOperator {
    MINUS,
    NOT
}
public class UnaryExpr : Expr {
    public readonly UnaryOperator operatorType;
    public readonly Expr right;

    public UnaryExpr(Position positionInCode, UnaryOperator @operator, Expr right) : base(positionInCode) {
        operatorType = @operator;
        this.right = right;
    }

    public override T accept<T>(IExprVisitor<T> visitor) {
        return visitor.visitUnaryExpr(this);
    }
}

public enum BinaryOperator {
    PLUS,
    MINUS,
    STAR,
    SLASH,
    PERCENT,
    EQUAL_EQUAL,
    BANG_EQUAL,
    GREATER,
    GREATER_EQUAL,
    LESS,
    LESS_EQUAL,
    AND,
    OR
}
public class BinaryExpr : Expr {
    public readonly Expr left;
    public readonly BinaryOperator operatorType;
    public readonly Expr right;

    public BinaryExpr(Position positionInCode, Expr left, BinaryOperator @operator, Expr right) : base(positionInCode) {
        this.left = left;
        operatorType = @operator;
        this.right = right;
    }

    public override T accept<T>(IExprVisitor<T> visitor) {
        return visitor.visitBinaryExpr(this);
    }
}

public class GroupingExpr : Expr {
    public readonly Expr expression;

    public GroupingExpr(Position positionInCode, Expr expression) : base(positionInCode) {
        this.expression = expression;
    }

    public override T accept<T>(IExprVisitor<T> visitor) {
        return visitor.visitGroupingExpr(this);
    }
}

public class LiteralExpr : Expr {
    public readonly object value;

    public LiteralExpr(Position positionInCode, object value) : base(positionInCode) {
        this.value = value;
    }

    public override T accept<T>(IExprVisitor<T> visitor) {
        return visitor.visitLiteralExpr(this);
    }
}

public class VarExpr : Expr {
    public readonly string varName;

    public VarExpr(Position positionInCode, string varName) : base(positionInCode) {
        this.varName = varName;
    }

    public override T accept<T>(IExprVisitor<T> visitor) {
        return visitor.visitVarExpr(this);
    }
}
