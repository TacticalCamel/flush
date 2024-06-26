grammar Flush;

import FlKeywords, FlLiterals, FlSeparators, FlOperators, FlCommon;

program
	: Header=programHeader Body=programBody EOF
	;

programHeader
	: ModuleSegment=moduleSegment ImportSegment=importSegment
	;

moduleSegment
	: ModuleStatement=moduleStatement?
	;

moduleStatement
    : KW_MODULE Name=namespace
    ;

namespace
	: (id '.')* id
	;

importSegment
	: importStatement*
	;

importStatement
	: KW_IMPORT Name=KW_AUTO #AutoImport
	| KW_IMPORT Name=namespace #ManualImport
	;

programBody
	: (typeDefinition | statement)*
	;

typeDefinition
	: Modifiers=modifierList Keyword=(KW_STRUCT | KW_CLASS) TypeName=id (GenericParameters=genericParameters)? BLOCK_START Body=typeBody BLOCK_END
	;

typeBody
	: (fieldDefinition | constructorDefinition | methodDefinition)*
	;

fieldDefinition
	: Modifiers=modifierList Type=type Name=id STATEMENT_SEP
	;

constructorDefinition
	: Modifiers=modifierList TypeName=id HEAD_START ParameterList=parameterList HEAD_END Body=block
	;

methodDefinition
	: Modifiers=modifierList ReturnType=returnType Name=id HEAD_START ParameterList=parameterList HEAD_END Body=block
	;

genericParameters
	: OP_LESS (id PARAM_SEP)* id OP_GREATER
	;

statement
    : debugStatement STATEMENT_SEP
	| controlStatement STATEMENT_SEP
	| regularStatement STATEMENT_SEP
	| blockStatement
	| STATEMENT_SEP
	;

debugStatement
    : 'pause'
    ;

regularStatement
	: VariableDeclaration=variableDeclaration
	| Expression=expression
	;

variableDeclaration
	: VariableWithType=variableWithType (OP_ASSIGN Expression=expression)?
	;

controlStatement
	: KW_RETURN (Expression=expression)? #ReturnStatement
	| KW_BREAK #BreakStatement
	| KW_CONTINUE #ContinueStatement
	;

blockStatement
	: block
    | ifBlock
    | forBlock
    | whileBlock
    | tryBlock
	;

block
	: BLOCK_START statement* BLOCK_END
	;

ifBlock
	: ifBlockBody (KW_ELSE ifBlockBody)* (KW_ELSE ElseStatement=statement)?
	;

ifBlockBody
	: KW_IF HEAD_START Condition=expression HEAD_END Statement=statement
	;

forBlock
	: KW_FOR HEAD_START (StartStatement=variableDeclaration)? STATEMENT_SEP (Condition=expression)? STATEMENT_SEP (IterationExpression=expression)? HEAD_END Statement=statement
	;

whileBlock
	: KW_WHILE HEAD_START Condition=expression HEAD_END Statement=statement
	;

tryBlock
	: KW_TRY statement KW_CATCH HEAD_START variableWithType HEAD_END statement
	;
	
expression
	: KW_NULL #NullExpression
	| Constant=constant #ConstantExpression
	| Identifier=id #IdentifierExpression
	| Type=expression OP_MEMBER_ACCESS Member=id #MemberAccessExpression
	| HEAD_START Type=type HEAD_END Expression=expression #CastExpression
	| HEAD_START Body=expression HEAD_END #NestedExpression
	| KW_NEW Type=type HEAD_START Parameters=expressionList HEAD_END (FieldAssignmentList=fieldAssignmentList)? #ConstructorExpression
	| Caller=expression HEAD_START Parameters=expressionList HEAD_END #FunctionCallExpression
 	| INDEX_START Items=expressionList INDEX_END #CollectionExpression
	| Operator=opLeftUnary Expression=expression #LeftUnaryExpression
	| Expression=expression Operator=opRightUnary #RightUnaryExpression
	| Left=expression Operator=opMultiplicative Right=expression #MultiplicativeExpression
	| Left=expression Operator=opAdditive Right=expression #AdditiveExpression
	| Left=expression Operator=opShift Right=expression #ShiftExpression
	| Left=expression Operator=opComparison Right=expression #ComparisonExpression
	| Left=expression Operator=opLogical Right=expression #LogicalExpression
	| Left=expression Operator=opAssignment Right=expression #AssigmentExpression
	;

constant
	: DECIMAL_INTEGER #DecimalLiteral
	| HEXADECIMAL_INTEGER #HexadecimalLiteral
	| BINARY_INTEGER #BinaryLiteral
	| DOUBLE_FLOAT #DoubleFloat
	| SINGLE_FLOAT #SingleFloat
	| HALF_FLOAT #HalfFloat
	| STRING_LITERAL #StringLiteral
	| CHAR_LITERAL #CharLiteral
	| KW_TRUE #TrueKeyword
	| KW_FALSE #FalseKeyword
	;

expressionList
	: ((expression PARAM_SEP)* expression)?
	;

fieldAssignmentList
	: BLOCK_START ((fieldAssignment PARAM_SEP)* fieldAssignment)? BLOCK_END
	;
	
fieldAssignment
	: Name=id OP_ASSIGN Expression=expression
	;

opLeftUnary
	: OP_PLUS
	| OP_MINUS
	| OP_NOT
	| OP_INCREMENT
    | OP_DECREMENT
	;

opRightUnary
	: OP_INCREMENT
	| OP_DECREMENT
	;

opMultiplicative
	: OP_MULTIPLY
	| OP_DIVIDE
	| OP_MODULUS
	;

opAdditive
	: OP_PLUS
    | OP_MINUS
	;

opShift
	: OP_SHIFT_LEFT
	| OP_SHIFT_RIGHT
	;

opComparison
	: OP_EQ
	| OP_NOT_EQ
	| OP_LESS
	| OP_GREATER
	| OP_LESS_EQ
	| OP_GREATER_EQ
	;

opLogical
	: OP_AND
	| OP_OR
	| OP_XOR
	;

opAssignment
	: OP_ASSIGN
	| OP_MULTIPLY_ASSIGN
	| OP_DIVIDE_ASSIGN
	| OP_MODULUS_ASSIGN
	| OP_PLUS_ASSIGN
	| OP_MINUS_ASSIGN
	| OP_SHIFT_LEFT_ASSIGN
	| OP_SHIFT_RIGHT_ASSIGN
	| OP_AND_ASSIGN
	| OP_OR_ASSIGN
	| OP_XOR_ASSIGN
	;

variableWithType
	: Type=type Name=id
	;

type
	: Name=id #SimpleType
	| Name=id OP_LESS (type PARAM_SEP)* type OP_GREATER #GenericType
	| Type=type INDEX_START INDEX_END #ArrayType
	;

returnType
	: Type=type
	| Void=KW_VOID
	;

parameterList
	: ((variableWithType PARAM_SEP)* variableWithType)?
	;

modifierList
	: modifier*
	;

modifier
	: KW_PRIVATE
	;

id
	: ID
	| contextualKeyword
	;

contextualKeyword
	: KW_MODULE
	| KW_IMPORT
	| KW_IN
	| KW_CLASS
	| KW_PRIVATE
	;