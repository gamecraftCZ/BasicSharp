using BasicSharp.lexer;

namespace BasicSharp.parser;

/// <summary>
/// Parses the Tokens list to Abstract Syntax Tree (AST) for each separate executable statement.
///  Uses recursive descent parsing.
/// </summary>
public class Parser {
    private readonly IEnumerator<Token> _tokensEnumerator;

    private Token _previousToken;
    private Token _currentToken;
    private Token _nextToken;

    private Parser(IEnumerable<Token> tokensEnumerable) {
        _tokensEnumerator = tokensEnumerable.GetEnumerator();
        advance();
        advance();
    }

    #region Actual parsing

    /// <summary>
    /// Parses the Tokens list to Abstract Syntax Tree (AST) for each separate executable statement.
    /// </summary>
    /// <param name="tokensEnumerable"></param>
    /// <returns>IEnumerable of </returns>
    public static IEnumerable<Stmt> parse(IEnumerable<Token> tokensEnumerable) {
        Parser parser = new(tokensEnumerable);

        while (!parser.isEnd()) {
            yield return parser.declaration();
        }
    }

    private Stmt declaration() {
        while (matchAdvance(Token.TokenType.EOS_TOKEN)) { }  // Skip semicolons (end of statement)
        if (matchAdvance(Token.TokenType.LET)) return letDeclaration();
        return statement();
    }

    private Stmt letDeclaration() {
        // LET IDENTIFIER EQUAL expression
        // Token letToken = consume(Token.TokenType.LET, "Expected 'let' keyword.");  // Already consumed by matchAdvance
        Token letToken = prev();

        // IDENTIFIER should always have a string literal.
        string varName = (string)consume(Token.TokenType.IDENTIFIER, "Expected variable name.").literal!;

        consume(Token.TokenType.EQUAL, "Expected '=' after variable name.");

        Expr expr = expression();
        return new LetStmt(letToken.pos, varName, expr);
    }

    private Stmt statement() {
        if (matchAdvance(Token.TokenType.IF)) return ifStmt();
        if (matchAdvance(Token.TokenType.WHILE)) return whileStmt();

        if (matchAdvance(Token.TokenType.PRINT)) return printStmt();
        if (matchAdvance(Token.TokenType.INPUT)) return inputStmt();
        if (matchAdvance(Token.TokenType.TONUM)) return toNumStmt();
        if (matchAdvance(Token.TokenType.TOSTR)) return toStrStmt();
        if (matchAdvance(Token.TokenType.RND)) return rndStmt();
        if (matchAdvance(Token.TokenType.BREAK)) return new BreakStmt(cur().pos);
        if (matchAdvance(Token.TokenType.CONTINUE)) return new ContinueStmt(cur().pos);

        if (match(Token.TokenType.EOF_TOKEN)) throw createError("Unexpected end of file.");

        throw createError("Statement expected.");
    }

    private Stmt ifStmt() {
        // IF expression THEN statement (ELSE statement)? END
        Token ifToken = prev();
        Expr condition = expression();

        consume(Token.TokenType.THEN, "Expected 'then' after condition.");

        Stmt thenBranch = block();
        Stmt? elseBranch = null;
        if (matchAdvance(Token.TokenType.ELSE)) {
            elseBranch = block();
        }

        consume(Token.TokenType.END, "Expected 'end' at the end of if-else statement.");

        return new IfStmt(ifToken.pos, condition, thenBranch, elseBranch);
    }

    private Stmt whileStmt() {
        // WHILE expression DO statement END
        Token whileToken = prev();
        Expr condition = expression();

        consume(Token.TokenType.DO, "Expected 'do' after condition.");

        Stmt body = block();

        consume(Token.TokenType.END, "Expected 'end' at the end of while statement.");

        return new WhileStmt(whileToken.pos, condition, body);
    }

    private Stmt block() {
        List<Stmt> statements = [];
        while (!isEnd() && !match(Token.TokenType.ELSE, Token.TokenType.END)) {
            statements.Add(declaration());
        }

        return new BlockStmt(cur().pos, statements);
    }

    private Stmt printStmt() {
        // PRINT expression
        Token printToken = prev();
        Expr expr = expression();
        return new PrintStmt(printToken.pos, expr);
    }

    private Stmt inputStmt() {
        // INPUT expression, IDENTIFIER
        Token inputToken = prev();
        Expr prompt = expression();
        consume(Token.TokenType.COMMA, "Expected ',' after prompt expression.");
        string varName = (string)consume(Token.TokenType.IDENTIFIER, "Expected variable name.").literal!;
        return new InputStmt(inputToken.pos, prompt, varName);
    }

