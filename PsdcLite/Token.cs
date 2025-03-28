using System.Diagnostics;

namespace Scover.PsdcLite;

readonly record struct Token(TokenType Type, int Start, string? Value = null)
{
    public int Length => Type switch {
        TokenType.Eof => 0,

        TokenType.Begin => 5,
        TokenType.End => 3,
        TokenType.Ident => Value.NotNull().Length,
        TokenType.Is => 5,
        TokenType.LParen => 1,
        TokenType.Number => Value.NotNull().Length,
        TokenType.Print => 6,
        TokenType.Program => 9,
        TokenType.RParen => 1,
        TokenType.Semi => 1,
        TokenType.String => Value.NotNull().Length + 2,
        TokenType.Walrus => 2,
        _ => throw new UnreachableException(),
    };
}

enum TokenType
{
    Eof,

    Begin,
    End,
    Ident,
    Is,
    LParen,
    Number,
    Print,
    Program,
    RParen,
    Semi,
    String,
    Walrus,
}
