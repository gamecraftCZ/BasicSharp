using System.Text;
using BasicSharp.lexer;

namespace BasicSharp_UnitTest;

public class UnitTest_Lexer {
    private IEnumerable<Token> LexString(string input) {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        return Lexer.lex(stream);
    }

    [Fact]
    public void TestSingleCharacterTokens() {
        var tokens = LexString("(),-+*/");
        var expectedTypes = new[] {
            Token.TokenType.LEFT_PAREN,
            Token.TokenType.RIGHT_PAREN,
            Token.TokenType.COMMA,
            Token.TokenType.MINUS,
            Token.TokenType.PLUS,
            Token.TokenType.STAR,
            Token.TokenType.SLASH,
            Token.TokenType.EOF_TOKEN
        };

        Assert.Equal(expectedTypes, tokens.Select(t => t.type));
    }

    [Fact]
    public void TestDoubleCharacterTokens() {
        var tokens = LexString("== <= >= <> <");
        var expectedTypes = new[] {
            Token.TokenType.EQUAL_EQUAL,
            Token.TokenType.LESS_EQUAL,
            Token.TokenType.GREATER_EQUAL,
            Token.TokenType.NOT_EQUAL,
            Token.TokenType.LESS,
            Token.TokenType.EOF_TOKEN
        };

        Assert.Equal(expectedTypes, tokens.Select(t => t.type));
    }

    [Fact]
    public void TestStringLiteral() {
        var tokens = LexString("\"Hello, World!\"");
        var stringToken = tokens.First();

        Assert.Equal(Token.TokenType.STRING, stringToken.type);
        Assert.Equal("Hello, World!", stringToken.literal);
    }

    [Fact]
    public void TestNumberLiteral() {
        var tokens = LexString("42 3.14").ToList();

        Assert.Equal(Token.TokenType.NUMBER, tokens[0].type);
        Assert.Equal(42.0, tokens[0].literal);

        Assert.Equal(Token.TokenType.NUMBER, tokens[1].type);
        Assert.Equal(3.14, tokens[1].literal);
    }

    [Fact]
    public void TestKeywords() {
        var tokens = LexString("IF THEN ELSE END WHILE");
        var expectedTypes = new[] {
            Token.TokenType.IF,
            Token.TokenType.THEN,
            Token.TokenType.ELSE,
            Token.TokenType.END,
            Token.TokenType.WHILE,
            Token.TokenType.EOF_TOKEN
        };

        Assert.Equal(expectedTypes, tokens.Select(t => t.type));
    }

    [Fact]
    public void TestIdentifiers() {
        var tokens = LexString("variable_name anotherVar").ToList();

        Assert.Equal(Token.TokenType.IDENTIFIER, tokens[0].type);
        Assert.Equal("variable_name", tokens[0].lexeme);

        Assert.Equal(Token.TokenType.IDENTIFIER, tokens[1].type);
        Assert.Equal("anotherVar", tokens[1].lexeme);
    }

    [Fact]
    public void TestUnterminatedString() {
        Assert.Throws<LexerError>(() => LexString("\"Unterminated string").ToList());
    }

    [Fact]
    public void TestInvalidNumber() { Assert.Throws<LexerError>(() => LexString("3.14.15").ToList()); }

    [Fact]
    public void TestUnexpectedCharacter() { Assert.Throws<LexerError>(() => LexString("$").ToList()); }

    [Fact]
    public void TestComplexExpression() {
        var tokens = LexString("LET x = (4 + 5) * 3");
        var expectedTypes = new[] {
            Token.TokenType.LET,
            Token.TokenType.IDENTIFIER,
            Token.TokenType.EQUAL,
            Token.TokenType.LEFT_PAREN,
            Token.TokenType.NUMBER,
            Token.TokenType.PLUS,
            Token.TokenType.NUMBER,
            Token.TokenType.RIGHT_PAREN,
            Token.TokenType.STAR,
            Token.TokenType.NUMBER,
            Token.TokenType.EOF_TOKEN
        };

        Assert.Equal(expectedTypes, tokens.Select(t => t.type));
    }

    [Fact]
    public void TestEmptyString() {
        var tokens = LexString("\"\"");
        var stringToken = tokens.First();

        Assert.Equal(Token.TokenType.STRING, stringToken.type);
        Assert.Equal("", stringToken.literal);
    }

