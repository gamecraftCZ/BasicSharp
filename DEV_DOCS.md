# Development documentation for BasicSharp

## Overall structure

- Stream (source code) -> Tokenization (Lexer) -> IEnumerable<Token>
- IEnumerable<Token> -> Parsing (Parser) -> IEnumerable<Stmt>
- IEnumerable<Stmt> -> Interpreting (Interpreter) -> Program behavior
- Main entry point (`Program.cs`), stitching all together.

### Tokenization (Lexer)

- Namespace `Lexer`
- Responsible for converting input source code to IEnumerable of Tokens.
- Defines `Token` object and `Token.TokenType` enum with all tokens types that may exists.
- Defines `Lexer` class for scanning tokens from Stream to a IEnumerable of Tokens.
    - Can throw `LexerError`

### Parsing (Parser)

- Namespace: `Parser`
- Responsible for converting IEnumerable of Tokens to Abstract Syntax Tree (AST) Statements IEnumerable.
- Expressions and Statements:
    - Defines `Expr` and `Stmt` with its subclasses for all possible expressions and statements.
    - Defines `IExprVisitor`, `IStmtVisitor` for implementation by the interpreter.
- `Parser`:
    - Defines `Parser` class for parsing tokens to AST.
    - Uses recursive descent parsing for parsing expressions and statements.
    - Uses `Expr` and `Stmt` subclasses for representing expressions and statements.
    - Can throw `ParsingError`

### Interpreting (Interpreter)

- Namespace: `BasicSharp.Interpreter`
- Responsible for interpreting AST statements.
- Defines `Interpreter` class implementing `IExprVisitor` and `IStmtVisitor` for interpreting AST
  using `interpretStatement(Stmt statement)`.
  - Can throw `InterpreterError`
  - Stateful interpreter, keeps track of variables and their values.

### Main entry point

- Files: `Program.cs`
- Responsible for stitching it all together.
- Gives help to user, opens input file, prints errors.

### Common code

- Namespace: `BasicSharp.common`
- Defines `Position` for storing position in source code.
