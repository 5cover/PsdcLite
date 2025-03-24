using System.Collections.Immutable;

namespace Scover.PsdcLite;

sealed record ParseError(string Subject, int Index, List<IReadOnlyCollection<TokenType>> Expected);
