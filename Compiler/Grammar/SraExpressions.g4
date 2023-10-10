grammar SraExpressions;

import SraLexer;

// Kifejezés
expression
	: constant
	| functionCall
	| objectConstructor
	| collectionConstructor
	| IDENTIFIER
	| HEAD_START expression HEAD_END
	| expression opMemberAccess expression
	| opSign expression
	| expression opUnary
	| expression opMultiplicative expression
	| expression opAdditive expression
	| expression opShift expression
	| expression opComparison expression
	| expression opLogical expression
	| expression opAssignment expression
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
functionCall
	: IDENTIFIER HEAD_START parameterList HEAD_END
	;

parameterList
	: ((expression PARAM_SEP)* expression)?
	;

// Objektum konstruktor
objectConstructor
	: IDENTIFIER BLOCK_START ((IDENTIFIER '=' expression PARAM_SEP)* (IDENTIFIER '=' expression))? BLOCK_END
	;

// Collekció konstruktor
collectionConstructor
	: INDEX_START parameterList INDEX_END
	;

// Operátor
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

opShift
	: '<<'
	| '>>'
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
