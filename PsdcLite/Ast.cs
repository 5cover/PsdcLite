using System.Collections.Immutable;

namespace Scover.PsdcLite;

public readonly record struct FixedRange(int Start, int End)
{
    /// <summary>
    /// Exclusive end bound of the range.
    /// </summary>
    public int Length => End - Start;
    public static implicit operator Range(FixedRange o) => o.Start..o.End;
}

interface Ast
{
    sealed record Algorithm(ImmutableArray<Decl> Body);

    interface Decl : Ast
    {
        sealed record Program(FixedRange Range, string Title, ImmutableArray<Stmt> Body) : Decl
        {
        }
    }

    interface Stmt : Ast
    {
        FixedRange Range { get; }
        sealed record Assignment(FixedRange Range, string Lhs, Expr Rhs) : Stmt
        {
        }
        sealed record Print(FixedRange Range, Expr Arg) : Stmt
        {
        }
    }
    interface Expr : Ast
    {
        FixedRange Range { get; }
        sealed record Variable(FixedRange Range, string Name) : Expr
        {
        }
        sealed record LiteralString(FixedRange Range, string Value) : Expr
        {
        }
        sealed record LiteralNumber(FixedRange Range, string Value) : Expr
        {
        }
    }
}
