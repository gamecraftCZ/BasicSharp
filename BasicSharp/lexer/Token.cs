using BasicSharp.common;

namespace BasicSharp.lexer;

public struct Token {
    // All possible types of the token
    public enum TokenType {
        // One character
        LEFT_PAREN, RIGHT_PAREN, COMMA,
        MINUS, PLUS, SLASH, STAR,

        // Potentially more characters
        EQUAL, EQUAL_EQUAL,
        GREATER, GREATER_EQUAL,
        LESS, LESS_EQUAL, NOT_EQUAL,

        // Literals
        IDENTIFIER, STRING, NUMBER, BOOLEAN,

        // Keywords
        REM, LET, INPUT, PRINT, TONUM, TOSTR, RND,
        IF, THEN, ELSE, END, WHILE, DO, BREAK, CONTINUE,
        NOT, AND, OR,

        EOF_TOKEN,
        EOS_TOKEN,  // Explicit end of statement (semicolon)
    }

    // Mapping of keywords to their respective TokenType
    public static readonly Dictionary<string, TokenType> keywords = new() {
        { "rem", TokenType.REM },
        { "let", TokenType.LET },
        { "input", TokenType.INPUT },
        { "print", TokenType.PRINT },
        { "tonum", TokenType.TONUM },
        { "tostr", TokenType.TOSTR },
        { "rnd", TokenType.RND },
        { "if", TokenType.IF },
        { "then", TokenType.THEN },
        { "else", TokenType.ELSE },
        { "end", TokenType.END },
        { "while", TokenType.WHILE },
        { "do", TokenType.DO },
        { "break", TokenType.BREAK },
        { "continue", TokenType.CONTINUE },
        { "not", TokenType.NOT },
        { "and", TokenType.AND },
        { "or", TokenType.OR }
    };

    public readonly TokenType type;     // Type of the Token
    public readonly string lexeme;      // Original Lexeme representation of the Token
    public readonly object? literal;    // Parsed value of the Token
    public readonly Position pos;       // Location of the Token in the source code

    public Token(TokenType type, string lexeme, object? literal, int line, int column) {
        this.type = type;
        this.lexeme = lexeme;
        this.literal = literal;
        this.pos = new Position(line, column);
    }

    public override string ToString() {
        return $"{type} ('{lexeme}') ('{literal}') (at {pos})";
    }
}
