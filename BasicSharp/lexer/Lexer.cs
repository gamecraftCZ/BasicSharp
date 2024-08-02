using System.Globalization;
using System.Text;

namespace BasicSharp.lexer;

public class Lexer {
    private readonly IEnumerator<char> _inputSource;

    private int _line = 1;
    private int _column;

    private int _startOfTokenLine;
    private int _startOfTokenColumn;

    private char _currentChar;
    private char _nextChar;
    private bool _isEnd;

    private Lexer(IEnumerable<char> inputSource) {
        _inputSource = inputSource.GetEnumerator();
        advance();
        _column = 0;
    }

    private static IEnumerable<char> readStreamCharByChar(Stream inputStream, Encoding? encoding = null) {
        // Use UTF-8 encoding by default.
        if (encoding == null) encoding = Encoding.UTF8;
        using (StreamReader reader = new StreamReader(inputStream, encoding)) {
            int charCode;
            // Read while there are characters to read
            while ((charCode = reader.Read()) != -1) {
                char character = (char)charCode;
                yield return character;
            }
        }
    }

    /// <summary>
    /// Lexes the source code from the input stream to Tokens.
    /// </summary>
    /// <param name="inputStream">Input stream of source code.</param>
    /// <returns>IEnumerable of lexed Tokens from the source code.</returns>
    public static IEnumerable<Token> lex(Stream inputStream) {
        Lexer lexer = new Lexer(readStreamCharByChar(inputStream));

        while (!lexer._isEnd) {
            Token? token = lexer.scanToken();
            if (token.HasValue) yield return token.Value;
        }

        yield return lexer.createToken(Token.TokenType.EOF_TOKEN, '\0');
    }

    /// <summary>
    /// Scans for next Token. May return null if no Token starts on next character.
    /// </summary>
    /// <returns>Token or null</returns>
    private Token? scanToken() {
        char c = advance();
        prepareTokenPosition();
        switch (c) {
            case '(': return createToken(Token.TokenType.LEFT_PAREN, c);
            case ')': return createToken(Token.TokenType.RIGHT_PAREN, c);
            case ',': return createToken(Token.TokenType.COMMA, c);
            case '-': return createToken(Token.TokenType.MINUS, c);
            case '+': return createToken(Token.TokenType.PLUS, c);
            case '*': return createToken(Token.TokenType.STAR, c);
            case '/': return createToken(Token.TokenType.SLASH, c);
            case ';': return createToken(Token.TokenType.EOS_TOKEN, c);
            case '=':
                if (matchNext('=')) {
                    advance();
                    return createToken(Token.TokenType.EQUAL_EQUAL, "==");
                }

                return createToken(Token.TokenType.EQUAL, "=");

            case '<':
                if (matchNext('=')) {
                    advance();
                    return createToken(Token.TokenType.LESS_EQUAL, "<=");
                }

                if (matchNext('>')) {
                    advance();
                    return createToken(Token.TokenType.NOT_EQUAL, "<>");
                }

                return createToken(Token.TokenType.LESS, "<");

            case '>':
                if (matchNext('=')) {
                    advance();
                    return createToken(Token.TokenType.GREATER_EQUAL, ">=");
                }

                return createToken(Token.TokenType.GREATER, ">");

            // New line.
            case '\n':
                _line++;
                _column = 0;
                return null;

            // Ignore whitespace.
            case ' ':
            case '\r':
            case '\t':
                return null;

            // String, Number, Identifier
            case '"': return scanString();
            default:
                if (isDigit(c)) return scanNumber();
                if (isAlpha(c)) return scanIdentifier();
                throw createError("Unexpected character.");
        }
    }

    /// <summary>
    /// Scans for a string token until the closing quote is reached.
    /// </summary>
    /// <returns>Token of type STRING</returns>
    /// <exception cref="LexerError">Throws when the string is unterminated and end of stream is reached.</exception>
    private Token scanString() {
        StringBuilder sb = new();
        // While there is not parentheses or end of file.
        while (peek() != '"' && !_isEnd) {
            if (peek() == '\n') {
                _line++;
                _column = 1;
            }

            sb.Append(advance());
        }

        if (_isEnd) throw createError("Unterminated string.");
        advance(); // Consume the closing quote.
        string value = sb.ToString();
        return createToken(Token.TokenType.STRING, value, value);
    }

