using System.Collections.Immutable;
using Scover.Options;

namespace Scover.PsdcLite;

static class ParseResult
{
    public static ParseResult<T> Ok<T>(string subject, int read, ImmutableArray<ParseError> errors, T value)
     => new ParseResultImpl<T>(subject, read, errors, true, value);

    public static ParseResult<T> Ko<T>(string subject, int read, ImmutableArray<ParseError> errors)
     => new ParseResultImpl<T>(subject, read, errors, false, default);

    readonly record struct ParseResultImpl<T>(
        string? Subject,
        int Read,
        ImmutableArray<ParseError> Errors,
        bool HasValue,
        T? Value) : ParseResult<T>;
}

interface ParseResult<out T> : Option<T>
{
    string? Subject { get; }
    int Read { get; }
    ImmutableArray<ParseError> Errors { get; }
}
