using BasicSharp.common;
using BasicSharp.interpreter;
using BasicSharp.parser;

namespace BasicSharp_UnitTest;

public class UnitTest_Interpreter {
    // Helper method to capture console output
    private static string captureConsoleOutput(Action action) {
        var originalOutput = Console.Out;
        using (var stringWriter = new StringWriter()) {
            Console.SetOut(stringWriter);
            action();
            Console.SetOut(originalOutput);
            return stringWriter.ToString();
        }
    }

    // Helper method to mock console input
    private void mockConsoleInput(string input, Action action) {
        var originalInput = Console.In;
        using (var stringReader = new StringReader(input)) {
            Console.SetIn(stringReader);
            action();
            Console.SetIn(originalInput);
        }
    }

    [Fact]
    public void InterpretStatement_PrintStmt_PrintsStringCorrectly() {
        // Arrange
        var interpreter = new Interpreter();
        var stmt = new PrintStmt(new Position(1, 1), new LiteralExpr(new Position(1, 7), "Hello, World!"));

        // Act
        var output = captureConsoleOutput(() => interpreter.interpretStatement(stmt));

        // Assert
        Assert.Equal("Hello, World!", output.Trim());
    }

    [Fact]
    public void InterpretStatement_PrintStmt_PrintsWholeNumberCorrectly() {
        // Arrange
        var interpreter = new Interpreter();
        var stmt = new PrintStmt(new Position(1, 1), new LiteralExpr(new Position(1, 7), 42.0));

        // Act
        var output = captureConsoleOutput(() => interpreter.interpretStatement(stmt));

        // Assert
        Assert.Equal("42", output.Trim());
    }

    [Fact]
    public void InterpretStatement_PrintStmt_PrintsDecimalNumberCorrectly() {
        // Arrange
        var interpreter = new Interpreter();
        var stmt = new PrintStmt(new Position(1, 1), new LiteralExpr(new Position(1, 7), 42.5));

        // Act
        var output = captureConsoleOutput(() => interpreter.interpretStatement(stmt));

        // Assert
        Assert.Equal("42.50", output.Trim());
    }

    [Fact]
    public void InterpretStatement_PrintStmt_PrintsBooleanCorrectly() {
        // Arrange
        var interpreter = new Interpreter();
        var stmtPrintTrue = new PrintStmt(new Position(1, 1), new LiteralExpr(new Position(1, 7), true));
        var stmtPrintFalse = new PrintStmt(new Position(2, 1), new LiteralExpr(new Position(2, 7), false));

        // Act
        var outputTrue = captureConsoleOutput(() => interpreter.interpretStatement(stmtPrintTrue));
        var outputFalse = captureConsoleOutput(() => interpreter.interpretStatement(stmtPrintFalse));

        // Assert
        Assert.Equal("TRUE", outputTrue.Trim());
        Assert.Equal("FALSE", outputFalse.Trim());
    }

    [Fact]
    public void InterpretStatement_LetStmt_AssignsVariable() {
        // Arrange
        var interpreter = new Interpreter();
        var stmt = new LetStmt(new Position(1, 1), "x", new LiteralExpr(new Position(1, 7), 42.0));

        // Act
        interpreter.interpretStatement(stmt);

        // Assert
        var result = interpreter.visitVarExpr(new VarExpr(new Position(1, 1), "x"));
        Assert.Equal(42.0, (double)result.Value);
    }

    [Fact]
    public void InterpretStatement_IfStmt_ExecutesThenBranch() {
        // Arrange
        var interpreter = new Interpreter();
        var condition = new LiteralExpr(new Position(1, 4), true);
        var thenBranch = new LetStmt(new Position(1, 10), "result", new LiteralExpr(new Position(1, 20), "then"));
        var elseBranch = new LetStmt(new Position(2, 7), "result", new LiteralExpr(new Position(2, 17), "else"));
        var stmt = new IfStmt(new Position(1, 1), condition, thenBranch, elseBranch);

        // Act
        interpreter.interpretStatement(stmt);

        // Assert
        var result = interpreter.visitVarExpr(new VarExpr(new Position(3, 1), "result"));
        Assert.Equal("then", result.Value);
    }

