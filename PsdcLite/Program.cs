// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using Scover.PsdcLite;

class Program
{
    static void Main(string[] args)
    {
        string input = File.ReadAllText("program.txt");

        Lexer l = new(input);
        var tokens = l.Lex();

        Queue<Token> errorTokens = new();

        Parser p = new(tokens, ReportError);
        var ast = p.Parse();

        int resetAt = -1;
        for (int i = 0; i < input.Length; i++) {
            if (i == resetAt) {
                Console.ResetColor();
            }
            if (errorTokens.Count > 0 && i == errorTokens.Peek().Start) {
                Console.ForegroundColor = ConsoleColor.Red;
                resetAt = i + errorTokens.Dequeue().Length;
            }
            Console.Write(input[i]);
        }

        if (ast is not null) {
            Console.WriteLine();
            PrettyPrint(ast);
        }

        void ReportError(ParseError e)
        {
            var t = tokens[e.Index];
            errorTokens.Enqueue(t);
            var pos = input.GetPositionAt(t.Start);
            Console.Error.WriteLine($"syntax error at {pos}: expected {string.Join(", then ",
                e.Expected.Select(e => string.Join(" or ", e)))} for {e.Subject}, found {t.Type}");
        }
    }

    static void PrettyPrint(Ast.Algorithm alg)
    {
        Console.WriteLine($"algorithm ({alg.Body.Length}) decls");
        foreach (var decl in alg.Body) {
            PrettyPrint(decl);
        }
    }

    static void PrettyPrint(Ast.Decl decl)
    {
        switch (decl) {
        case Ast.Decl.Program prog:
            PrintLn(1, $"program {prog.Title} ({prog.Body.Length}) stmts");
            foreach (var stmt in prog.Body) {
                PrettyPrint(2, stmt);
            }
            break;
        default: throw new UnreachableException();
        }
    }

    static void PrettyPrint(int lvl, Ast.Stmt stmt)
    {
        switch (stmt) {
        case Ast.Stmt.Assignment a: PrintLn(lvl, $"{a.Lhs} := {ToString(a.Rhs)};"); break;
        case Ast.Stmt.Print p: PrintLn(lvl, $"ecrire({ToString(p.Arg)});"); break;
        default: throw new UnreachableException();
        }
    }

    static void PrintLn(int lvl, string str)
    {
        Console.Write(new string(' ', lvl * 4));
        Console.WriteLine(str);
    }

    static string ToString(Ast.Expr expr) => expr switch {
        Ast.Expr.LiteralNumber l => l.Value,
        Ast.Expr.LiteralString l => $"\"{l.Value}\"",
        Ast.Expr.Variable v => v.Name,
        _ => throw new UnreachableException(),
    };
}
