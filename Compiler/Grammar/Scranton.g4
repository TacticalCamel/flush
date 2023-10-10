grammar Scranton;

import SraExpressions, SraStatements;

// Egy program felépítése
program
	: Header=programHeader Body=programBody EOF
	;

// 1 Program fejléc
programHeader
	: Module=moduleSegment Imports=importSegment Parameters=parameterSegment
	;

// 1.1 Modul szegmens
moduleSegment
	: (KW_MODULE Name=IDENTIFIER)?
	;

// 1.2 Import szegmens
importSegment
	: importStatement*
	;

importStatement
	: KW_IMPORT Name=(IDENTIFIER | KW_AUTO)
	;

// 1.3 Paraméter szegmens
parameterSegment
	: inParameters? outParameters?
	| outParameters? inParameters?
	;

scriptParameterList
	: (varWithType PARAM_SEP)* varWithType
	;

// 1.3.1 Bemeneti paraméterek
inParameters
	: KW_IN (KW_NULL | scriptParameterList)
	;

// 1.3.2 Kimeneti paraméterek
outParameters
	: KW_OUT (KW_NULL | scriptParameterList)
	;

// 2 Program test
programBody
	: (functionDef | classDef | statement)*
	;

// 2.1 Függvény definíció
functionDef
	: varWithType HEAD_START parameterList HEAD_END block
	;

parameterList
	: ((varWithType PARAM_SEP)* varWithType)?
	;

// 2.2 Osztály definíció
classDef
	: classHeader classBody
	;

classHeader
	: KW_CLASS IDENTIFIER
	;

classBody
	: BLOCK_START classMember* BLOCK_END
	;

classMember
	: functionDef
	| propertyDef
	;

propertyDef
	: varWithType STATEMENT_SEP
	;