    [Fact]
    public void InterpretStatement_WhileStmt_ExecutesBody() {
        // Arrange
        var interpreter = new Interpreter();
        var initStmt = new LetStmt(new Position(1, 1), "counter", new LiteralExpr(new Position(1, 12), 0.0));
        var condition = new BinaryExpr(
            new Position(2, 7),
            new VarExpr(new Position(2, 7), "counter"),
            BinaryOperator.LESS,
            new LiteralExpr(new Position(2, 17), 3.0)
        );
        var body = new LetStmt(
            new Position(3, 5),
            "counter",
            new BinaryExpr(
                new Position(3, 15),
                new VarExpr(new Position(3, 15), "counter"),
                BinaryOperator.PLUS,
                new LiteralExpr(new Position(3, 25), 1.0)
            )
        );
        var whileStmt = new WhileStmt(new Position(2, 1), condition, body);

        // Act
        interpreter.interpretStatement(initStmt);
        interpreter.interpretStatement(whileStmt);

        // Assert
        var result = interpreter.visitVarExpr(new VarExpr(new Position(4, 1), "counter"));
        Assert.Equal(3.0, (double)result.Value);
    }

    [Fact]
    public void InterpretStatement_BreakOutsideLoop_ThrowsInterpreterError() {
        // Arrange
        var interpreter = new Interpreter();
        var stmt = new BreakStmt(new Position(1, 1));

        // Act & Assert
        Assert.Throws<InterpreterError>(() => interpreter.interpretStatement(stmt));
    }

    [Fact]
    public void InterpretStatement_ContinueOutsideLoop_ThrowsInterpreterError() {
        // Arrange
        var interpreter = new Interpreter();
        var stmt = new ContinueStmt(new Position(1, 1));

        // Act & Assert
        Assert.Throws<InterpreterError>(() => interpreter.interpretStatement(stmt));
    }

    [Fact]
    public void InterpretStatement_PrintStmt_PrintsCorrectOutput() {
        // Arrange
        var interpreter = new Interpreter();
        var stmt = new PrintStmt(new Position(1, 1), new LiteralExpr(new Position(1, 7), "Hello, World!"));

        // Act
        var output = captureConsoleOutput(() => interpreter.interpretStatement(stmt));

        // Assert
        Assert.Equal("Hello, World!", output.Trim());
    }

    [Fact]
    public void InterpretStatement_InputStmt_AssignsUserInputToVariable() {
        // Arrange
        var interpreter = new Interpreter();
        var promptExpr = new LiteralExpr(new Position(1, 7), "Enter your name: ");
        var stmt = new InputStmt(new Position(1, 1), promptExpr, "name");
        var expectedInput = "John Doe";

        // Act
        mockConsoleInput(expectedInput, () => {
                var output = captureConsoleOutput(() => interpreter.interpretStatement(stmt));
                // Assert prompt is printed
                Assert.Equal("Enter your name: ", output);
            }
        );

        // Assert
        var result = interpreter.visitVarExpr(new VarExpr(new Position(2, 1), "name"));
        Assert.Equal(expectedInput, result.Value);
    }

    [Fact]
    public void InterpretStatement_ArithmeticExpr_CalculatesCorrectly() {
        // Arrange
        var interpreter = new Interpreter();
        var expr = new BinaryExpr(
            new Position(1, 1),
            new BinaryExpr(
                new Position(1, 1),
                new LiteralExpr(new Position(1, 1), 5.0),
                BinaryOperator.PLUS,
                new LiteralExpr(new Position(1, 5), 3.0)
            ),
            BinaryOperator.STAR,
            new LiteralExpr(new Position(1, 10), 2.0)
        );
        var stmt = new LetStmt(new Position(1, 1), "result", expr);

        // Act
        interpreter.interpretStatement(stmt);

        // Assert
        var result = interpreter.visitVarExpr(new VarExpr(new Position(2, 1), "result"));
        Assert.Equal(16.0, (double)result.Value);
    }

    [Fact]
    public void InterpretStatement_StringConcatenation_ConcatenatesCorrectly() {
        // Arrange
        var interpreter = new Interpreter();
        var expr = new BinaryExpr(
            new Position(1, 1),
            new LiteralExpr(new Position(1, 1), "Hello, "),
            BinaryOperator.PLUS,
            new LiteralExpr(new Position(1, 10), "World!")
        );
        var stmt = new LetStmt(new Position(1, 1), "result", expr);

        // Act
        interpreter.interpretStatement(stmt);

        // Assert
        var result = interpreter.visitVarExpr(new VarExpr(new Position(2, 1), "result"));
        Assert.Equal("Hello, World!", result.Value);
    }

