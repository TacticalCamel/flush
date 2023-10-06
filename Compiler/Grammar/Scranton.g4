grammar Scranton;

// Egy program felépítése
program
	: program_header program_body EOF
	;

// 1 Program fejléc
program_header
	: module_segment import_segment parameter_segment
	;

// 1.1 Modul szegmens
module_segment
	: (KW_MODULE var_name)?
	;

// 1.2 Import szegmens
import_segment
	: import_statement*
	;

import_statement
	: KW_IMPORT var_name
	| KW_IMPORT KW_AUTO
	;

// 1.3 Paraméter szegmens
parameter_segment
	: in_parameters? out_parameters?
	| out_parameters? in_parameters?
	;

script_parameter_list
	: (var_with_type PARAM_SEP)* var_with_type
	;

// 1.3.1 Bemeneti paraméterek
in_parameters
	: KW_IN (KW_NULL | script_parameter_list)
	;

// 1.3.2 Kimeneti paraméterek
out_parameters
	: KW_OUT (KW_NULL | script_parameter_list)
	;

// 2 Program test
program_body
	: (global_function_def | class_def | statement)*
	;

// 2.1 Függvény definíció
global_function_def
	: var_with_type HEAD_START function_def_param_list HEAD_END block
	;

// 2.2 Osztály definíció
class_def
	: class_header class_body
	;

class_header
	: KW_CLASS var_type
	;

class_body
	: BLOCK_START class_member* BLOCK_END
	;

class_member
	: method_def
	| property_def
	;

method_def
	: var_with_type HEAD_START function_def_param_list HEAD_END block
	;

property_def
	: var_with_type ST_SEP
	;

// 2.3 Utasítás
statement
	: control_statement ST_SEP
	| regular_statement ST_SEP
	| block_statement
	;

// 2.3.1 Vezérlési utasítás
control_statement
	: return_statement
	| break_statement
	| skip_statement
	| goto_statement
	| label_statement
	;

// 2.3.1.1 Return utasítás
return_statement
	: KW_RETURN expression?
	;

// 2.3.1.2 Break utasítás
break_statement
	: KW_BREAK
	;

// 2.3.1.3 Skip utasítás
skip_statement
	: KW_SKIP
	;

// 2.3.1.4 Goto utasítás
goto_statement
	: KW_GOTO var_name
	;

// 2.3.1.5 Label utasítás
label_statement
	: KW_LABEL var_name
	;

// 2.3.2 Blokk utasítás
block_statement
	: block
    | if_block
    | for_block
    | while_block
    | try_block
	;

// 2.3.2.1 Blokk
block
	: BLOCK_START statement* BLOCK_END
	;

// 2.3.2.2 If blokk
if_block
	: KW_IF HEAD_START expression HEAD_END block (KW_ELSE KW_IF HEAD_START expression HEAD_END block)* (KW_ELSE block)?
	;

// 2.3.2.3 For block
for_block
	: KW_FOR HEAD_START variable_declaration? ST_SEP expression? ST_SEP expression? HEAD_END block (KW_ELSE block)?
	| KW_FOR HEAD_START var_with_type 'in' var_name HEAD_END block (KW_ELSE block)?
	;

// 2.3.2.4 While block
while_block
	: KW_WHILE HEAD_START expression HEAD_END block (KW_ELSE block)?
	;

// 2.3.2.5 Try block
try_block
	: KW_TRY block KW_CATCH HEAD_START var_with_type HEAD_END block
	;

// 2.3.3 Általános utasítás
regular_statement
	: variable_declaration
	| expression
	;

// 2.3.3.1 Változó deklaráció
variable_declaration
	: var_with_type (OP_EQ expression)?
	;

// 2.3.3.2 Kifejezés
expression
	: constant
	| function_call
	| object_ctor
	| collection_ctor
	| var_name
	| HEAD_START expression HEAD_END
	| expression op_member_access expression
	| op_sign expression
	| expression op_unary
	| expression op_multiplicative expression
	| expression op_additive expression
	| expression op_shift expression
	| expression op_comparison expression
	| expression op_logical expression
	| expression op_assignment expression
	;

// 2.3.3.2.1 Konstans
constant
	: FLOAT
	| INT_DEC
	| INT_HEX
	| INT_BIN
	| STRING_LITERAL
	| CHAR_LITERAL
	| KW_NULL
	| KW_TRUE
	| KW_FALSE
	;

// 2.3.3.2.2 Függvényhívás
function_call
	: var_name HEAD_START expression_param_list HEAD_END
	;

// 2.3.3.2.3
object_ctor
	: var_type BLOCK_START object_ctor_param_list BLOCK_END
	;

// 2.3.3.2.4
collection_ctor
	: INDEX_START expression_param_list INDEX_END
	;

function_def_param_list
	: ((var_with_type PARAM_SEP)* var_with_type)?
	;

expression_param_list
	: ((expression PARAM_SEP)* expression)?
	;

object_ctor_param_list
	: ((object_ctor_param PARAM_SEP)* object_ctor_param)?
	;

object_ctor_param
	: var_name OP_EQ expression
	;

var_with_type
	: var_type var_name
	;

var_type
	: IDENTIFIER
	;

var_name
	: IDENTIFIER
	;

op_member_access
	: '.'
	;

op_sign
	: '+'
	| '-'
	;

op_unary
	: '++'
	| '--'
	;

op_multiplicative
	: '*'
	| '/'
	| '%'
	;

op_additive
	: '+'
	| '-'
	;

op_shift
	: '<<'
	| '>>'
	;

op_comparison
	: '=='
	| '!='
	| '>='
	| '<='
	| '>'
	| '<'
	;

op_logical
	: ('&' | 'and')
	| ('^' | 'xor')
	| ('|' | 'or')
	;

op_assignment
	: OP_EQ
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

// lexer szabályok
WS
	: [ \t\r\n]+ -> skip
	;
	
LINE_COMMENT
	: '//' ~[\r\n]* -> skip
	;
	
BLOCK_COMMENT
	: '/*' .*? '*/' -> skip
	;

PARAM_SEP
	: ','
	;

ST_SEP
	: ';'
	;

INT_DEC
	: NUMBER_SIGN? DEC_DIGIT+
	;
	
INT_HEX
	: NUMBER_SIGN? HEX_PREFIX HEX_DIGIT+
	;
	
INT_BIN
	: NUMBER_SIGN? BIN_PREFIX BIN_DIGIT+
	; 

FLOAT
	: NUMBER_SIGN? DEC_DIGIT+ '.' DEC_DIGIT+
	;
	
STRING_LITERAL
	: '"' .*? '"'
	;
	
CHAR_LITERAL
	: '\'' . '\''
	| '\'\\' . '\''
	| '\'\\' ('u' | 'U') [0-9]+  '\''
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

KW_TRY
	: 'try'
	;

KW_CATCH
	: 'catch'
	;
	
KW_IF
	: 'if'
	;
	
KW_FOR
	: 'for'
	;
	
KW_WHILE
	: 'while'
	;

KW_ELSE
	: 'else'
	;
	
KW_MODULE
	: 'module'
	;

KW_CLASS
	: 'class'
	;

KW_RETURN
	: 'return'
	;

KW_BREAK
	: 'break'
	;

KW_SKIP
	: 'skip'
	;

KW_GOTO
	: 'goto'
	;

KW_LABEL
	: 'label'
	;
	
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

OP_EQ
	: '='
	;

IDENTIFIER
	: (LETTER | UNDERSCORE)(LETTER | UNDERSCORE | DEC_DIGIT)*
	;

// fragmentek
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
