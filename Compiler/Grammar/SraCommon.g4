lexer grammar SraCommon;

WS
	: [ \t\r\n]+ -> skip
	;

LINE_COMMENT
	: '//' ~[\r\n]* -> skip
	;

BLOCK_COMMENT
	: '/*' .*? '*/' -> skip
	;

// Azonosítók
ID
	: [a-zA-Z_][a-zA-Z0-9_]*
	;
