using BasicSharp.lexer;
using BasicSharp.parser;

namespace BasicSharp_UnitTest;

public class UnitTest_Parser {
    [Fact]
    public void Parse_EmptyInput_ReturnsEmptyList() {
        // Arrange
        var tokens = new List<Token>();

        // Act
        var result = Parser.parse(tokens).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Parse_SingleRemComment_ReturnsEmptyList() {
        // Arrange
        var tokens = new List<Token> {
            new Token(Token.TokenType.REM, "REM", null, 1, 1),
            new Token(Token.TokenType.EOF_TOKEN, "", null, 2, 1)
        };

        // Act
        var result = Parser.parse(tokens).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Parse_MultipleRemComments_ReturnsEmptyList() {
        // Arrange
        var tokens = new List<Token> {
            new Token(Token.TokenType.REM, "REM", null, 1, 1),
            new Token(Token.TokenType.REM, "REM", null, 2, 1),
            new Token(Token.TokenType.REM, "REM", null, 3, 1),
            new Token(Token.TokenType.EOF_TOKEN, "", null, 4, 1)
        };

        // Act
        var result = Parser.parse(tokens).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Parse_RemCommentsBetweenStatements_IgnoresComments() {
        // Arrange
        var tokens = new List<Token> {
            new Token(Token.TokenType.LET, "LET", null, 1, 1),
            new Token(Token.TokenType.IDENTIFIER, "x", "x", 1, 5),
            new Token(Token.TokenType.EQUAL, "=", null, 1, 7),
            new Token(Token.TokenType.NUMBER, "10", 10.0, 1, 9),
            new Token(Token.TokenType.REM, "REM", null, 2, 1),
            new Token(Token.TokenType.PRINT, "PRINT", null, 3, 1),
            new Token(Token.TokenType.IDENTIFIER, "x", "x", 3, 7),
            new Token(Token.TokenType.REM, "REM", null, 4, 1),
            new Token(Token.TokenType.EOF_TOKEN, "", null, 5, 1)
        };

        // Act
        var result = Parser.parse(tokens).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.IsType<LetStmt>(result[0]);
        Assert.IsType<PrintStmt>(result[1]);

        var letStmt = (LetStmt)result[0];
        Assert.Equal("x", letStmt.targetVarName);
        Assert.IsType<LiteralExpr>(letStmt.expr);
        Assert.Equal(10.0, ((LiteralExpr)letStmt.expr).value);

        var printStmt = (PrintStmt)result[1];
        Assert.IsType<VarExpr>(printStmt.expr);
        Assert.Equal("x", ((VarExpr)printStmt.expr).varName);
    }

    [Fact]
    public void Parse_SingleLetStatement_ReturnsCorrectAst() {
        // Arrange
        var tokens = new List<Token> {
            new Token(Token.TokenType.LET, "let", null, 1, 1),
            new Token(Token.TokenType.IDENTIFIER, "x", "x", 1, 5),
            new Token(Token.TokenType.EQUAL, "=", null, 1, 7),
            new Token(Token.TokenType.NUMBER, "42", 42.0, 1, 9),
            new Token(Token.TokenType.EOF_TOKEN, "", null, 1, 11)
        };

        // Act
        var result = Parser.parse(tokens).ToList();

        // Assert
        Assert.Single(result);
        Assert.IsType<LetStmt>(result[0]);
        var letStmt = (LetStmt)result[0];
        Assert.Equal("x", letStmt.targetVarName);
        Assert.IsType<LiteralExpr>(letStmt.expr);
        Assert.Equal(42.0, ((LiteralExpr)letStmt.expr).value);
    }

    [Fact]
    public void Parse_IfStatement_ReturnsCorrectAst() {
        // Arrange
        var tokens = new List<Token> {
            new Token(Token.TokenType.IF, "if", null, 1, 1),
            new Token(Token.TokenType.NUMBER, "1", 1.0, 1, 4),
            new Token(Token.TokenType.LESS, "<", null, 1, 6),
            new Token(Token.TokenType.NUMBER, "2", 2.0, 1, 8),
            new Token(Token.TokenType.THEN, "then", null, 1, 10),
            new Token(Token.TokenType.PRINT, "print", null, 2, 1),
            new Token(Token.TokenType.STRING, "\"True\"", "True", 2, 7),
            new Token(Token.TokenType.END, "end", null, 3, 1),
            new Token(Token.TokenType.EOF_TOKEN, "", null, 3, 4)
        };

        // Act
        var result = Parser.parse(tokens).ToList();

        // Assert
        Assert.Single(result);
        Assert.IsType<IfStmt>(result[0]);
        var ifStmt = (IfStmt)result[0];
        Assert.IsType<BinaryExpr>(ifStmt.condition);
        Assert.IsType<BlockStmt>(ifStmt.thenBranch);
        Assert.Null(ifStmt.elseBranch);
    }

    [Fact]
    public void Parse_WhileStatement_ReturnsCorrectAst() {
        // Arrange
        var tokens = new List<Token> {
            new Token(Token.TokenType.WHILE, "while", null, 1, 1),
            new Token(Token.TokenType.IDENTIFIER, "x", "x", 1, 7),
            new Token(Token.TokenType.LESS, "<", null, 1, 9),
            new Token(Token.TokenType.NUMBER, "10", 10.0, 1, 11),
            new Token(Token.TokenType.DO, "do", null, 1, 14),
            new Token(Token.TokenType.PRINT, "print", null, 2, 1),
            new Token(Token.TokenType.IDENTIFIER, "x", "x", 2, 7),
            new Token(Token.TokenType.END, "end", null, 3, 1),
            new Token(Token.TokenType.EOF_TOKEN, "", null, 3, 4)
        };

        // Act
        var result = Parser.parse(tokens).ToList();

        // Assert
        Assert.Single(result);
        Assert.IsType<WhileStmt>(result[0]);
        var whileStmt = (WhileStmt)result[0];
        Assert.IsType<BinaryExpr>(whileStmt.condition);
        Assert.IsType<BlockStmt>(whileStmt.body);
    }

    [Fact]
    public void Parse_ComplexExpression_ReturnsCorrectAst() {
        // Arrange
        var tokens = new List<Token> {
            new Token(Token.TokenType.LET, "let", null, 1, 1),
            new Token(Token.TokenType.IDENTIFIER, "result", "result", 1, 5),
            new Token(Token.TokenType.EQUAL, "=", null, 1, 12),
            new Token(Token.TokenType.LEFT_PAREN, "(", null, 1, 14),
            new Token(Token.TokenType.NUMBER, "5", 5.0, 1, 15),
            new Token(Token.TokenType.PLUS, "+", null, 1, 17),
            new Token(Token.TokenType.NUMBER, "3", 3.0, 1, 19),
            new Token(Token.TokenType.RIGHT_PAREN, ")", null, 1, 20),
            new Token(Token.TokenType.STAR, "*", null, 1, 22),
            new Token(Token.TokenType.NUMBER, "2", 2.0, 1, 24),
            new Token(Token.TokenType.EOF_TOKEN, "", null, 1, 25)
        };

        // Act
        var result = Parser.parse(tokens).ToList();

        // Assert
        Assert.Single(result);
        Assert.IsType<LetStmt>(result[0]);
        var letStmt = (LetStmt)result[0];
        Assert.Equal("result", letStmt.targetVarName);
        Assert.IsType<BinaryExpr>(letStmt.expr);
    }

    [Fact]
    public void Parse_MultipleStatements_ReturnsCorrectAst() {
        // Arrange
        var tokens = new List<Token> {
            new Token(Token.TokenType.LET, "let", null, 1, 1),
            new Token(Token.TokenType.IDENTIFIER, "x", "x", 1, 5),
            new Token(Token.TokenType.EQUAL, "=", null, 1, 7),
            new Token(Token.TokenType.NUMBER, "5", 5.0, 1, 9),
            new Token(Token.TokenType.PRINT, "print", null, 2, 1),
            new Token(Token.TokenType.IDENTIFIER, "x", "x", 2, 7),
            new Token(Token.TokenType.EOF_TOKEN, "", null, 2, 8)
        };

        // Act
        var result = Parser.parse(tokens).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.IsType<LetStmt>(result[0]);
        Assert.IsType<PrintStmt>(result[1]);
    }

    [Fact]
    public void Parse_MissingIdentifierAfterLet_ThrowsParserError() {
        // Arrange
        var tokens = new List<Token> {
            new Token(Token.TokenType.LET, "let", null, 1, 1),
            new Token(Token.TokenType.EQUAL, "=", null, 1, 5),
            new Token(Token.TokenType.NUMBER, "42", 42.0, 1, 7),
            new Token(Token.TokenType.EOF_TOKEN, "", null, 1, 9)
        };

        // Act & Assert
        var exception = Assert.Throws<ParserError>(() => Parser.parse(tokens).ToList());
        Assert.Equal("Expected variable name.", exception.Message);
    }

    [Fact]
    public void Parse_MissingEqualSignInLetStatement_ThrowsParserError() {
        // Arrange
        var tokens = new List<Token> {
            new Token(Token.TokenType.LET, "let", null, 1, 1),
            new Token(Token.TokenType.IDENTIFIER, "x", "x", 1, 5),
            new Token(Token.TokenType.NUMBER, "42", 42.0, 1, 7),
            new Token(Token.TokenType.EOF_TOKEN, "", null, 1, 9)
        };

        // Act & Assert
        var exception = Assert.Throws<ParserError>(() => Parser.parse(tokens).ToList());
        Assert.Equal("Expected '=' after variable name.", exception.Message);
    }

    [Fact]
    public void Parse_MissingThenInIfStatement_ThrowsParserError() {
        // Arrange
        var tokens = new List<Token> {
            new Token(Token.TokenType.IF, "if", null, 1, 1),
            new Token(Token.TokenType.NUMBER, "1", 1.0, 1, 4),
            new Token(Token.TokenType.LESS, "<", null, 1, 6),
            new Token(Token.TokenType.NUMBER, "2", 2.0, 1, 8),
            new Token(Token.TokenType.PRINT, "print", null, 2, 1),
            new Token(Token.TokenType.STRING, "\"True\"", "True", 2, 7),
            new Token(Token.TokenType.END, "end", null, 3, 1),
            new Token(Token.TokenType.EOF_TOKEN, "", null, 3, 4)
        };

        // Act & Assert
        var exception = Assert.Throws<ParserError>(() => Parser.parse(tokens).ToList());
        Assert.Equal("Expected 'then' after condition.", exception.Message);
    }

    [Fact]
    public void Parse_MissingEndInIfStatement_ThrowsParserError() {
        // Arrange
        var tokens = new List<Token> {
            new Token(Token.TokenType.IF, "if", null, 1, 1),
            new Token(Token.TokenType.NUMBER, "1", 1.0, 1, 4),
            new Token(Token.TokenType.LESS, "<", null, 1, 6),
            new Token(Token.TokenType.NUMBER, "2", 2.0, 1, 8),
            new Token(Token.TokenType.THEN, "then", null, 1, 10),
            new Token(Token.TokenType.PRINT, "print", null, 2, 1),
            new Token(Token.TokenType.STRING, "\"True\"", "True", 2, 7),
            new Token(Token.TokenType.EOF_TOKEN, "", null, 3, 1)
        };

        // Act & Assert
        var exception = Assert.Throws<ParserError>(() => Parser.parse(tokens).ToList());
        Assert.Equal("Expected 'end' at the end of if-else statement.", exception.Message);
    }

    [Fact]
    public void Parse_MissingDoInWhileStatement_ThrowsParserError() {
        // Arrange
        var tokens = new List<Token> {
            new Token(Token.TokenType.WHILE, "while", null, 1, 1),
            new Token(Token.TokenType.IDENTIFIER, "x", "x", 1, 7),
            new Token(Token.TokenType.LESS, "<", null, 1, 9),
            new Token(Token.TokenType.NUMBER, "10", 10.0, 1, 11),
            new Token(Token.TokenType.PRINT, "print", null, 2, 1),
            new Token(Token.TokenType.IDENTIFIER, "x", "x", 2, 7),
            new Token(Token.TokenType.END, "end", null, 3, 1),
            new Token(Token.TokenType.EOF_TOKEN, "", null, 3, 4)
        };

        // Act & Assert
        var exception = Assert.Throws<ParserError>(() => Parser.parse(tokens).ToList());
        Assert.Equal("Expected 'do' after condition.", exception.Message);
    }

    [Fact]
    public void Parse_MissingEndInWhileStatement_ThrowsParserError() {
        // Arrange
        var tokens = new List<Token> {
            new Token(Token.TokenType.WHILE, "while", null, 1, 1),
            new Token(Token.TokenType.IDENTIFIER, "x", "x", 1, 7),
            new Token(Token.TokenType.LESS, "<", null, 1, 9),
            new Token(Token.TokenType.NUMBER, "10", 10.0, 1, 11),
            new Token(Token.TokenType.DO, "do", null, 1, 14),
            new Token(Token.TokenType.PRINT, "print", null, 2, 1),
            new Token(Token.TokenType.IDENTIFIER, "x", "x", 2, 7),
            new Token(Token.TokenType.EOF_TOKEN, "", null, 3, 1)
        };

        // Act & Assert
        var exception = Assert.Throws<ParserError>(() => Parser.parse(tokens).ToList());
        Assert.Equal("Expected 'end' at the end of while statement.", exception.Message);
    }

    [Fact]
    public void Parse_MissingRightParenInGrouping_ThrowsParserError() {
        // Arrange
        var tokens = new List<Token> {
            new Token(Token.TokenType.PRINT, "print", null, 1, 1),
            new Token(Token.TokenType.LEFT_PAREN, "(", null, 1, 7),
            new Token(Token.TokenType.NUMBER, "5", 5.0, 1, 8),
            new Token(Token.TokenType.PLUS, "+", null, 1, 10),
            new Token(Token.TokenType.NUMBER, "3", 3.0, 1, 12),
            new Token(Token.TokenType.PRINT, "print", null, 2, 1),
            new Token(Token.TokenType.STRING, "Hi", "Hi", 2, 7),
            new Token(Token.TokenType.EOF_TOKEN, "", null, 2, 9),
        };

        // Act & Assert
        var exception = Assert.Throws<ParserError>(() => Parser.parse(tokens).ToList());
        Assert.Equal("Expected ')' after expression.", exception.Message);
    }

    [Fact]
    public void Parse_UnexpectedToken_ThrowsParserError() {
        // Arrange
        var tokens = new List<Token> {
            new Token(Token.TokenType.PLUS, "+", null, 1, 1),
            new Token(Token.TokenType.EOF_TOKEN, "", null, 1, 2)
        };

        // Act & Assert
        var exception = Assert.Throws<ParserError>(() => Parser.parse(tokens).ToList());
        Assert.Equal("Statement expected.", exception.Message);
    }

    [Fact]
    public void Parse_TwoPluses_ThrowsParserError() {
        // Arrange
        var tokens = new List<Token> {
            new Token(Token.TokenType.PRINT, "print", null, 1, 1),
            new Token(Token.TokenType.NUMBER, "5", 5, 1, 7),
            new Token(Token.TokenType.PLUS, "+", null, 1, 9),
            new Token(Token.TokenType.PLUS, "+", null, 1, 10),
            new Token(Token.TokenType.NUMBER, "7", 7, 1, 12),
            new Token(Token.TokenType.EOF_TOKEN, "", null, 1, 13)
        };

        // Act & Assert
        var exception = Assert.Throws<ParserError>(() => Parser.parse(tokens).ToList());
        Assert.Equal("Expression expected.", exception.Message);
    }

    [Fact]
    public void Parse_WhileStatementWithNestedStatements_ReturnsCorrectStructure() {
        // Arrange
        var tokens = new List<Token> {
            new Token(Token.TokenType.WHILE, "WHILE", null, 1, 1),
            new Token(Token.TokenType.IDENTIFIER, "x", "x", 1, 7),
            new Token(Token.TokenType.GREATER, ">", null, 1, 9),
            new Token(Token.TokenType.NUMBER, "0", 0.0, 1, 11),
            new Token(Token.TokenType.DO, "DO", null, 1, 13),
            new Token(Token.TokenType.PRINT, "PRINT", null, 2, 3),
            new Token(Token.TokenType.STRING, "\"x is positive\"", "x is positive", 2, 9),
            new Token(Token.TokenType.IF, "IF", null, 3, 3),
            new Token(Token.TokenType.IDENTIFIER, "x", "x", 3, 6),
            new Token(Token.TokenType.GREATER, ">", null, 3, 8),
            new Token(Token.TokenType.NUMBER, "10", 10.0, 3, 10),
            new Token(Token.TokenType.THEN, "THEN", null, 3, 13),
            new Token(Token.TokenType.PRINT, "PRINT", null, 4, 5),
            new Token(Token.TokenType.STRING, "\"x is greater than 10\"", "x is greater than 10", 4, 11),
            new Token(Token.TokenType.END, "END", null, 5, 3),
            new Token(Token.TokenType.LET, "LET", null, 6, 3),
            new Token(Token.TokenType.IDENTIFIER, "x", "x", 6, 7),
            new Token(Token.TokenType.EQUAL, "=", null, 6, 9),
            new Token(Token.TokenType.IDENTIFIER, "x", "x", 6, 11),
            new Token(Token.TokenType.MINUS, "-", null, 6, 13),
            new Token(Token.TokenType.NUMBER, "1", 1.0, 6, 15),
            new Token(Token.TokenType.END, "END", null, 7, 1),
            new Token(Token.TokenType.EOF_TOKEN, "", null, 8, 1)
        };

        // Act
        var result = Parser.parse(tokens).ToList();

        // Assert
        Assert.Single(result);
        Assert.IsType<WhileStmt>(result[0]);

        var whileStmt = (WhileStmt)result[0];
        Assert.IsType<BinaryExpr>(whileStmt.condition);
        Assert.IsType<BlockStmt>(whileStmt.body);

        var whileBody = (BlockStmt)whileStmt.body;
        Assert.Equal(3, whileBody.statements.Count());

        // Check first statement in while body (PRINT)
        Assert.IsType<PrintStmt>(whileBody.statements.ElementAt(0));
        var printStmt = (PrintStmt)whileBody.statements.ElementAt(0);
        Assert.IsType<LiteralExpr>(printStmt.expr);
        Assert.Equal("x is positive", ((LiteralExpr)printStmt.expr).value);

        // Check second statement in while body (IF)
        Assert.IsType<IfStmt>(whileBody.statements.ElementAt(1));
        var ifStmt = (IfStmt)whileBody.statements.ElementAt(1);
        Assert.IsType<BinaryExpr>(ifStmt.condition);
        Assert.IsType<BlockStmt>(ifStmt.thenBranch);
        Assert.Null(ifStmt.elseBranch);

        var ifBody = (BlockStmt)ifStmt.thenBranch;
        Assert.Single(ifBody.statements);
        Assert.IsType<PrintStmt>(ifBody.statements.First());

        // Check third statement in while body (LET)
        Assert.IsType<LetStmt>(whileBody.statements.ElementAt(2));
        var letStmt = (LetStmt)whileBody.statements.ElementAt(2);
        Assert.Equal("x", letStmt.targetVarName);
        Assert.IsType<BinaryExpr>(letStmt.expr);
    }

    [Fact]
    public void Parse_ComplexNumberGuesserGame_ReturnsCorrectAst() {
        // Arrange
        var tokens = new List<Token> {
            new Token(Token.TokenType.REM, "REM", null, 1, 1),
            new Token(Token.TokenType.RND, "RND", null, 3, 1),
            new Token(Token.TokenType.IDENTIFIER, "target_num", "target_num", 3, 5),
            new Token(Token.TokenType.COMMA, ",", null, 3, 15),
            new Token(Token.TokenType.NUMBER, "0", 0.0, 3, 17),
            new Token(Token.TokenType.COMMA, ",", null, 3, 18),
            new Token(Token.TokenType.NUMBER, "100", 100.0, 3, 20),
            new Token(Token.TokenType.PRINT, "PRINT", null, 4, 1),
            new Token(Token.TokenType.STRING, "\"DEBUG: the number is: \"", "DEBUG: the number is: ", 4, 7),
            new Token(Token.TokenType.PLUS, "+", null, 4, 32),
            new Token(Token.TokenType.IDENTIFIER, "target_num", "target_num", 4, 34),
            new Token(Token.TokenType.LET, "LET", null, 6, 1),
            new Token(Token.TokenType.IDENTIFIER, "current_try", "current_try", 6, 5),
            new Token(Token.TokenType.EQUAL, "=", null, 6, 17),
            new Token(Token.TokenType.NUMBER, "1", 1.0, 6, 19),
            new Token(Token.TokenType.LET, "LET", null, 7, 1),
            new Token(Token.TokenType.IDENTIFIER, "num", "num", 7, 5),
            new Token(Token.TokenType.EQUAL, "=", null, 7, 9),
            new Token(Token.TokenType.NUMBER, "-1", -1.0, 7, 11),
            new Token(Token.TokenType.WHILE, "WHILE", null, 9, 1),
            new Token(Token.TokenType.IDENTIFIER, "num", "num", 9, 7),
            new Token(Token.TokenType.NOT_EQUAL, "<>", null, 9, 11),
            new Token(Token.TokenType.IDENTIFIER, "target_num", "target_num", 9, 14),
            new Token(Token.TokenType.DO, "DO", null, 9, 25),
            new Token(Token.TokenType.INPUT, "INPUT", null, 10, 5),
            new Token(Token.TokenType.STRING, "\"Guess a number from 1 to 100: \"", "Guess a number from 1 to 100: ",
                10, 11
            ),
            new Token(Token.TokenType.COMMA, ",", null, 10, 44),
            new Token(Token.TokenType.IDENTIFIER, "num", "num", 10, 46),
            new Token(Token.TokenType.TONUM, "TONUM", null, 11, 5),
            new Token(Token.TokenType.IDENTIFIER, "num", "num", 11, 11),
            new Token(Token.TokenType.IF, "IF", null, 13, 5),
            new Token(Token.TokenType.IDENTIFIER, "num", "num", 13, 8),
            new Token(Token.TokenType.EQUAL_EQUAL, "==", null, 13, 12),
            new Token(Token.TokenType.IDENTIFIER, "target_num", "target_num", 13, 15),
            new Token(Token.TokenType.THEN, "THEN", null, 13, 26),
            new Token(Token.TokenType.PRINT, "PRINT", null, 14, 9),
            new Token(Token.TokenType.STRING, "\"Good job!\"", "Good job!", 14, 15),
            new Token(Token.TokenType.ELSE, "ELSE", null, 16, 5),
            new Token(Token.TokenType.IF, "IF", null, 17, 9),
            new Token(Token.TokenType.IDENTIFIER, "target_num", "target_num", 17, 12),
            new Token(Token.TokenType.GREATER, ">", null, 17, 23),
            new Token(Token.TokenType.IDENTIFIER, "num", "num", 17, 25),
            new Token(Token.TokenType.THEN, "THEN", null, 17, 29),
            new Token(Token.TokenType.PRINT, "PRINT", null, 18, 13),
            new Token(Token.TokenType.STRING, "\"Nope, guess higher.\"", "Nope, guess higher.", 18, 19),
            new Token(Token.TokenType.ELSE, "ELSE", null, 19, 9),
            new Token(Token.TokenType.PRINT, "PRINT", null, 20, 13),
            new Token(Token.TokenType.STRING, "\"Nope, guess lower.\"", "Nope, guess lower.", 20, 19),
            new Token(Token.TokenType.END, "END", null, 21, 9),
            new Token(Token.TokenType.LET, "LET", null, 23, 9),
            new Token(Token.TokenType.IDENTIFIER, "current_try", "current_try", 23, 13),
            new Token(Token.TokenType.EQUAL, "=", null, 23, 25),
            new Token(Token.TokenType.IDENTIFIER, "current_try", "current_try", 23, 27),
            new Token(Token.TokenType.PLUS, "+", null, 23, 39),
            new Token(Token.TokenType.NUMBER, "1", 1.0, 23, 41),
            new Token(Token.TokenType.IF, "IF", null, 24, 9),
            new Token(Token.TokenType.IDENTIFIER, "current_try", "current_try", 24, 12),
            new Token(Token.TokenType.GREATER, ">", null, 24, 24),
            new Token(Token.TokenType.NUMBER, "5", 5.0, 24, 26),
            new Token(Token.TokenType.THEN, "THEN", null, 24, 28),
            new Token(Token.TokenType.PRINT, "PRINT", null, 25, 13),
            new Token(Token.TokenType.STRING, "\"You didn't guess it after 5 tries... :(\"",
                "You didn't guess it after 5 tries... :(", 25, 19
            ),
            new Token(Token.TokenType.BREAK, "BREAK", null, 26, 13),
            new Token(Token.TokenType.END, "END", null, 27, 9),
            new Token(Token.TokenType.END, "END", null, 28, 5),
            new Token(Token.TokenType.END, "END", null, 29, 1),
            new Token(Token.TokenType.PRINT, "PRINT", null, 31, 1),
            new Token(Token.TokenType.STRING, "\"Bye Bro.\"", "Bye Bro.", 31, 7),
            new Token(Token.TokenType.EOF_TOKEN, "", null, 32, 1)
        };

        // Act
        var result = Parser.parse(tokens).ToList();

        // Assert
        Assert.Equal(6, result.Count); // RND, PRINT, LET, LET, WHILE, PRINT

        // Check RND statement
        Assert.IsType<RndStmt>(result[0]);
        var rndStmt = (RndStmt)result[0];
        Assert.Equal("target_num", rndStmt.targetVarName);
        Assert.IsType<LiteralExpr>(rndStmt.lowerBound);
        Assert.Equal(0.0, ((LiteralExpr)rndStmt.lowerBound).value);
        Assert.IsType<LiteralExpr>(rndStmt.upperBound);
        Assert.Equal(100.0, ((LiteralExpr)rndStmt.upperBound).value);

        // Check PRINT statement
        Assert.IsType<PrintStmt>(result[1]);

        // Check LET statements
        Assert.IsType<LetStmt>(result[2]);
        Assert.IsType<LetStmt>(result[3]);

        // Check WHILE statement
        Assert.IsType<WhileStmt>(result[4]);
        var whileStmt = (WhileStmt)result[4];
        Assert.IsType<BinaryExpr>(whileStmt.condition);
        Assert.IsType<BlockStmt>(whileStmt.body);

        var whileBody = (BlockStmt)whileStmt.body;
        Assert.Equal(3, whileBody.statements.Count()); // INPUT, TONUM, IF

        // Check nested IF statement inside WHILE
        var ifStmt = (IfStmt)whileBody.statements.ElementAt(2);
        Assert.IsType<BlockStmt>(ifStmt.thenBranch);
        Assert.IsType<BlockStmt>(ifStmt.elseBranch);

        var elseBranch = (BlockStmt)ifStmt.elseBranch;
        Assert.Equal(3, elseBranch.statements.Count()); // IF, LET, IF

        // Check BREAK statement
        var nestedIfStmt = (IfStmt)elseBranch.statements.ElementAt(2);
        var nestedIfBody = (BlockStmt)nestedIfStmt.thenBranch;
        Assert.IsType<BreakStmt>(nestedIfBody.statements.ElementAt(1));

        // Check final PRINT statement
        Assert.IsType<PrintStmt>(result[5]);
    }
}
