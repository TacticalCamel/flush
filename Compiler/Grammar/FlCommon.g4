lexer grammar FlCommon;

WS
	: [ \t\r\n]+ -> skip
	;

LINE_COMMENT
	: '//' ~[\r\n]* -> skip
	;

BLOCK_COMMENT
	: '/*' .*? '*/' -> skip
	;

ID
	: [a-zA-Z_][a-zA-Z0-9_]*
	;

UNKNOWN
    : .
    ;