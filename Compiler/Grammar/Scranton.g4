grammar Scranton;

import SraLexer;

// Egy program felépítése
program
	: Header=programHeader Body=programBody EOF
	;

// 1 Program fejléc
programHeader
	: moduleSegment importSegment parameterSegment
	;

// 1.1 Modul szegmens
moduleSegment
	: (KW_MODULE Name=ID)?
	;

// 1.2 Import szegmens
importSegment
	: importStatement*
	;

importStatement
	: KW_IMPORT Name=KW_AUTO #AutoImport
	| KW_IMPORT Name=ID #ManualImport
	;

// 1.3 Paraméter szegmens
parameterSegment
	: InParameters=inParameters? OutParameters=outParameters?
	| OutParameters=outParameters? InParameters=inParameters?
	;

scriptParameterList
	: (varWithType PARAM_SEP)* varWithType
	;

// 1.3.1 Bemeneti paraméterek
inParameters
	: KW_IN (KW_NULL | ParameterList=scriptParameterList)
	;

// 1.3.2 Kimeneti paraméterek
outParameters
	: KW_OUT (KW_NULL | ParameterList=scriptParameterList)
	;

// 2 Program test
programBody
	: (functionDefinition | classDefinition | statement)*
	;

// 2.1 Függvény definíció
functionDefinition
	: functionModifiers varWithType HEAD_START parameterList HEAD_END block
	;

functionModifiers
	: functionModifier*
	;

functionModifier
	: 'private'
	;

// 2.2 Osztály definíció
classDefinition
	: classHeader classBody
	;

classHeader
	: KW_CLASS Name=ID
	;

classBody
	: BLOCK_START classMember* BLOCK_END
	;

classMember
	: functionDefinition
	| propertyDef
	;

propertyDef
	: Type=variableType Name=ID STATEMENT_SEP
	;

// 2.3 Utasítás
statement
	: regularStatement STATEMENT_SEP
	| controlStatement STATEMENT_SEP
	| blockStatement
	;

// 2.3.1 Általános utasítás
regularStatement
	: variableDeclaration
	| expression
	;

// 2.3.1.1 Változó deklaráció
variableDeclaration
	: varWithType ('=' expression)?
	;

// 2.3.2 Vezérlési utasítás
controlStatement
	: KW_RETURN expression? #ReturnStatement
	| KW_BREAK #BreakStatement
	| KW_CONTINUE #ContinueStatement
	| KW_GOTO Name=ID #GotoStatement
	| KW_LABEL Name=ID #LabelStatement
	;

// 2.3.3 Blokk utasítás
blockStatement
	: block
    | ifBlock
    | forBlock
    | whileBlock
    | tryBlock
	;

// 2.3.3.1 Blokk
block
	: BLOCK_START statement* BLOCK_END
	;

// 2.3.3.2 If blokk
ifBlock
	: KW_IF HEAD_START expression HEAD_END block (KW_ELSE KW_IF HEAD_START expression HEAD_END block)* (KW_ELSE block)?
	;

// 2.3.3.3 For block
forBlock
	: KW_FOR HEAD_START variableDeclaration? STATEMENT_SEP expression? STATEMENT_SEP expression? HEAD_END block (KW_ELSE block)?
	| KW_FOR HEAD_START varWithType 'in' ID HEAD_END block (KW_ELSE block)?
	;

// 2.3.3.4 While block
whileBlock
	: KW_WHILE HEAD_START expression HEAD_END block (KW_ELSE block)?
	;

// 2.3.3.5 Try block
tryBlock
	: KW_TRY block KW_CATCH HEAD_START varWithType HEAD_END block
	;
	
// Kifejezés
expression
	: constant
	| functionCall
	| lambda
	| objectConstructor
	| collectionConstructor
	| ID
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
	: ID HEAD_START expressionList HEAD_END
	;

expressionList
	: ((expression PARAM_SEP)* expression)?
	;

// Objektum konstruktor
objectConstructor
	: ID BLOCK_START ((ID '=' expression PARAM_SEP)* (ID '=' expression))? BLOCK_END
	;

// Collekció konstruktor
collectionConstructor
	: INDEX_START expressionList INDEX_END
	;

// Lambda
lambda
	: HEAD_START parameterList HEAD_END '->' (block | expression) 
	;

// Operátor
opMemberAccess
	: '.' #MemberAccessOperator
	;

opSign
	: '+' #PlusSignOperator
	| '-' #MinusSignOperator
	;

opUnary
	: '++' #IncrementOperator
	| '--' #DecrementOperator
	;

opMultiplicative
	: '*' #MultiplyOperator
	| '/' #DivideOperator
	| '%' #ModulusOperator
	;

opAdditive
	: '+' #AddOperator
	| '-' #SubtractOperator
	;

opShift
	: '<<' #ShiftLeftOperator
	| '>>' #ShiftRightOperator
	;

opComparison
	: '==' #EqualOperator
	| '!=' #NotEqualOperator
	| '>=' #GreaterOrEqualOperator
	| '<=' #LessOrEqualOperator
	| '>' #GreaterThanOperator
	| '<' #LessThanOperator
	;

opLogical
	: ('&' | 'and') #AndOperator
	| ('^' | 'xor') #XorOperator
	| ('|' | 'or') #OrOperator
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

// Közös szabály
varWithType
	: Type=variableType Name=ID
	;

variableType
	: ID #SimpleType
	| ID '<' varWithType '>' #GenericType
	;

parameterList
	: ((varWithType PARAM_SEP)* varWithType)?
	;
