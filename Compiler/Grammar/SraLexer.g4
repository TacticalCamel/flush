lexer grammar SraLexer;

// Átugrott elem
WS
	: [ \t\r\n]+ -> skip
	;

LINE_COMMENT
	: '//' ~[\r\n]* -> skip
	;

BLOCK_COMMENT
	: '/*' .*? '*/' -> skip
	;

// Literál
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
	: NUMBER_SIGN? DEC_DIGIT+ '.' DEC_DIGIT+
	;

STRING_LIT
	: STRING_QUOTE .*? STRING_QUOTE
	;

CHAR_LIT
	: CHAR_QUOTE ESCAPE? . CHAR_QUOTE
	| CHAR_QUOTE ESCAPE ('u' | 'U') DEC_DIGIT+ CHAR_QUOTE
	;

// Elválasztó
PARAM_SEP
	: ','
	;

STATEMENT_SEP
	: ';'
	;

// Kulcsszó
KW_MODULE
	: 'module'
	;

KW_IMPORT
	: 'import'
	;

KW_AUTO
	: 'auto'
	;

KW_IN
	: 'in'
	;

KW_OUT
	: 'out'
	;

KW_NULL
	: 'null'
	;

KW_TRUE
	: 'true'
	;

KW_FALSE
	: 'false'
	;

KW_TRY
	: 'try'
	;

KW_CATCH
	: 'catch'
	;

KW_IF
	: 'if'
	;

KW_ELSE
	: 'else'
	;

KW_FOR
	: 'for'
	;

KW_WHILE
	: 'while'
	;

KW_CLASS
	: 'class'
	;

KW_BREAK
	: 'break'
	;

KW_CONTINUE
	: 'skip'
	;

KW_GOTO
	: 'goto'
	;

KW_LABEL
	: 'label'
	;	

KW_RETURN
	: 'return'
	;

// Zárójel
BLOCK_START
	: '{'
	;

BLOCK_END
	: '}'
	;

HEAD_START
	: '('
	;

HEAD_END
	: ')'
	;

INDEX_START
	: '['
	;

INDEX_END
	: ']'
	;

// Azonosítók
IDENTIFIER
	: [a-zA-Z_][a-zA-Z0-9_]*
	;

ANY
	: .
	;

// Fragment
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

fragment STRING_QUOTE
	: '"'
	;

fragment CHAR_QUOTE
	: '\''
	;

fragment ESCAPE
	: '\\'
	;
