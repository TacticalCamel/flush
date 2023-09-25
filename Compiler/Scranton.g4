grammar Scranton;

// átugrott tokenek
WS
    : [ \t\r\n]+ -> skip
    ;
    
LINE_COMMENT
    : '//' ~[\r\n]* -> skip
    ;
    
BLOCK_COMMENT
    : '/*' .*? '*/' -> skip
    ;

// elválasztók
PARAM_SEPARATOR
    : ','
    ;
STATEMENT_SEPARATOR
    : ';'
    ;

// konstans értékek
INT_DEC
    : [+-]? [0-9]+
    ;
    
INT_HEX
    : [+-]? '0x' [0-9a-fA-F]+
    ;
    
INT_BIN
    : [+-]? '0b' [01]+
    ; 

FLOAT
    : [+-]? [0-9]+ '.' [0-9]+
    ;

STRING
    : '"' .*? '"'
    ;
    
CHAR
    : '\'' . '\''
    | '\'\\' . '\''
    | '\'\\' ('u' | 'U') [0-9]+  '\''
    ;
    
CONST_KEYWORDS
    : 'true'
    | 'false'
    | 'null'
    ;

// azonósítók
IDENTIFIER
    : [a-zA-Z_][a-zA-Z0-9_]*
    ;

// gyökérelem és nagyobb építőblokkok
script
    : importStatement* inParams? outParams? line* EOF
    ;

importStatement
    : 'import' IDENTIFIER
    ;

inParams
    : 'in' (IDENTIFIER IDENTIFIER PARAM_SEPARATOR)* IDENTIFIER IDENTIFIER
    ;

outParams
    : 'out' (IDENTIFIER IDENTIFIER PARAM_SEPARATOR)* IDENTIFIER IDENTIFIER
    ;

line
    : statement STATEMENT_SEPARATOR
    | block
    | ifBlock
    | forBlock
    | whileBlock
    | functionDefinition
    ;
    
// vezérlési szerkezetek
block
    : '{' line* '}'
    ;
    
ifBlock
    : 'if' '(' expression ')' block ('else' 'if' '(' expression ')' block)* ('else' block)?
    ;
    
forBlock
    : 'for' '(' variableDeclaration? STATEMENT_SEPARATOR expression? STATEMENT_SEPARATOR expression? ')' block
    ;
    
whileBlock
    : 'while' '(' expression ')' block
    ;

// többi top level
statement
    : variableDeclaration
    | 'return' expression?
    | expression
    ;    

functionDefinition
    : IDENTIFIER IDENTIFIER '(' ((IDENTIFIER IDENTIFIER PARAM_SEPARATOR)* (IDENTIFIER IDENTIFIER))? ')' block
    ;

variableDeclaration
    : IDENTIFIER IDENTIFIER ('=' expression)?
    ;

// operátorok
opMemberAccess
    : '.'
    ;

opSign
    : '+'
    | '-'
    ;

opUnary
    : '++'
    | '--'
    ;
    
opMultiplicative
    : '*'
    | '/'
    | '%'
    ;
    
opAdditive
    : '+'
    | '-'
    ;
    
opComparison
    : '=='
    | '!='
    | '>='
    | '<='
    | '>'
    | '<'
    ;

opLogical
    : ('&' | 'and')
    | ('^' | 'xor')
    | ('|' | 'or')
    ;
    
opAssignment
    : '='
    | '+='
    | '-='
    | '*='
    | '/='
    | '%='
    | '&='
    | '^='
    | '|='
    ;

// kifejezések
expression
    : constant
    | functionCall
    | objectCtor
    | collectionCtor
    | IDENTIFIER
    | '(' expression ')'
    | expression opMemberAccess expression
    | opSign expression
    | expression opUnary
    | expression opMultiplicative expression
    | expression opAdditive expression
    | expression opComparison expression
    | expression opLogical expression
    | expression opAssignment expression
    ;

genericType
    : IDENTIFIER '<' IDENTIFIER '>'
    ;

constant
    : FLOAT
    | INT_DEC
    | INT_HEX
    | INT_BIN
    | STRING
    | CHAR
    | CONST_KEYWORDS
    ;

functionCall
    : IDENTIFIER '(' ((expression PARAM_SEPARATOR)* expression)? ')'
    ;
    
objectCtor
    : '{' ((IDENTIFIER '=' expression PARAM_SEPARATOR)* (IDENTIFIER '=' expression))? '}'
    ;
    
collectionCtor
    : '[' ((expression PARAM_SEPARATOR)* expression)? ']'
    ;