    [Fact]
    public void TestIntegerNumbers() {
        var tokens = LexString("0 42 -17 +8").ToList();
        var expectedValues = new[] { 0.0, 42.0, 17.0, 8.0 };

        var numberTokens = tokens.Where(t => t.type == Token.TokenType.NUMBER).ToList();
        Assert.Equal(4, numberTokens.Count);

        Assert.Equal(Token.TokenType.MINUS, tokens[2].type);
        Assert.Equal(Token.TokenType.PLUS, tokens[4].type);

        for (int i = 0; i < expectedValues.Length; i++) {
            Assert.Equal(Token.TokenType.NUMBER, numberTokens[i].type);
            Assert.Equal(expectedValues[i], numberTokens[i].literal);
        }
    }

    [Fact]
    public void TestFloatingPointNumbers() {
        var tokens = LexString("3.14 -0.5 2.0 0.75").ToList();
        var expectedValues = new[] { 3.14, 0.5, 2.0, 0.75 };

        var numberTokens = tokens.Where(t => t.type == Token.TokenType.NUMBER).ToList();
        Assert.Equal(4, numberTokens.Count);

        Assert.Equal(Token.TokenType.MINUS, tokens[1].type);

        for (int i = 0; i < expectedValues.Length; i++) {
            Assert.Equal(Token.TokenType.NUMBER, numberTokens[i].type);
            Assert.Equal(expectedValues[i], numberTokens[i].literal);
        }
    }

    [Fact]
    public void TestInvalidNumbers() {
        Assert.Throws<LexerError>(() => LexString("3..14").ToList());
        Assert.Throws<LexerError>(() => LexString("3.14.15").ToList());
        Assert.Throws<LexerError>(() => LexString(".5").ToList());
    }

    [Fact]
    public void TestLineAndColumnNumbers() {
        string programCode = @"LET x = 10
IF x > 5 THEN
    PRINT ""x is greater than 5""
ELSE
    PRINT ""x is not greater than 5""
END
";
        var tokens = LexString(programCode).ToList();

        // Check line numbers
        Assert.Equal(1, tokens[0].line); // LET
        Assert.Equal(1, tokens[3].line); // 10
        Assert.Equal(2, tokens[4].line); // IF
        Assert.Equal(3, tokens[9].line); // PRINT
        Assert.Equal(4, tokens[11].line); // ELSE
        Assert.Equal(5, tokens[12].line); // PRINT
        Assert.Equal(6, tokens[14].line); // END

        // Check column numbers
        Assert.Equal(1, tokens[0].column); // LET
        Assert.Equal(5, tokens[1].column); // x
        Assert.Equal(7, tokens[2].column); // =
        Assert.Equal(9, tokens[3].column); // 10
        Assert.Equal(1, tokens[4].column); // IF
        Assert.Equal(4, tokens[5].column); // x
        Assert.Equal(6, tokens[6].column); // >
        Assert.Equal(8, tokens[7].column); // 5
        Assert.Equal(10, tokens[8].column); // THEN
        Assert.Equal(5, tokens[9].column); // PRINT
        Assert.Equal(11, tokens[10].column); // "x is greater than 5"
        Assert.Equal(1, tokens[11].column); // ELSE
        Assert.Equal(5, tokens[12].column); // PRINT
        Assert.Equal(11, tokens[13].column); // "x is not greater than 5"
        Assert.Equal(1, tokens[14].column); // END
    }

    [Fact]
    public void TestLineAndColumnNumbersWithComments() {
        string programCode = @"LET x = 10 REM Initialize x
IF x > 5 THEN REM Check if x is greater than 5
    PRINT ""x is greater than 5""
END
";
        var tokens = LexString(programCode).ToList();

        // Check line numbers
        Assert.Equal(1, tokens[0].line); // LET
        Assert.Equal(1, tokens[4].line); // REM
        Assert.Equal(2, tokens[5].line); // IF
        Assert.Equal(2, tokens[10].line); // REM
        Assert.Equal(3, tokens[11].line); // PRINT
        Assert.Equal(4, tokens[13].line); // END

        // Check column numbers
        Assert.Equal(1, tokens[0].column); // LET
        Assert.Equal(12, tokens[4].column); // REM
        Assert.Equal(1, tokens[5].column); // IF
        Assert.Equal(15, tokens[10].column); // REM
        Assert.Equal(5, tokens[11].column); // PRINT
        Assert.Equal(1, tokens[13].column); // END
    }

