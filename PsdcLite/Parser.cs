using System.Collections.Immutable;
using System.Diagnostics;

namespace Scover.PsdcLite;

static class Parser
{
    // Token type -> whether or not to consume the token
    static readonly IReadOnlyDictionary<TokenType, bool> syncPointsProgram = new Dictionary<TokenType, bool>() {
        [TokenType.End] = true,
        [TokenType.Program] = false,
    };
    static readonly IReadOnlyDictionary<TokenType, bool> syncPointsStmt = new Dictionary<TokenType, bool>() {
        [TokenType.Semi] = true,
    };

    public static ParseResult<Ast.Algorithm> Parse(IReadOnlyList<Token> tokens) => Algorithm(new(tokens, 0, ""));

    static ParseResult<Ast.Algorithm> Algorithm(ParsingContext ctx)
    {
        var o = ParseOperation.Start(ctx);
        o.ZeroOrMore(Program, TokenType.Eof, syncPointsProgram, out var decls);
        return o.Ok(new Ast.Algorithm(decls));
    }

    static ParseResult<Ast.Decl> Program(ParsingContext ctx)
    {
        var o = ParseOperation.Start(ctx with { Subject = "program" });
        o.One(TokenType.Program);
        if (!o.One(TokenType.Ident, out var title)) return o.Ko<Ast.Decl>();
        o.One(TokenType.Is);
        if (!o.One(Block, out var body)) return o.Ko<Ast.Decl>();
        return o.Ok<Ast.Decl>(new Ast.Decl.Program(o.Extent, title.Value.NotNull(), body));
    }

    // "Transparent" node : we inherit the subject, since a 'block' isn't really a meaningful syntactic construct for the user
    static ParseResult<ImmutableArray<Ast.Stmt>> Block(ParsingContext ctx)
    {
        var o = ParseOperation.Start(ctx);

        o.One(TokenType.Begin);

        o.ZeroOrMore(Stmt, TokenType.End, syncPointsStmt, out var stmts);

        o.One(TokenType.End);

        return o.Ok(stmts);
    }

    static ParseResult<Ast.Stmt> Stmt(ParsingContext ctx)
    {
        var o = ParseOperation.Start(ctx with { Subject = "statement" });
        if (!o.One([TokenType.Ident, TokenType.Print], out var head)) return o.Ko<Ast.Stmt>();
        switch (head) {
        case { Type: TokenType.Ident, Value: var lhs }: {
            o.One(TokenType.Walrus);
            if (!o.One(Expr, out var rhs)) return o.Ko<Ast.Stmt>();
            o.One(TokenType.Semi);
            return o.Ok<Ast.Stmt>(new Ast.Stmt.Assignment(o.Extent, lhs.NotNull(), rhs));
        }
        case { Type: TokenType.Print }: {
            o.One(TokenType.LParen);
            if (!o.One(Expr, out var arg)) return o.Ko<Ast.Stmt>();
            o.One(TokenType.RParen);
            o.One(TokenType.Semi);
            return o.Ok<Ast.Stmt>(new Ast.Stmt.Print(o.Extent, arg));
        }
        default: throw new UnreachableException();
        }
    }

    static ParseResult<Ast.Expr> Expr(ParsingContext ctx)
    {
        var o = ParseOperation.Start(ctx with { Subject = "expression" });
        if (!o.One([TokenType.Ident, TokenType.String, TokenType.Number], out var head)) return o.Ko<Ast.Expr>();
        return head switch {
            { Type: TokenType.Ident, Value: var v } => o.Ok<Ast.Expr>(new Ast.Expr.Variable(o.Extent, v.NotNull())),
            { Type: TokenType.String, Value: var v } => o.Ok<Ast.Expr>(new Ast.Expr.LiteralString(o.Extent, v.NotNull())),
            { Type: TokenType.Number, Value: var v } => o.Ok<Ast.Expr>(new Ast.Expr.LiteralNumber(o.Extent, v.NotNull())),
            _ => throw new UnreachableException(),
        };
    }
}