    [Fact]
    public void InterpretStatement_StringNumberWithImplicitConversionConcatenation_ConcatenatesCorrectly() {
        // Arrange
        var interpreter = new Interpreter();
        var expr = new BinaryExpr(
            new Position(1, 1),
            new LiteralExpr(new Position(1, 1), "Hello, "),
            BinaryOperator.PLUS,
            new LiteralExpr(new Position(1, 10), 42.0)
        );
        var stmt = new LetStmt(new Position(1, 1), "result", expr);

        // Act
        interpreter.interpretStatement(stmt);

        // Assert
        var result = interpreter.visitVarExpr(new VarExpr(new Position(2, 1), "result"));
        Assert.Equal("Hello, 42", result.Value);
    }

    [Fact]
    public void InterpretStatement_ImplicitTypeConversion_NumberToBooleanInIfStatementThrowsException() {
        // Arrange
        var interpreter = new Interpreter();
        var condition = new LiteralExpr(new Position(1, 4), 1.0);
        var thenBranch = new LetStmt(new Position(1, 10), "result", new LiteralExpr(new Position(1, 20), "then"));
        var elseBranch = new LetStmt(new Position(2, 7), "result", new LiteralExpr(new Position(2, 17), "else"));
        var stmt = new IfStmt(new Position(1, 1), condition, thenBranch, elseBranch);

        // Act & Assert
        Assert.Throws<InterpreterError>(() => interpreter.interpretStatement(stmt));
    }

    [Fact]
    public void InterpretStatement_WhileStmt_ExecutesBreakCorrectly() {
        // Arrange
        var interpreter = new Interpreter();
        var initStmt = new LetStmt(new Position(1, 1), "counter", new LiteralExpr(new Position(1, 12), 0.0));
        var condition = new LiteralExpr(new Position(2, 7), true);
        var body = new BlockStmt(new Position(3, 1), new Stmt[] {
                new LetStmt(
                    new Position(3, 5),
                    "counter",
                    new BinaryExpr(
                        new Position(3, 15),
                        new VarExpr(new Position(3, 15), "counter"),
                        BinaryOperator.PLUS,
                        new LiteralExpr(new Position(3, 25), 1.0)
                    )
                ),
                new IfStmt(
                    new Position(4, 5),
                    new BinaryExpr(
                        new Position(4, 9),
                        new VarExpr(new Position(4, 9), "counter"),
                        BinaryOperator.GREATER_EQUAL,
                        new LiteralExpr(new Position(4, 20), 3.0)
                    ),
                    new BreakStmt(new Position(5, 9)),
                    null
                )
            }
        );
        var whileStmt = new WhileStmt(new Position(2, 1), condition, body);

        // Act
        interpreter.interpretStatement(initStmt);
        interpreter.interpretStatement(whileStmt);

        // Assert
        var result = interpreter.visitVarExpr(new VarExpr(new Position(7, 1), "counter"));
        Assert.Equal(3.0, (double)result.Value);
    }

    [Fact]
    public void InterpretStatement_WhileStmt_ExecutesContinueCorrectly() {
        // Arrange
        var interpreter = new Interpreter();
        var initStmt = new LetStmt(new Position(1, 1), "sum", new LiteralExpr(new Position(1, 12), 0.0));
        var condition = new BinaryExpr(
            new Position(2, 7),
            new VarExpr(new Position(2, 7), "sum"),
            BinaryOperator.LESS,
            new LiteralExpr(new Position(2, 13), 10.0)
        );
        var body = new BlockStmt(new Position(3, 1), new Stmt[] {
                new LetStmt(
                    new Position(3, 5),
                    "sum",
                    new BinaryExpr(
                        new Position(3, 11),
                        new VarExpr(new Position(3, 11), "sum"),
                        BinaryOperator.PLUS,
                        new LiteralExpr(new Position(3, 17), 1.0)
                    )
                ),
                new IfStmt(
                    new Position(4, 5),
                    new BinaryExpr(
                        new Position(4, 9),
                        new VarExpr(new Position(4, 9), "sum"),
                        BinaryOperator.EQUAL_EQUAL,
                        new LiteralExpr(new Position(4, 16), 5.0)
                    ),
                    new ContinueStmt(new Position(5, 9)),
                    null
                ),
                new LetStmt(
                    new Position(6, 5),
                    "sum",
                    new BinaryExpr(
                        new Position(6, 11),
                        new VarExpr(new Position(6, 11), "sum"),
                        BinaryOperator.PLUS,
                        new LiteralExpr(new Position(6, 17), 1.0)
                    )
                )
            }
        );
        var whileStmt = new WhileStmt(new Position(2, 1), condition, body);

        // Act
        interpreter.interpretStatement(initStmt);
        interpreter.interpretStatement(whileStmt);

        // Assert
        var result = interpreter.visitVarExpr(new VarExpr(new Position(8, 1), "sum"));
        Assert.Equal(11.0, (double)result.Value);
    }