    /// <summary>
    /// Scans for a number token.
    /// </summary>
    /// <returns>Token of type NUMBER</returns>
    /// <exception cref="LexerError">Throws when the parsed number is not valid double.</exception>
    private Token scanNumber() {
        StringBuilder sb = new();
        sb.Append(cur());
        while (!_isEnd && isDigit(peek())) sb.Append(advance());
        if (!_isEnd && (peek() == '.' || isDigit(peek()))) {
            sb.Append(advance());
            while (!_isEnd && isDigit(peek())) sb.Append(advance());
        }

        // Try parse with invariant culture so that we always use '.' as decimal separator.
        if (Double.TryParse(sb.ToString(), CultureInfo.InvariantCulture, out double number)) {
            return createToken(Token.TokenType.NUMBER, sb.ToString(), number);
        } else {
            throw createError($"Invalid number format: '{sb}'.");
        }
    }


    /// <summary>
    /// Scans for an identifier token until a non-alphanumeric character is reached.
    /// If the identifier is a keyword, returns a token with the type of the keyword.
    /// </summary>
    /// <returns>Token with type of specific keyword. Token with type IDENTIFIER otherwise.</returns>
    private Token scanIdentifier() {
        StringBuilder sb = new();
        sb.Append(cur());
        while (!_isEnd && (isAlpha(peek()) || isDigit(peek())) ) sb.Append(advance());
        string lexeme = sb.ToString();
        string lexemeLower = lexeme.ToLower();
        Token.TokenType type = Token.keywords.GetValueOrDefault(lexemeLower, Token.TokenType.IDENTIFIER);
        if (type == Token.TokenType.IDENTIFIER) {
            if (lexemeLower is "true" or "false") {
                return createToken(Token.TokenType.BOOLEAN, lexeme, lexemeLower == "true");
            }

            return createToken(type, lexeme, lexeme);
        }

        if (type == Token.TokenType.REM) {
            // Comments consume the rest of the line.
            while (peek() != '\n' && peek() != '\r' && !_isEnd) sb.Append(advance());
            lexeme = sb.ToString();
            return createToken(type, lexeme);
        }

        return createToken(type, lexeme);
    }

    private void prepareTokenPosition() {
        _startOfTokenLine = _line;
        _startOfTokenColumn = _column;
    }

    private Token createToken(Token.TokenType type, string lexeme, object? literal = null) {
        return new Token(type, lexeme, literal, _startOfTokenLine, _startOfTokenColumn);
    }

    private Token createToken(Token.TokenType type, char lexeme, object? literal = null) {
        return createToken(type, lexeme.ToString(), literal);
    }

    private LexerError createError(string message) {
        return new LexerError(message, _startOfTokenLine, _startOfTokenColumn);
    }

    private static bool isDigit(char c) { return c is >= '0' and <= '9'; }

    private static bool isAlpha(char c) { return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_'; }


    /// <summary>
    /// Returns the character at "cursor position"
    /// </summary>
    private char cur() { return _currentChar; }


    /// <summary>
    /// Returns the character at "cursor position + 1"
    /// </summary>
    private char peek() { return _nextChar; }

    /// <summary>
    /// Advances the cursor position and returns the character at the new position.
    /// </summary>
    private char advance() {
        if (_isEnd)
            throw createError(
                "Cannot advance past end of input. This is a bug in the lexer, please report it on GitHub."
            );

        _currentChar = _nextChar;
        _column++;

        // Read the next character. End if we reach end of input or null character.
        if (!_inputSource.MoveNext() || _inputSource.Current == '\0') {
            _isEnd = true;
            _nextChar = '\0';
        } else {
            _nextChar = _inputSource.Current;
        }

        return _currentChar;
    }

    /// <summary>
    /// True if next character matches the given character.
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    private bool matchNext(char c) {
        if (_isEnd) return false;
        return c == peek();
    }
}