    private Stmt toNumStmt() {
        // TONUM SRC_IDENTIFIER, <optional DEST_IDENTIFIER>
        Token toNumToken = prev();
        string srcVarName = (string)consume(Token.TokenType.IDENTIFIER, "Expected source variable name.").literal!;
        string? dstVarName;
        if (matchAdvance(Token.TokenType.COMMA)) {
            dstVarName = (string)consume(Token.TokenType.IDENTIFIER, "Expected destination variable name.").literal!;
        } else {
            dstVarName = srcVarName;
        }

        return new ToNumStmt(toNumToken.pos, srcVarName, dstVarName);
    }

    private Stmt toStrStmt() {
        // TOSTR SRC_IDENTIFIER, <optional DEST_IDENTIFIER>
        Token toStrToken = prev();
        string srcVarName = (string)consume(Token.TokenType.IDENTIFIER, "Expected source variable name.").literal!;
        string? dstVarName;
        if (matchAdvance(Token.TokenType.COMMA)) {
            dstVarName = (string)consume(Token.TokenType.IDENTIFIER, "Expected destination variable name.").literal!;
        } else {
            dstVarName = srcVarName;
        }

        return new ToStrStmt(toStrToken.pos, srcVarName, dstVarName);
    }

    private Stmt rndStmt() {
        // RND IDENTIFIER, expression, expression
        Token rndToken = prev();
        string varName = (string)consume(Token.TokenType.IDENTIFIER, "Expected variable name.").literal!;
        consume(Token.TokenType.COMMA, "Expected ',' after variable name.");
        Expr lowerBound = expression();
        consume(Token.TokenType.COMMA, "Expected ',' after lower bound.");
        Expr upperBound = expression();
        return new RndStmt(rndToken.pos, varName, lowerBound, upperBound);
    }

    private Expr expression() {
        return orWord();
    }

    private Expr orWord() {
        // andWord (OR andWord)*
        Expr expr = andWord();
        while (matchAdvance(Token.TokenType.OR)) {
            Token operatorToken = prev();
            Expr right = andWord();
            expr = new BinaryExpr(operatorToken.pos, expr, BinaryOperator.OR, right);
        }

        return expr;
    }

    private Expr andWord() {
        // equality (AND equality)*
        Expr expr = unaryNot();
        while (matchAdvance(Token.TokenType.AND)) {
            Token operatorToken = prev();
            Expr right = unaryNot();
            expr = new BinaryExpr(operatorToken.pos, expr, BinaryOperator.AND, right);
        }

        return expr;
    }

    private Expr unaryNot() {
        // NOT unaryNot | equality
        if (matchAdvance(Token.TokenType.NOT)) {
            Token operatorToken = prev();
            Expr right = unaryNot();
            return new UnaryExpr(operatorToken.pos, UnaryOperator.NOT, right);
        }

        return comparison();
    }

    private Expr comparison() {
        // term ((EQUAL_EQUAL | NOT_EQUAL | GREATER | GREATER_EQUAL | LESS | LESS_EQUAL) term)*
        Expr expr = term();
        while (matchAdvance(Token.TokenType.EQUAL_EQUAL, Token.TokenType.NOT_EQUAL,
                            Token.TokenType.GREATER, Token.TokenType.GREATER_EQUAL,
                            Token.TokenType.LESS, Token.TokenType.LESS_EQUAL)) {
            Token operatorToken = prev();
            Expr right = term();
            BinaryOperator operatorType = operatorToken.type switch {
                Token.TokenType.EQUAL_EQUAL => BinaryOperator.EQUAL_EQUAL,
                Token.TokenType.NOT_EQUAL => BinaryOperator.BANG_EQUAL,
                Token.TokenType.GREATER => BinaryOperator.GREATER,
                Token.TokenType.GREATER_EQUAL => BinaryOperator.GREATER_EQUAL,
                Token.TokenType.LESS => BinaryOperator.LESS,
                Token.TokenType.LESS_EQUAL => BinaryOperator.LESS_EQUAL,
                _ => throw createError("Invalid comparison operator.")
            };

            expr = new BinaryExpr(operatorToken.pos, expr, operatorType, right);
        }

        return expr;
    }