    [Fact]
    public void InterpretStatement_ToNumStmt_ConvertsStringToNumber() {
        // Arrange
        var interpreter = new Interpreter();
        var letStmt = new LetStmt(new Position(1, 1), "strNum", new LiteralExpr(new Position(1, 11), "42.5"));
        var toNumStmt = new ToNumStmt(new Position(2, 1), "strNum", "numResult");

        // Act
        interpreter.interpretStatement(letStmt);
        interpreter.interpretStatement(toNumStmt);

        // Assert
        var result = interpreter.visitVarExpr(new VarExpr(new Position(3, 1), "numResult"));
        Assert.Equal(42.5, (double)result.Value);
    }

    [Fact]
    public void InterpretStatement_ToStrStmt_ConvertsNumberToString() {
        // Arrange
        var interpreter = new Interpreter();
        var letStmt = new LetStmt(new Position(1, 1), "num", new LiteralExpr(new Position(1, 8), 42.5));
        var toStrStmt = new ToStrStmt(new Position(2, 1), "num", "strResult");

        // Act
        interpreter.interpretStatement(letStmt);
        interpreter.interpretStatement(toStrStmt);

        // Assert
        var result = interpreter.visitVarExpr(new VarExpr(new Position(3, 1), "strResult"));
        Assert.Equal("42.50", result.Value);
    }

    [Fact]
    public void InterpretStatement_RndStmt_GeneratesRandomNumberWithinRange() {
        // Arrange
        var interpreter = new Interpreter(new Random(42)); // Fixed random seed
        var rndStmt = new RndStmt(
            new Position(1, 1),
            "result",
            new LiteralExpr(new Position(1, 8), 1.0),
            new LiteralExpr(new Position(1, 11), 1000.0)
        );
        double expectedRndVal = Math.Floor(new Random(42).NextDouble() * (1000 - 1) + 1);

        // Act
        interpreter.interpretStatement(rndStmt);

        // Assert
        var result = interpreter.visitVarExpr(new VarExpr(new Position(2, 1), "result"));
        Assert.True((double)result.Value >= 1.0 && (double)result.Value < 1000.0);
        Assert.Equal(expectedRndVal, (double)result.Value);
    }