    [Fact]
    public void TestLineAndColumnNumbersWithMultiLineStrings() {
        string programCode = @"PRINT ""This is a
multi-line
string""
LET x = 5
";
        var tokens = LexString(programCode).ToList();

        // Check line numbers
        Assert.Equal(1, tokens[0].line); // PRINT
        Assert.Equal(1, tokens[1].line); // Start of the string
        Assert.Equal(4, tokens[2].line); // LET (after the multi-line string)
        Assert.Equal(4, tokens[4].line); // 5

        // Check column numbers
        Assert.Equal(1, tokens[0].column); // PRINT
        Assert.Equal(7, tokens[1].column); // Start of the string
        Assert.Equal(1, tokens[2].column); // LET
        Assert.Equal(5, tokens[3].column); // x
        Assert.Equal(7, tokens[4].column); // x
        Assert.Equal(9, tokens[5].column); // 5
    }

    [Fact]
    public void TestREMCommentConsumesWholeLine() {
        string programCode = @"LET x = 10
REM This is a comment LET y = 20
PRINT x
LET y = 30 REM This is another comment
PRINT y
";
        var tokens = LexString(programCode).ToList();

        // Check the number of tokens (excluding EOF)
        Assert.Equal(14, tokens.Count - 1);

        // Check the sequence of tokens
        Assert.Equal(Token.TokenType.LET, tokens[0].type);
        Assert.Equal(Token.TokenType.IDENTIFIER, tokens[1].type);
        Assert.Equal(Token.TokenType.EQUAL, tokens[2].type);
        Assert.Equal(Token.TokenType.NUMBER, tokens[3].type);
        Assert.Equal(10.0, tokens[3].literal);

        Assert.Equal(Token.TokenType.REM, tokens[4].type);
        Assert.Equal("REM This is a comment LET y = 20", tokens[4].lexeme.Trim());

        Assert.Equal(Token.TokenType.PRINT, tokens[5].type);
        Assert.Equal(Token.TokenType.IDENTIFIER, tokens[6].type);
        Assert.Equal("x", tokens[6].lexeme);

        Assert.Equal(Token.TokenType.LET, tokens[7].type);
        Assert.Equal(Token.TokenType.IDENTIFIER, tokens[8].type);
        Assert.Equal("y", tokens[8].lexeme);
        Assert.Equal(Token.TokenType.EQUAL, tokens[9].type);
        Assert.Equal(Token.TokenType.NUMBER, tokens[10].type);
        Assert.Equal(30.0, tokens[10].literal);

        Assert.Equal(Token.TokenType.REM, tokens[11].type);
        Assert.Equal("REM This is another comment", tokens[11].lexeme.Trim());

        Assert.Equal(Token.TokenType.PRINT, tokens[12].type);
        Assert.Equal(Token.TokenType.IDENTIFIER, tokens[13].type);
        Assert.Equal("y", tokens[13].lexeme);

        // Check line numbers
        Assert.Equal(1, tokens[0].line); // LET x = 10
        Assert.Equal(2, tokens[4].line); // REM comment
        Assert.Equal(3, tokens[5].line); // PRINT x
        Assert.Equal(4, tokens[7].line); // LET y = 30
        Assert.Equal(4, tokens[11].line); // REM comment on same line as LET
        Assert.Equal(5, tokens[12].line); // PRINT y

        // Check that no tokens were created for the commented-out LET y = 20
        Assert.DoesNotContain(tokens, t => t.lexeme == "20" && t.line == 2);
    }

