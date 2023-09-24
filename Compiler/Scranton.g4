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

// 
IDENTIFIER
    : [a-zA-Z_][a-zA-Z0-9_]*
    ;

//
script
    : import_statement* inParams? out_params? line* EOF
    ;

import_statement
    : 'import' IDENTIFIER
    ;

inParams
    : 'in' (IDENTIFIER IDENTIFIER PARAM_SEPARATOR)* IDENTIFIER IDENTIFIER
    ;

out_params
    : 'out' (IDENTIFIER IDENTIFIER PARAM_SEPARATOR)* IDENTIFIER IDENTIFIER
    ;

line
    : block
    | if_block
    | for_block
    | while_block
    | statement STATEMENT_SEPARATOR
    | function_definition
    ;
  
block
    : '{' line* '}'
    ;
    
if_block
    : 'if' '(' expression ')' block ('else' 'if' '(' expression ')' block)* ('else' block)?
    ;
    
for_block
    : 'for' '(' variable_declaration? STATEMENT_SEPARATOR expression? STATEMENT_SEPARATOR expression? ')' block
    ;
    
while_block
    : 'while' '(' expression ')' block
    ;
    
statement
    : variable_declaration
    | return_statement
    | expression
    ;
    
return_statement
    : 'return' expression? 
    ; 

function_definition
    : IDENTIFIER IDENTIFIER '(' ((IDENTIFIER IDENTIFIER PARAM_SEPARATOR)* (IDENTIFIER IDENTIFIER))? ')' block
    ;
    
function_call
    : IDENTIFIER '(' ((expression PARAM_SEPARATOR)* expression)? ')'
    ;


variable_declaration
    : IDENTIFIER IDENTIFIER ('=' expression)?
    ;
    
constant
    : FLOAT
    | INT_DEC
    | INT_HEX
    | INT_BIN
    | STRING
    | CHAR
    | 'null'
    | 'true'
    | 'false'
    ;
    
expression
    : constant
    | IDENTIFIER
    | '(' expression ')'
    | expression member_access expression
    | sign_operator expression
    | expression unary_operator
    | expression multiplicative_operator expression
    | expression additive_operator expression
    | expression comparison_operator expression
    | expression logical_operator expression
    | expression assignment_operator expression
    | function_call
    ;

member_access
    : '.'
    ;

unary_operator
    : '++'
    | '--'
    ;
    
sign_operator
    : '+'
    | '-'
    ;
 
multiplicative_operator
    : '*'
    | '/'
    | '%'
    ;
additive_operator
    : '+'
    | '-'
    ;
    
comparison_operator
    : '=='
    | '!='
    | '>='
    | '<='
    | '>'
    | '<'
    ;

logical_operator
    : '|'
    | '&'
    | '^'
    ;
    
assignment_operator
    : '='
    | '+='
    | '-='
    | '*='
    | '/='
    | '%='
    ;