    [Fact]
    public void InterpretStatement_ComplexNumberGuessingGame_ExecutesCorrectly() {
        // Arrange
        var interpreter = new Interpreter(new Random(42));
        var statements = new List<Stmt> {
            // RND target_num, 0, 100
            new RndStmt(new Position(1, 1), "target_num",
                new LiteralExpr(new Position(1, 16), 0.0),
                new LiteralExpr(new Position(1, 19), 100.0)
            ),

            // PRINT "DEBUG: the number is: " + target_num
            new PrintStmt(new Position(2, 1),
                new BinaryExpr(new Position(2, 7),
                    new LiteralExpr(new Position(2, 7), "DEBUG: the number is: "),
                    BinaryOperator.PLUS,
                    new VarExpr(new Position(2, 32), "target_num")
                )
            ),

            // LET current_try = 1
            new LetStmt(new Position(4, 1), "current_try", new LiteralExpr(new Position(4, 18), 1.0)),

            // LET num = -1
            new LetStmt(new Position(5, 1), "num", new LiteralExpr(new Position(5, 10), -1.0)),

            // WHILE num <> target_num DO
            new WhileStmt(new Position(7, 1),
                new BinaryExpr(new Position(7, 7),
                    new VarExpr(new Position(7, 7), "num"),
                    BinaryOperator.BANG_EQUAL,
                    new VarExpr(new Position(7, 14), "target_num")
                ),
                new BlockStmt(new Position(7, 28), new List<Stmt> {
                        // INPUT "Guess a number from 1 to 100: ", num
                        new InputStmt(new Position(8, 5),
                            new LiteralExpr(new Position(8, 11), "Guess a number from 1 to 100: "),
                            "num"
                        ),

                        // TONUM num
                        new ToNumStmt(new Position(9, 5), "num", "num"),

                        // if num == target_num THEN
                        new IfStmt(new Position(11, 5),
                            new BinaryExpr(new Position(11, 8),
                                new VarExpr(new Position(11, 8), "num"),
                                BinaryOperator.EQUAL_EQUAL,
                                new VarExpr(new Position(11, 15), "target_num")
                            ),
                            new PrintStmt(new Position(12, 9), new LiteralExpr(new Position(12, 15), "Good job!")),
                            new BlockStmt(new Position(14, 5), new List<Stmt> {
                                    // IF target_num > num THEN
                                    new IfStmt(new Position(15, 9),
                                        new BinaryExpr(new Position(15, 12),
                                            new VarExpr(new Position(15, 12), "target_num"),
                                            BinaryOperator.GREATER,
                                            new VarExpr(new Position(15, 25), "num")
                                        ),
                                        new PrintStmt(new Position(16, 13),
                                            new LiteralExpr(new Position(16, 19), "Nope, guess higher.")
                                        ),
                                        new PrintStmt(new Position(18, 13),
                                            new LiteralExpr(new Position(18, 19), "Nope, guess lower.")
                                        )
                                    ),

                                    // LET current_try = current_try + 1
                                    new LetStmt(new Position(21, 9), "current_try",
                                        new BinaryExpr(new Position(21, 24),
                                            new VarExpr(new Position(21, 24), "current_try"),
                                            BinaryOperator.PLUS,
                                            new LiteralExpr(new Position(21, 38), 1.0)
                                        )
                                    ),

                                    // IF current_try > 5 THEN
                                    new IfStmt(new Position(22, 9),
                                        new BinaryExpr(new Position(22, 12),
                                            new VarExpr(new Position(22, 12), "current_try"),
                                            BinaryOperator.GREATER,
                                            new LiteralExpr(new Position(22, 26), 5.0)
                                        ),
                                        new BlockStmt(new Position(22, 32), new List<Stmt> {
                                                new PrintStmt(new Position(23, 13),
                                                    new LiteralExpr(new Position(23, 19),
                                                        "You didn't guess it after 5 tries... :("
                                                    )
                                                ),
                                                new BreakStmt(new Position(24, 13))
                                            }
                                        ),
                                        null
                                    )
                                }
                            )
                        )
                    }
                )
            ),

            // PRINT "Bye Bro."
            new PrintStmt(new Position(30, 1), new LiteralExpr(new Position(30, 7), "Bye Bro."))
        };

        double expectedRndVal = Math.Floor(new Random(42).NextDouble() * (100 - 0) + 0);  // 0 - 100

        double[] guesses =
            [expectedRndVal + 10, expectedRndVal - 10, expectedRndVal + 1, expectedRndVal - 1, expectedRndVal];
        string guessesStr = string.Join(Environment.NewLine, guesses);
        string[] expectedResponses = ["Nope, guess lower.", "Nope, guess higher.", "Nope, guess lower.", "Nope, guess higher.", "Good job!"];

        // Act & Assert
        mockConsoleInput(guessesStr, () => {
                var output = captureConsoleOutput(() => {
                        foreach (var stmt in statements) {
                            interpreter.interpretStatement(stmt);
                        }
                    }
                );

                // Split the output into lines for easier assertion
                var outputLines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                // Assert
                Assert.True(outputLines.Length >= 7, "Expected at least 8 lines of output");
                Assert.Matches($"DEBUG: the number is: {expectedRndVal}", outputLines[0]);
                for (int i = 0; i < guesses.Length; i++) {
                    Assert.Equal($"Guess a number from 1 to 100: {expectedResponses[i]}", outputLines[i + 1]);
                }
                Assert.Equal("Bye Bro.", outputLines[6]);
            }
        );
    }
}
