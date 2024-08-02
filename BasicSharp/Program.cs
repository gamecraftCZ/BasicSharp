// See https://aka.ms/new-console-template for more information

using BasicSharp.common;
using BasicSharp.interpreter;
using BasicSharp.lexer;
using BasicSharp.parser;

namespace BasicSharp;

static class Program {
    public static void Main(string[] args) {
        // Check if the user has provided a source code file
        if (args.Length != 1) {
            Console.WriteLine("""
                              Usage: 
                                Read from file: BasicSharp <sourcecode_filename>
                              """
            );
            Environment.Exit(1);
        }

        try {
            executeFromFile(args[0]);
        } catch (Exception e) {
            Console.Write("Unknown exception: ");
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            Environment.Exit(1);
        }
    }

    private static void executeFromFile(string filename) {
        try {
            // Open the source code file for reading if it exists
            if (!File.Exists(filename)) {
                Console.WriteLine($"Error: File '{filename}' not found.");
                Environment.Exit(1);
            }

            // Open sourcecode reader
            using Stream reader = File.OpenRead(filename);

            // Lex the source code to Tokens list
            IEnumerable<Token> tokensEnumerable = Lexer.lex(reader);

            // Parse the Tokens list to Abstract Syntax Tree (AST) for each separate executable statement
            IEnumerable<Stmt> stmtsEnumerable = Parser.parse(tokensEnumerable);

            // Interpret the parsed ASTs
            Interpreter interpreter = new();
            foreach (Stmt stmt in stmtsEnumerable) {
                interpreter.interpretStatement(stmt);
            }
        } catch (Exception e) {
            if (e is LexerError or ParserError or InterpreterError) {
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }

            throw;
        }
    }
}


