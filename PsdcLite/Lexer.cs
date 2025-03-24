namespace Scover.PsdcLite;

sealed class Lexer
{
    readonly string _input;
    readonly List<Token> _tokens = [];

    int _start = 0;
    int _i = 0;

    public Lexer(string source)
    {
        _input = source;
    }

    public List<Token> Lex()
    {
        while (!IsAtEnd) {
            _start = _i;
            ScanToken();
        }

        _tokens.Add(new Token(TokenType.Eof, _i));
        return _tokens;
    }

    void ScanToken()
    {
        char c = Advance();

        switch (c) {
        case ' ':
        case '\t':
        case '\r':
            // ignore
            break;

        case '(': Add(TokenType.LParen); break;
        case ')': Add(TokenType.RParen); break;
        case ';': Add(TokenType.Semi); break;
        case ':':
            if (Match('=')) Add(TokenType.Walrus);
            break;

        case '#':
            while (MatchComplement('\n')) ;
            break;

        case '"': String(); break;

        default:
            if (char.IsDigit(c)) {
                Number();
                break;
            }
            if (c == 'c' && Match('\'') && Match('e') && Match('s') && Match('t')) {
                Add(TokenType.Is);
                break;
            } else {
                _i = _start + 1;
            }

            if (char.IsLetter(c) || c == '_') {
                Word();
            }
            break;
        }
    }

    void String()
    {
        while (MatchComplement('"')) ;
        if (!IsAtEnd) Advance(); // closing quote
        Add(TokenType.String);
    }

    void Number()
    {
        while (Match(char.IsDigit)) ;

        if (Match('.')) {
            while (Match(char.IsDigit)) ;
        }

        Add(TokenType.Number);
    }

    void Word()
    {
        while (Match(c => c == '_' || char.IsLetterOrDigit(c))) ;


        var type = _input[_start.._i] switch {
            "programme" => TokenType.Program,
            "début" => TokenType.Begin,
            "fin" => TokenType.End,
            "ecrire" => TokenType.Print,
            _ => TokenType.Ident
        };

        Add(type);
    }

    bool Match(char expected)
    {
        if (IsAtEnd || _input[_i] != expected) return false;
        _i++;
        return true;
    }

    bool Match(Predicate<char> expected)
    {
        if (IsAtEnd || !expected(_input[_i])) return false;
        _i++;
        return true;
    }

    bool MatchComplement(char expected)
    {
        if (IsAtEnd || _input[_i] == expected) return false;
        _i++;
        return true;
    }

    char Peek() => IsAtEnd ? '\0' : _input[_i];
    char PeekNext() => (_i + 1 >= _input.Length) ? '\0' : _input[_i + 1];

    char Advance() => _input[_i++];

    void Add(TokenType type)
     => _tokens.Add(new Token(type, _start, type switch {
         TokenType.Ident or TokenType.Number => _input[_start.._i],
         TokenType.String => _input[(_start + 1)..(_i - 1)],
         _ => null
     }));

    bool IsAtEnd => _i >= _input.Length;
}
