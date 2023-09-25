grammar Scranton;

// fragmentek
fragment DEC_DIGIT
	: [0-9]
	;

fragment HEX_DIGIT
	: [0-9a-fA-F]
	;

fragment BIN_DIGIT
	: [01]
	;

fragment HEX_PREFIX
	: '0x'
	| '0X'
	;

fragment BIN_PREFIX
	: '0b'
	| '0B'
	;
	
fragment NUMBER_SIGN
	: '+'
	| '-'
	;

fragment LETTER
	: [a-zA-Z]
	;
	
fragment UNDERSCORE
	: '_'
	;

// lexer szabályok
WS
	: [ \t\r\n]+ -> skip
	;
	
LINE_COMMENT
	: '//' ~[\r\n]* -> skip
	;
	
BLOCK_COMMENT
	: '/*' .*? '*/' -> skip
	;

PARAM_SEPARATOR
	: ','
	;

STATEMENT_SEPARATOR
	: ';'
	;

INT_DEC
	: NUMBER_SIGN? DEC_DIGIT+
	;
	
INT_HEX
	: NUMBER_SIGN? HEX_PREFIX HEX_DIGIT+
	;
	
INT_BIN
	: NUMBER_SIGN? BIN_PREFIX BIN_DIGIT+
	; 

FLOAT
	: NUMBER_SIGN? DEC_DIGIT+ '.' DEC_DIGIT+
	;
	
STRING
	: '"' .*? '"'
	;
	
CHAR
	: '\'' . '\''
	| '\'\\' . '\''
	| '\'\\' ('u' | 'U') [0-9]+  '\''
	;
	
NULL
	: 'null'
	;
	
TRUE
	: 'true'
	;

FALSE
	: 'false'
	;
	
IDENTIFIER
	: [a-zA-Z_][a-zA-Z0-9_]*
	;

// parser szabályok
program
	: import_segment parameter_segment code_segment EOF
	;
	
import_segment
	: import_statement*
	;

parameter_segment
	: in_parameters? out_parameters?
	| out_parameters? in_parameters?
	;

code_segment
	: (line | function_definition)*
	;

import_statement
	: 'import' IDENTIFIER
	| 'import' 'auto'
	;

in_parameters
	: 'in' (NULL | ((IDENTIFIER IDENTIFIER PARAM_SEPARATOR)* IDENTIFIER IDENTIFIER))
	;

out_parameters
	: 'out' (NULL | ((IDENTIFIER IDENTIFIER PARAM_SEPARATOR)* IDENTIFIER IDENTIFIER))
	;

line
	: statement STATEMENT_SEPARATOR
	| if_block
	| for_block
	| while_block
	| block
	;

function_definition
	: IDENTIFIER IDENTIFIER '(' ((IDENTIFIER IDENTIFIER PARAM_SEPARATOR)* (IDENTIFIER IDENTIFIER))? ')' block
	;

statement
	: variable_declaration
	| 'return' expression?
	| expression
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

block
	: '{' line* '}'
	;

variable_declaration
	: IDENTIFIER IDENTIFIER ('=' expression)?
	;
	
expression
	: constant
	| function_call
	| object_ctor
	| collection_ctor
	| IDENTIFIER
	| '(' expression ')'
	| expression op_member_access expression
	| op_sign expression
	| expression op_unary
	| expression op_multiplicative expression
	| expression op_additive expression
	| expression op_comparison expression
	| expression op_logical expression
	| expression op_assignment expression
	;
	
constant
	: FLOAT
	| INT_DEC
	| INT_HEX
	| INT_BIN
	| STRING
	| CHAR
	| NULL
	| TRUE
	| FALSE
	;

function_call
	: IDENTIFIER '(' ((expression PARAM_SEPARATOR)* expression)? ')'
	;
	
object_ctor
	: '{' ((IDENTIFIER '=' expression PARAM_SEPARATOR)* (IDENTIFIER '=' expression))? '}'
	;
	
collection_ctor
	: '[' ((expression PARAM_SEPARATOR)* expression)? ']'
	;

op_member_access
	: '.'
	;

op_sign
	: '+'
	| '-'
	;

op_unary
	: '++'
	| '--'
	;
	
op_multiplicative
	: '*'
	| '/'
	| '%'
	;
	
op_additive
	: '+'
	| '-'
	;
	
op_comparison
	: '=='
	| '!='
	| '>='
	| '<='
	| '>'
	| '<'
	;

op_logical
	: ('&' | 'and')
	| ('^' | 'xor')
	| ('|' | 'or')
	;
	
op_assignment
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
