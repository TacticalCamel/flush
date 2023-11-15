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
	: KW_IN (KW_VOID | ParameterList=scriptParameterList)
	;

// 1.3.2 Kimeneti paraméterek
outParameters
	: KW_OUT (KW_VOID | ParameterList=scriptParameterList)
	;

// 2 Program test
programBody
	: (functionDefinition | typeDefinition | statement)*
	;

// 2.1 Függvény definíció
functionDefinition
	: Modifiers=functionModifiers ReturnType=returnType Name=ID HEAD_START ParameterList=parameterList HEAD_END Body=block
	;

functionModifiers
	: functionModifier*
	;

functionModifier
	: KW_PUBLIC
	;

// 2.2 Típus definíció
typeDefinition
	: shortTypeDefinition
	| longTypeDefinition
	;

shortTypeDefinition
	: Header=classHeader INDEX_START Body=parameterList INDEX_END
	;
	
longTypeDefinition
	: Header=classHeader BLOCK_START Body=classBody BLOCK_END
	;

classHeader
	: Modifiers=classModifiers KW_CLASS Name=ID (COLON inheritanceList)?
	;

inheritanceList
	: ((typeName PARAM_SEP)* typeName)?
	;

classBody
	: classMember*
	;

classMember
	: functionDefinition
	| propertyDefinition
	| constructorDefinition
	;
	
classModifiers
	: classModifier*
	;

classModifier
	: KW_PUBLIC
	;

propertyDefinition
	: Modifiers=propertyModifiers Type=typeName Name=ID STATEMENT_SEP
	;
	
propertyModifiers
	: propertyModifier*
	;
	
propertyModifier
	: KW_PUBLIC
	;

constructorDefinition
	: KW_NEW HEAD_START ParameterList=parameterList HEAD_END Body=block
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
	: KW_IF HEAD_START expression HEAD_END statement (KW_ELSE KW_IF HEAD_START expression HEAD_END statement)* (KW_ELSE statement)?
	;

// 2.3.3.3 For block
forBlock
	: KW_FOR HEAD_START variableDeclaration? STATEMENT_SEP expression? STATEMENT_SEP expression? HEAD_END statement (KW_ELSE statement)?
	| KW_FOR HEAD_START varWithType 'in' ID HEAD_END statement (KW_ELSE statement)?
	;

// 2.3.3.4 While block
whileBlock
	: KW_WHILE HEAD_START expression HEAD_END statement (KW_ELSE statement)?
	;

// 2.3.3.5 Try block
tryBlock
	: KW_TRY statement KW_CATCH HEAD_START varWithType HEAD_END statement
	;
	
// Kifejezés
expression
	: constant
	| functionCall
	| lambda
	| objectConstructor
	| collectionConstructor
	| ID
	| HEAD_START expression HEAD_END //#NestedExpression
	| expression opMemberAccess expression //#MemberAccessExpression
	| opNegate expression
	| opSign expression //#SignExpression
	| expression opUnary //#UnaryExpression
	| expression opMultiplicative expression //#MultiplicativeExpression
	| expression opAdditive expression //#AdditiveExpression
	| expression opShift expression //#ShiftExpression
	| expression opComparison expression //#ComparisonExpression
	| expression opLogical expression //#LogicalExpression
	| expression opAssignment expression //#AssigmentExpression
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

opNegate
	: '!'
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
	: Type=typeName Name=ID
	;

typeName
	: Name=ID #SimpleType
	| Name=ID '<' ContainedName=typeName '>' #GenericType
	;

returnType
	: typeName
	| KW_VOID
	;

parameterList
	: ((varWithType PARAM_SEP)* varWithType)?
	;
