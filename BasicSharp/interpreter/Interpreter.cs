using BasicSharp.common;
using BasicSharp.parser;

namespace BasicSharp.interpreter;

public class Interpreter : IExprVisitor, IStmtVisitor {
    // Global variables
    private Dictionary<string, Variable> _globalVariables = new();

    public void interpretStatement(Stmt stmt) {
        try {
            stmt.accept(this);
        } catch (BreakException ex) {
            throw new InterpreterError("Unexpected 'BREAK' outside of loop", ex.pos);
        } catch (ContinueException ex) {
            throw new InterpreterError("Unexpected 'CONTINUE' outside of loop", ex.pos);
        }
    }

    #region Helpers

    private string getLiteralTypename(Variable variable) {
        return variable.Type switch {
            VariableType.NUMBER => "number",
            VariableType.STRING => "string",
            VariableType.BOOL => "bool",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private Variable getVarValue(string varName, Expr expr) {
        if (!_globalVariables.TryGetValue(varName, out Variable? value)) {
            throw new InterpreterError($"Variable '{varName}' is not defined.", expr.pos);
        }
        return value;
    }
    private Variable getVarValue(string varName, Stmt stmt) {
        if (!_globalVariables.TryGetValue(varName, out Variable? value)) {
            throw new InterpreterError($"Variable '{varName}' is not defined.", stmt.pos);
        }
        return value;
    }
    private void setVarValue(string varName, Variable value) {
        _globalVariables[varName] = value;
    }

    #endregion

    #region IExprVisitor

    public Variable visitUnaryExpr(UnaryExpr expr) {
        Variable right = expr.right.accept(this);

        switch (expr.operatorType) {
            case UnaryOperator.MINUS:
                if (right.Type != VariableType.NUMBER) throw new InterpreterError("Expected number", expr.pos);
                return new NumberVariable(-(double)right.Value);
            case UnaryOperator.NOT:
                if (right.Type != VariableType.BOOL) throw new InterpreterError("Expected bool", expr.pos);
                return new BoolVariable(!(bool)right.Value);
            default:
                throw new InterpreterError("Unknown unary operator", expr.pos);
        }
    }

    public Variable visitBinaryExpr(BinaryExpr expr) {
        Variable left = expr.left.accept(this);
        Variable right = expr.right.accept(this);

        switch (expr.operatorType) {
            case BinaryOperator.PLUS:
                if (left.Type == VariableType.NUMBER && right.Type == VariableType.NUMBER) {
                    return new NumberVariable((double)left.Value + (double)right.Value);
                }
                if (left.Type == VariableType.STRING) {
                    return new StringVariable((string)left.Value + right.ToString());
                }
                if (right.Type == VariableType.STRING) {
                    return new StringVariable(left.ToString() + (string)right.Value);
                }
                throw new InterpreterError($"Binary '+' is not allowed on '{getLiteralTypename(left)}' and '{getLiteralTypename(right)}'.", expr.pos);

            case BinaryOperator.MINUS:
                if (left.Type == VariableType.NUMBER && right.Type == VariableType.NUMBER) {
                    return new NumberVariable((double)left.Value - (double)right.Value);
                }
                throw new InterpreterError($"Binary '-' is not allowed on '{getLiteralTypename(left)}' and '{getLiteralTypename(right)}'.", expr.pos);

            case BinaryOperator.STAR:
                if (left.Type == VariableType.NUMBER && right.Type == VariableType.NUMBER) {
                    return new NumberVariable((double)left.Value * (double)right.Value);
                }
                throw new InterpreterError($"Binary '*' is not allowed on '{getLiteralTypename(left)}' and '{getLiteralTypename(right)}'.", expr.pos);

            case BinaryOperator.SLASH:
                if (left.Type == VariableType.NUMBER && right.Type == VariableType.NUMBER) {
                    return new NumberVariable((double)left.Value / (double)right.Value);
                }
                throw new InterpreterError($"Binary '/' is not allowed on '{getLiteralTypename(left)}' and '{getLiteralTypename(right)}'.", expr.pos);

            case BinaryOperator.EQUAL_EQUAL:
                if (left.Type == VariableType.NUMBER && right.Type == VariableType.NUMBER) {
                    return new BoolVariable(Math.Abs((double)left.Value - (double)right.Value) < double.Epsilon);
                }
                if (left.Type == VariableType.STRING && right.Type == VariableType.STRING) {
                    return new BoolVariable((string)left.Value == (string)right.Value);
                }
                throw new InterpreterError($"Binary '==' is not allowed on '{getLiteralTypename(left)}' and '{getLiteralTypename(right)}'.", expr.pos);

            case BinaryOperator.BANG_EQUAL:
                if (left.Type == VariableType.NUMBER && right.Type == VariableType.NUMBER) {
                    return new BoolVariable(Math.Abs((double)left.Value - (double)right.Value) > double.Epsilon);
                }
                if (left.Type == VariableType.STRING && right.Type == VariableType.STRING) {
                    return new BoolVariable((string)left.Value != (string)right.Value);
                }
                throw new InterpreterError($"Binary '!=' is not allowed on '{getLiteralTypename(left)}' and '{getLiteralTypename(right)}'.", expr.pos);

            case BinaryOperator.GREATER:
                if (left.Type == VariableType.NUMBER && right.Type == VariableType.NUMBER) {
                    return new BoolVariable((double)left.Value > (double)right.Value);
                }
                throw new InterpreterError($"Binary '>' is not allowed on '{getLiteralTypename(left)}' and '{getLiteralTypename(right)}'.", expr.pos);

            case BinaryOperator.GREATER_EQUAL:
                if (left.Type == VariableType.NUMBER && right.Type == VariableType.NUMBER) {
                    return new BoolVariable((double)left.Value >= (double)right.Value);
                }
                throw new InterpreterError($"Binary '>=' is not allowed on '{getLiteralTypename(left)}' and '{getLiteralTypename(right)}'.", expr.pos);

            case BinaryOperator.LESS:
                if (left.Type == VariableType.NUMBER && right.Type == VariableType.NUMBER) {
                    return new BoolVariable((double)left.Value < (double)right.Value);
                }
                throw new InterpreterError($"Binary '<' is not allowed on '{getLiteralTypename(left)}' and '{getLiteralTypename(right)}'.", expr.pos);

            case BinaryOperator.LESS_EQUAL:
                if (left.Type == VariableType.NUMBER && right.Type == VariableType.NUMBER) {
                    return new BoolVariable((double)left.Value <= (double)right.Value);
                }
                throw new InterpreterError($"Binary '<=' is not allowed on '{getLiteralTypename(left)}' and '{getLiteralTypename(right)}'.", expr.pos);

            case BinaryOperator.AND:
                if (left.Type == VariableType.BOOL && right.Type == VariableType.BOOL) {
                    return new BoolVariable((bool)left.Value && (bool)right.Value);
                }
                throw new InterpreterError($"Binary '&&' is not allowed on '{getLiteralTypename(left)}' and '{getLiteralTypename(right)}'.", expr.pos);

            case BinaryOperator.OR:
                if (left.Type == VariableType.BOOL && right.Type == VariableType.BOOL) {
                    return new BoolVariable((bool)left.Value || (bool)right.Value);
                }
                throw new InterpreterError($"Binary '||' is not allowed on '{getLiteralTypename(left)}' and '{getLiteralTypename(right)}'.", expr.pos);

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public Variable visitGroupingExpr(GroupingExpr expr) {
        return expr.expression.accept(this);
    }

    public Variable visitLiteralExpr(LiteralExpr expr) {
        return expr.value switch {
            double number => new NumberVariable(number),
            string str => new StringVariable(str),
            bool boolean => new BoolVariable(boolean),
            _ => throw new ApplicationException(),
        };
    }

    public Variable visitVarExpr(VarExpr expr) {
        return getVarValue(expr.varName, expr);
    }

    #endregion

    #region IStmtVisitor

    public void visitPrintStmt(PrintStmt stmt) {
        Variable value = stmt.expr.accept(this);
        Console.WriteLine(value.ToString());
    }

    public void visitInputStmt(InputStmt stmt) {
        Variable prompt = stmt.expr.accept(this);
        Console.Write(prompt.ToString());
        string inputVal = Console.ReadLine() ?? "";
        setVarValue(stmt.targetVarName, new StringVariable(inputVal));
    }

    public void visitLetStmt(LetStmt stmt) {
        Variable value = stmt.expr.accept(this);
        setVarValue(stmt.targetVarName, value);
    }

    public void visitToNumStmt(ToNumStmt stmt) {
        Variable srcValue = getVarValue(stmt.srcVarName, stmt);
        switch (srcValue.Type) {
            case VariableType.NUMBER:
                setVarValue(stmt.dstVarName, srcValue);
                break;

            case VariableType.STRING:
                if (!double.TryParse((string)srcValue.Value, out double numValue)) {
                    throw new InterpreterError($"Cannot convert '{srcValue.Value}' to number.", stmt.pos);
                }
                setVarValue(stmt.dstVarName, new NumberVariable(numValue));
                break;

            case VariableType.BOOL:
                setVarValue(stmt.dstVarName, new NumberVariable((bool)srcValue.Value ? 1 : 0));
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void visitToStrStmt(ToStrStmt stmt) {
        Variable srcValue = getVarValue(stmt.srcVarName, stmt);
        setVarValue(stmt.dstVarName, new StringVariable(srcValue.ToString()!));
    }

    public void visitRndStmt(RndStmt stmt) {
        Variable lowerBound = stmt.lowerBound.accept(this);
        Variable upperBound = stmt.upperBound.accept(this);
        if (lowerBound.Type == VariableType.NUMBER && upperBound.Type == VariableType.NUMBER) {
            double lower = (double)lowerBound.Value;
            double upper = (double)upperBound.Value;
            double randomValue = Math.Floor(new Random().NextDouble() * (upper - lower) + lower);
            setVarValue(stmt.targetVarName, new NumberVariable(randomValue));

        } else {
            throw new InterpreterError($"'RND' is not allowed with lowerBound '{getLiteralTypename(lowerBound)}' and upperBound '{getLiteralTypename(upperBound)}'.", stmt.pos);
        }
    }

    public void visitBlockStmt(BlockStmt stmt) {
        foreach (Stmt nestedStmt in stmt.statements) {
            nestedStmt.accept(this);
        }
    }

    public void visitIfStmt(IfStmt stmt) {
        Variable condition = stmt.condition.accept(this);
        if (condition.Type != VariableType.BOOL) {
            throw new InterpreterError("Expected bool in IF condition", stmt.pos);
        }

        if ((bool)condition.Value) {
            stmt.thenBranch.accept(this);
        } else {
            stmt.elseBranch?.accept(this);
        }
    }

    private class BreakException(Position pos) : Exception { public readonly Position pos = pos;}
    private class ContinueException(Position pos) : Exception {public readonly Position pos = pos; }
    public void visitWhileStmt(WhileStmt stmt) {
        while (true) {
            Variable condition = stmt.condition.accept(this);
            if (condition.Type != VariableType.BOOL) {
                throw new InterpreterError("Expected bool in WHILE condition", stmt.pos);
            }
            if (!(bool)condition.Value) break;
            try {
                stmt.body.accept(this);
            } catch (BreakException) {
                break;
            } catch (ContinueException) {
                continue;
            }
        }
    }

    public void visitContinueStmt(ContinueStmt stmt) { throw new ContinueException(stmt.pos); }

    public void visitBreakStmt(BreakStmt stmt) { throw new BreakException(stmt.pos); }
    #endregion
}