    [Fact]
    public void TestCompleteBasicProgram() {
        string programCode = @"
REM Number guesser game

RND target_num, 0, 100
PRINT ""DEBUG: the number is: "" + target_num

LET current_try = 1
LET num = -1

WHILE num <> target_num DO
    INPUT ""Guess a number from 1 to 100: "", num
    TONUM num
    
    if num == target_num THEN
        PRINT ""Good job!""
        
    ELSE
        IF target_num > num THEN
            PRINT ""Nope, guess higher.""
        ELSE
            PRINT ""Nope, guess lower.""
        END
        
        LET current_try = current_try + 1
        IF current_try > 5 THEN
            PRINT ""You didn't guess it after 5 tries... :(""
            BREAK
        END
    END
END

PRINT ""Bye Bro.""
";
        var tokens = LexString(programCode).ToList();

        // Test overall structure
        Assert.True(tokens.Count == 70); // Ensure we have a significant number of tokens

        // Test specific elements
        Assert.Contains(tokens, t => t.type == Token.TokenType.REM);
        Assert.Contains(tokens, t => t.type == Token.TokenType.RND);
        Assert.Contains(tokens, t => t.type == Token.TokenType.PRINT);
        Assert.Contains(tokens, t => t.type == Token.TokenType.LET);
        Assert.Contains(tokens, t => t.type == Token.TokenType.WHILE);
        Assert.Contains(tokens, t => t.type == Token.TokenType.DO);
        Assert.Contains(tokens, t => t.type == Token.TokenType.INPUT);
        Assert.Contains(tokens, t => t.type == Token.TokenType.TONUM);
        Assert.Contains(tokens, t => t.type == Token.TokenType.IF);
        Assert.Contains(tokens, t => t.type == Token.TokenType.THEN);
        Assert.Contains(tokens, t => t.type == Token.TokenType.ELSE);
        Assert.Contains(tokens, t => t.type == Token.TokenType.END);
        Assert.Contains(tokens, t => t.type == Token.TokenType.BREAK);

        // Test specific literals and identifiers
        Assert.Contains(tokens, t => t.type == Token.TokenType.IDENTIFIER && t.lexeme == "target_num");
        Assert.Contains(tokens, t => t.type == Token.TokenType.IDENTIFIER && t.lexeme == "current_try");
        Assert.Contains(tokens, t => t.type == Token.TokenType.IDENTIFIER && t.lexeme == "num");
        Assert.Contains(tokens, t => t.type == Token.TokenType.NUMBER && (double)t.literal == 0);
        Assert.Contains(tokens, t => t.type == Token.TokenType.NUMBER && (double)t.literal == 100);
        Assert.Contains(tokens, t => t.type == Token.TokenType.NUMBER && (double)t.literal == 1);
        Assert.Contains(tokens, t => t.type == Token.TokenType.NUMBER && (double)t.literal == 5);

        // Test string literals
        Assert.Contains(tokens, t => t.type == Token.TokenType.STRING && (string)t.literal == "DEBUG: the number is: ");
        Assert.Contains(tokens,
            t => t.type == Token.TokenType.STRING && (string)t.literal == "Guess a number from 1 to 100: "
        );
        Assert.Contains(tokens, t => t.type == Token.TokenType.STRING && (string)t.literal == "Good job!");
        Assert.Contains(tokens, t => t.type == Token.TokenType.STRING && (string)t.literal == "Nope, guess higher.");
        Assert.Contains(tokens, t => t.type == Token.TokenType.STRING && (string)t.literal == "Nope, guess lower.");
        Assert.Contains(tokens,
            t => t.type == Token.TokenType.STRING && (string)t.literal == "You didn't guess it after 5 tries... :("
        );
        Assert.Contains(tokens, t => t.type == Token.TokenType.STRING && (string)t.literal == "Bye Bro.");

        // Test operators
        Assert.Contains(tokens, t => t.type == Token.TokenType.PLUS);
        Assert.Contains(tokens, t => t.type == Token.TokenType.EQUAL);
        Assert.Contains(tokens, t => t.type == Token.TokenType.NOT_EQUAL);
        Assert.Contains(tokens, t => t.type == Token.TokenType.GREATER);

        // Ensure correct order of some key elements
        var tokenTypes = tokens.Select(t => t.type).ToList();
        Assert.True(tokenTypes.IndexOf(Token.TokenType.WHILE) < tokenTypes.IndexOf(Token.TokenType.DO));
        Assert.True(tokenTypes.IndexOf(Token.TokenType.IF) < tokenTypes.IndexOf(Token.TokenType.THEN));
        Assert.True(tokenTypes.IndexOf(Token.TokenType.ELSE) < tokenTypes.LastIndexOf(Token.TokenType.END));
    }
}
