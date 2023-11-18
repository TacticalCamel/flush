lexer grammar SraLiterals;

DEC_LIT
	: NUMBER_SIGN? DEC_DIGIT+
	;

HEX_LIT
	: NUMBER_SIGN? HEX_PREFIX HEX_DIGIT+
	;

BIN_LIT
	: NUMBER_SIGN? BIN_PREFIX BIN_DIGIT+
	; 

FLOAT_LIT
	: NUMBER_SIGN? DEC_DIGIT+ COMMA DEC_DIGIT+
	;

STRING_LIT
	: STRING_QUOTE ANY*? STRING_QUOTE
	;

CHAR_LIT
	: CHAR_QUOTE CHAR_ESCAPE? ANY CHAR_QUOTE
	| CHAR_QUOTE CHAR_ESCAPE UNICODE_PREFIX DEC_DIGIT+ CHAR_QUOTE
	;

fragment BIN_DIGIT
	: [01]
	;

fragment DEC_DIGIT
	: [0-9]
	;

fragment HEX_DIGIT
	: [0-9a-fA-F]
	;

fragment BIN_PREFIX
	: '0b'
	| '0B'
	;

fragment HEX_PREFIX
	: '0x'
	| '0X'
	;

fragment COMMA
	: '.'
	;

fragment NUMBER_SIGN
	: '+'
	| '-'
	;

fragment ANY
	: .
	;

fragment UNICODE_PREFIX
	: 'u'
	| 'U'
	;

fragment STRING_QUOTE
	: '"'
	;

fragment CHAR_QUOTE
	: '\''
	;

fragment CHAR_ESCAPE
	: '\\'
	;
