grammar SraExpressions;

import SraLexer;

// Kifejezés
expression
	: constant
	| function_call
	| object_ctor
	| collection_ctor
	| IDENTIFIER
	| HEAD_START expression HEAD_END
	| expression op_member_access expression
	| op_sign expression
	| expression op_unary
	| expression op_multiplicative expression
	| expression op_additive expression
	| expression op_shift expression
	| expression op_comparison expression
	| expression op_logical expression
	| expression op_assignment expression
	;

// Konstans
constant
	: FLOAT_LIT
	| DEC_LIT
	| HEX_LIT
	| BIN_LIT
	| STRING_LIT
	| CHAR_LIT
	| KW_NULL
	| KW_TRUE
	| KW_FALSE
	;

// Függvényhívás
function_call
	: IDENTIFIER HEAD_START param_list HEAD_END
	;

param_list
	: ((expression PARAM_SEP)* expression)?
	;

// Objektum konstruktor
object_ctor
	: IDENTIFIER BLOCK_START ((IDENTIFIER '=' expression PARAM_SEP)* (IDENTIFIER '=' expression))? BLOCK_END
	;

// Collekció konstruktor
collection_ctor
	: INDEX_START param_list INDEX_END
	;

// Operátor
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

op_shift
	: '<<'
	| '>>'
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
	| '*='
	| '/='
	| '%='
	| '+='
	| '-='
	| '<<='
	| '>>='
	| '&='
	| '^='
	| '|='
	;