    private Expr term() {
        // factor ((PLUS | MINUS) factor)*
        Expr expr = factor();
        while (matchAdvance(Token.TokenType.PLUS, Token.TokenType.MINUS)) {
            Token operatorToken = prev();
            Expr right = factor();
            BinaryOperator operatorType = operatorToken.type switch {
                Token.TokenType.PLUS => BinaryOperator.PLUS,
                Token.TokenType.MINUS => BinaryOperator.MINUS,
                _ => throw createError("Invalid term operator.")
            };

            expr = new BinaryExpr(operatorToken.pos, expr, operatorType, right);
        }

        return expr;
    }

    private Expr factor() {
        // unary ((STAR | SLASH) unary)*
        Expr expr = unary();
        while (matchAdvance(Token.TokenType.STAR, Token.TokenType.SLASH)) {
            Token operatorToken = prev();
            Expr right = unary();
            BinaryOperator operatorType = operatorToken.type switch {
                Token.TokenType.STAR => BinaryOperator.STAR,
                Token.TokenType.SLASH => BinaryOperator.SLASH,
                _ => throw createError("Invalid factor operator.")
            };

            expr = new BinaryExpr(operatorToken.pos, expr, operatorType, right);
        }

        return expr;
    }

    private Expr unary() {
        // MINUS unary | primary
        if (matchAdvance(Token.TokenType.MINUS)) {
            Token operatorToken = prev();
            Expr right = unary();
            UnaryOperator operatorType = operatorToken.type switch {
                Token.TokenType.MINUS => UnaryOperator.MINUS,
                _ => throw createError("Invalid unary operator.")
            };

            return new UnaryExpr(operatorToken.pos, operatorType, right);
        }

        return primary();
    }

    private Expr primary() {
        // NUMBER | STRING | BOOLEAN | IDENTIFIER | LEFT_PAREN expression RIGHT_PAREN
        if (matchAdvance(Token.TokenType.NUMBER, Token.TokenType.STRING, Token.TokenType.BOOLEAN)) {
            return new LiteralExpr(prev().pos, prev().literal!);
        }

        if (matchAdvance(Token.TokenType.LEFT_PAREN)) {
            Expr expr = expression();
            consume(Token.TokenType.RIGHT_PAREN, "Expected ')' after expression.");
            return new GroupingExpr(prev().pos, expr);
        }

        if (matchAdvance(Token.TokenType.IDENTIFIER)) {
            return new VarExpr(prev().pos, (string)prev().literal!);
        }

        throw createError("Expression expected.");
    }

    #endregion

    #region Helper methods
    private Token peek() {
        return _nextToken;
    }

    private Token cur() {
        return _currentToken;
    }

    private Token prev() {
        return _previousToken;
    }

    /// <summary>
    /// We are at the end of the tokens list if the current token is EOF or the next token is EOF.
    /// </summary>
    /// <returns></returns>
    private bool isEnd() {
        return _currentToken.type == Token.TokenType.EOF_TOKEN;
    }

    /// <summary>
    /// Advances to the next token.
    /// </summary>
    /// <returns>Token that is now the current one.</returns>
    private Token advance() {
        _previousToken = _currentToken;
        _currentToken = _nextToken;

        // Skip comments
        do {
            if (_tokensEnumerator.MoveNext()) {
                _nextToken = _tokensEnumerator.Current;
            } else {
                _nextToken = new Token(Token.TokenType.EOF_TOKEN, "", null, -1, -1);
                break;
            }
        } while (_nextToken.type == Token.TokenType.REM);

        return _currentToken;
    }

    /// <summary>
    /// If the current token is of the given type returns true.
    /// </summary>
    /// <param name="types"></param>
    /// <returns></returns>
    private bool match(params Token.TokenType[] types) {
        if (types.Any(type => cur().type == type)) {
            return true;
        }

        return false;
    }

    /// <summary>
    /// If the current token is of the given type, advances to the next token and returns true.
    /// </summary>
    /// <param name="types"></param>
    /// <returns></returns>
    private bool matchAdvance(params Token.TokenType[] types) {
        if (match(types)) {
            advance();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if the current token is of the given type, if not throws an error.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="errorMessage"></param>
    private Token consume(Token.TokenType type, string errorMessage) {
        if (matchAdvance(type)) return prev();
        throw createError(errorMessage);
    }

    /// <summary>
    /// Creates a ParserError with the given message and the position of the current token.
    /// </summary>
    /// <param name="message">Message of the Error</param>
    /// <returns>ParserError</returns>
    private ParserError createError(string message) {
        return new ParserError(message, _currentToken.pos);
    }
    #endregion
}
