grammar PsdcLite;

algorithm
    : decl* EOF
    ;

decl
    : 'programme' Ident 'c\'est' block # main_program
    ;

block
    : 'début' stmt* 'fin'
    ;

stmt
    : Ident ':=' expr ';'       # assignment
    | 'ecrire' '(' expr ')' ';' # print
    ;

expr
    : Ident  # variable
    | String # literal_string
    | Number # literal_number
    ;

Ws
    : [\p{White_Space}]+ -> skip
    ;

Comment
    : (
        '#' ~[\n] '\n'
    ) -> skip
    ;

Ident
    : [\p{L}_][\p{L}_0-9]*
    ;

String
    : '"' ~["\\\r\n]* '"'
    ;

Number
    : [0-9]* '.' [0-9]+
    ;