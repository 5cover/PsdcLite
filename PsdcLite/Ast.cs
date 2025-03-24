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
    readonly record struct Algorithm(ImmutableArray<Decl> Body);

    interface Decl : Ast
    {
        readonly record struct Program(FixedRange Range, string Title, ImmutableArray<Stmt> Body) : Decl
        {
        }
    }

    interface Stmt : Ast
    {
        FixedRange Range { get; }
        readonly record struct Assignment(FixedRange Range, string Lhs, Expr Rhs) : Stmt
        {
        }
        readonly record struct Print(FixedRange Range, Expr Arg) : Stmt
        {
        }
    }
    interface Expr : Ast
    {
        FixedRange Range { get; }
        readonly record struct Variable(FixedRange Range, string Name) : Expr
        {
        }
        readonly record struct LiteralString(FixedRange Range, string Value) : Expr
        {
        }
        readonly record struct LiteralNumber(FixedRange Range, string Value) : Expr
        {
        }
    }
}
