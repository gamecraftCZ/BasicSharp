using BasicSharp.common;

namespace BasicSharp.parser;

public interface IStmtVisitor<T> {
    T visitPrintStmt(PrintStmt stmt);
    T visitInputStmt(InputStmt stmt);
    T visitLetStmt(LetStmt stmt);
    T visitToNumStmt(ToNumStmt stmt);
    T visitToStrStmt(ToStrStmt stmt);
    T visitRndStmt(RndStmt stmt);
    T visitBlockStmt(BlockStmt stmt);
    T visitIfStmt(IfStmt stmt);
    T visitWhileStmt(WhileStmt stmt);
    T visitContinueStmt(ContinueStmt stmt);
    T visitBreakStmt(BreakStmt stmt);
}

/// <summary>
/// Expression is a part of the Abstract Syntax Tree (AST) that returns a Literal (value).
/// </summary>
public abstract class Stmt {
    public readonly Position pos;
    public abstract T accept<T>(IStmtVisitor<T> visitor);
    protected Stmt(Position pos) { this.pos = pos; }
}

public class PrintStmt : Stmt {
    public readonly Expr expr;

    public PrintStmt(Position positionInCode, Expr expression) : base(positionInCode) {
        this.expr = expression;
    }

    public override T accept<T>(IStmtVisitor<T> visitor) {
        return visitor.visitPrintStmt(this);
    }
}

public class InputStmt : Stmt {
    public readonly Expr expr;
    public readonly string targetVarName;

    public InputStmt(Position positionInCode, Expr expression, string targetVarName) : base(positionInCode) {
        this.expr = expression;
        this.targetVarName = targetVarName;
    }

    public override T accept<T>(IStmtVisitor<T> visitor) {
        return visitor.visitInputStmt(this);
    }
}

public class LetStmt : Stmt {
    public readonly Expr expr;
    public readonly string targetVarName;

    public LetStmt(Position positionInCode, string targetVarName, Expr expression) : base(positionInCode) {
        this.targetVarName = targetVarName;
        this.expr = expression;
    }

    public override T accept<T>(IStmtVisitor<T> visitor) {
        return visitor.visitLetStmt(this);
    }
}

public class ToNumStmt : Stmt {
    public readonly string srcVarName;
    public readonly string dstVarName;

    public ToNumStmt(Position positionInCode, string sourceVarName, string destinationVarName) : base(positionInCode) {
        srcVarName = sourceVarName;
        dstVarName = destinationVarName;
    }

    public override T accept<T>(IStmtVisitor<T> visitor) {
        return visitor.visitToNumStmt(this);
    }
}

public class ToStrStmt : Stmt {
    public readonly string srcVarName;
    public readonly string dstVarName;

    public ToStrStmt(Position positionInCode, string sourceVarName, string destinationVarName) : base(positionInCode) {
        srcVarName = sourceVarName;
        dstVarName = destinationVarName;
    }

    public override T accept<T>(IStmtVisitor<T> visitor) {
        return visitor.visitToStrStmt(this);
    }
}

public class RndStmt : Stmt {
    public readonly string targetVarName;
    public readonly Expr lowerBound;
    public readonly Expr upperBound;

    public RndStmt(Position positionInCode, string targetVarName, Expr lowerBound, Expr upperBound) : base(positionInCode) {
        this.targetVarName = targetVarName;
        this.lowerBound = lowerBound;
        this.upperBound = upperBound;
    }

    public override T accept<T>(IStmtVisitor<T> visitor) {
        return visitor.visitRndStmt(this);
    }
}

public class BlockStmt : Stmt {
    public readonly IEnumerable<Stmt> statements;

    public BlockStmt(Position positionInCode, IEnumerable<Stmt> statements) : base(positionInCode) {
        this.statements = statements;
    }

    public override T accept<T>(IStmtVisitor<T> visitor) {
        return visitor.visitBlockStmt(this);
    }
}

public class IfStmt : Stmt {
    public readonly Expr condition;
    public readonly Stmt thenBranch;
    public readonly Stmt? elseBranch;

    public IfStmt(Position positionInCode, Expr condition, Stmt thenBranch, Stmt? elseBranch) : base(positionInCode) {
        this.condition = condition;
        this.thenBranch = thenBranch;
        this.elseBranch = elseBranch;
    }

    public override T accept<T>(IStmtVisitor<T> visitor) {
        return visitor.visitIfStmt(this);
    }
}

public class WhileStmt : Stmt {
    public readonly Expr condition;
    public readonly Stmt body;

    public WhileStmt(Position positionInCode, Expr condition, Stmt body) : base(positionInCode) {
        this.condition = condition;
        this.body = body;
    }

    public override T accept<T>(IStmtVisitor<T> visitor) {
        return visitor.visitWhileStmt(this);
    }
}

public class ContinueStmt : Stmt {
    public ContinueStmt(Position positionInCode) : base(positionInCode) { }

    public override T accept<T>(IStmtVisitor<T> visitor) {
        return visitor.visitContinueStmt(this);
    }
}

public class BreakStmt : Stmt {
    public BreakStmt(Position positionInCode) : base(positionInCode) { }

    public override T accept<T>(IStmtVisitor<T> visitor) {
        return visitor.visitBreakStmt(this);
    }
}
