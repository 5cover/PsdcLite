using System.Collections.Immutable;

namespace Scover.PsdcLite;

sealed record ParseError(int Index, string Subject, List<IReadOnlyCollection<TokenType>> Expected);

sealed class Parser(IReadOnlyList<Token> tokens, Action<ParseError> onError)
{
    readonly IReadOnlyList<Token> _tokens = tokens;
    readonly Action<ParseError> _onError = onError;
    int _i;

    public Ast.Algorithm? Parse()
    {
        var r = Algorithm();
        ReportError();
        return r;
    }

    Ast.Algorithm Algorithm()
    {
        var decls = ImmutableArray.CreateBuilder<Ast.Decl>();

        while (!IsAtEnd) {
            Ast.Decl? decl = Program();
            if (decl is null) Synchronize(TokenType.Program);
            else decls.Add(decl);
        }

        return new(decls.ToImmutable());
    }

    Ast.Decl.Program? Program()
    {
        int start = _i;
        const string Subject = "program";
        Expect(Subject, TokenType.Program);
        var title = Expect(Subject, TokenType.Ident);
        if (title is null) return null;
        Expect(Subject, TokenType.Is);
        var body = Block(Subject);

        return new(new(start, _i), title.Value.NotNull(), body);
    }

    // "Transparent" node : we inherit the subject, since a 'block' isn't really a meaningful syntactic construct for the user
    ImmutableArray<Ast.Stmt> Block(string subject)
    {
        Expect(subject, TokenType.Begin);
        var stmts = ImmutableArray.CreateBuilder<Ast.Stmt>();

        while (!IsAtEnd && Peek().Type is not TokenType.End) {
            var stmt = Stmt();
            if (stmt is null) Synchronize(TokenType.Semi);
            else stmts.Add(stmt);
        }

        Expect(subject, TokenType.End);
        return stmts.ToImmutable();
    }

    Ast.Stmt? Stmt()
    {
        const string Subject = "statement";
        int start = _i;

        switch (Expect(Subject, [TokenType.Ident, TokenType.Print])) {
        case { Type: TokenType.Ident, Value: var lhs }: {
            Expect(Subject, TokenType.Walrus);
            var rhs = Expr();
            if (rhs is null) return null;
            Expect(Subject, TokenType.Semi);
            return new Ast.Stmt.Assignment(new(start, _i), lhs.NotNull(), rhs);
        }
        case { Type: TokenType.Print }: {
            Expect(Subject, TokenType.LParen);
            var arg = Expr();
            if (arg is null) return null;
            Expect(Subject, TokenType.RParen);
            Expect(Subject, TokenType.Semi);
            return new Ast.Stmt.Print(new(start, _i), arg);
        }
        default: return null;
        }
    }

    Ast.Expr? Expr()
    {
        const string Subject = "expression";
        int start = _i;
        // a lot of duplication BUT it is intentional to keep open for extension
        return Expect(Subject, [TokenType.Ident, TokenType.String, TokenType.Number]) switch {
            { Type: TokenType.Ident, Value: var v } => new Ast.Expr.Variable(new(start, _i), v.NotNull()),
            { Type: TokenType.String, Value: var v } => new Ast.Expr.LiteralString(new(start, _i), v.NotNull()),
            { Type: TokenType.Number, Value: var v } => new Ast.Expr.LiteralNumber(new(start, _i), v.NotNull()),
            _ => null,
        };
    }

    // Helpers

    Token? Expect(string subject, TokenType type)
    {
        return Expect(subject, [type]); // fixme: provide optimized overload
    }

    Token? Expect(string subject, HashSet<TokenType> types)
    {
        // i gave up on the idea of making "fake" tokens. I don't want to build an AST with holes in it. If we miss syntax token, we can just pretend it's there. If we miss an AST-crucial token, we return an error result
        if (IsAtEnd || !types.Contains(Peek().Type)) {
            Error(subject, types);
            return null;
        }
        return Advance();
    }

    Token Advance() => _tokens[_i++];

    bool Check(TokenType type)
    {
        return Peek().Type == type;
    }

    Token Peek() => _tokens[_i];
    bool IsAtEnd => Peek().Type == TokenType.Eof;

    void Synchronize(TokenType to)
    {
        while (!IsAtEnd && _tokens[_i].Type != to) _i++;
    }

    int _iLastError = -1;
    ParseError _pendingError = new(-1, "", []);
    void Error(string subject, IReadOnlyCollection<TokenType> expected)
    {
        if (_iLastError == _i) {
            _pendingError.Expected.Add(expected);
        } else {
            ReportError();
            _pendingError = new(_iLastError = _i, subject, [expected]);
        }
    }

    void ReportError()
    {
        if (_pendingError.Index != -1) _onError(_pendingError);
    }
}
