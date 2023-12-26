grammar CompilerOptions;

STRING_LITERAL
	: '"' .*? '"'
	;

INT_LITERAL
	: [0-9]+
	;
	
PREFIX_LONG
	: '--'
	;
	
PREFIX_SHORT
	: '-'
	| '/'
	;

WORD
	: [a-zA-Z][a-zA-Z-_]*
	;

WS
	: [ \t\r\n]+ -> skip
	;

COMMENT
	: '/*' .*? '*/' -> skip
	;

UNKNOWN
	: .+?
	;

arguments
	: Options=option* EOF
	;

option
	: Key=key #BoolOption
	| Key=key STRING_LITERAL+ #StringOption
	| Key=key INT_LITERAL+ #IntOption
	;
	
key
	: PREFIX_SHORT Name=WORD #ShortKey
	| PREFIX_LONG Name=WORD #LongKey
	;
