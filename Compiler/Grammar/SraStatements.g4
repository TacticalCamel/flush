grammar SraStatements;

import SraExpressions;

// Utasítás
statement
	: regularStatement STATEMENT_SEP
	| controlStatement STATEMENT_SEP
	| blockStatement
	;

// 1 Általános utasítás
regularStatement
	: variableDeclaration
	| expression
	;

// 1.1 Változó deklaráció
variableDeclaration
	: varWithType ('=' expression)?
	;

// 2 Vezérlési utasítás
controlStatement
	: returnStatement
	| breakStatement
	| continueStatement
	| gotoStatement
	| labelStatement
	;

// 2.1 Return utasítás
returnStatement
	: KW_RETURN expression?
	;

// 2.2 Break utasítás
breakStatement
	: KW_BREAK
	;

// 2.3 Skip utasítás
continueStatement
	: KW_CONTINUE
	;

// 2.4 Goto utasítás
gotoStatement
	: KW_GOTO IDENTIFIER
	;

// 2.5 Label utasítás
labelStatement
	: KW_LABEL IDENTIFIER
	;

// 3 Blokk utasítás
blockStatement
	: block
    | ifBlock
    | forBlock
    | whileBlock
    | tryBlock
	;

// 3.1 Blokk
block
	: BLOCK_START statement* BLOCK_END
	;

// 3.2 If blokk
ifBlock
	: KW_IF HEAD_START expression HEAD_END block (KW_ELSE KW_IF HEAD_START expression HEAD_END block)* (KW_ELSE block)?
	;

// 3.3 For block
forBlock
	: KW_FOR HEAD_START variableDeclaration? STATEMENT_SEP expression? STATEMENT_SEP expression? HEAD_END block (KW_ELSE block)?
	| KW_FOR HEAD_START varWithType 'in' IDENTIFIER HEAD_END block (KW_ELSE block)?
	;

// 3.4 While block
whileBlock
	: KW_WHILE HEAD_START expression HEAD_END block (KW_ELSE block)?
	;

// 3.5 Try block
tryBlock
	: KW_TRY block KW_CATCH HEAD_START varWithType HEAD_END block
	;

// Közös szabály
varWithType
	: IDENTIFIER IDENTIFIER
	;