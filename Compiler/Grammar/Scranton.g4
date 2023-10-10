grammar Scranton;

import SraExpressions;

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
	: (KW_MODULE IDENTIFIER)?
	;

// 1.2 Import szegmens
import_segment
	: import_statement*
	;

import_statement
	: KW_IMPORT IDENTIFIER
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
	: (function_def | class_def | statement)*
	;

// 2.1 Függvény definíció
function_def
	: var_with_type HEAD_START parameter_list HEAD_END block
	;

parameter_list
	: ((var_with_type PARAM_SEP)* var_with_type)?
	;

// 2.2 Osztály definíció
class_def
	: class_header class_body
	;

class_header
	: KW_CLASS IDENTIFIER
	;

class_body
	: BLOCK_START class_member* BLOCK_END
	;

class_member
	: function_def
	| property_definition
	;

property_definition
	: var_with_type STATEMENT_SEP
	;

// 2.3 Utasítás
statement
	: regular_statement STATEMENT_SEP
	| control_statement STATEMENT_SEP
	| block_statement
	;

// 2.3.1 Vezérlési utasítás
control_statement
	: return_statement
	| break_statement
	| continue_statement
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
continue_statement
	: KW_CONTINUE
	;

// 2.3.1.4 Goto utasítás
goto_statement
	: KW_GOTO IDENTIFIER
	;

// 2.3.1.5 Label utasítás
label_statement
	: KW_LABEL IDENTIFIER
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
	: KW_FOR HEAD_START variable_declaration? STATEMENT_SEP expression? STATEMENT_SEP expression? HEAD_END block (KW_ELSE block)?
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
	: var_with_type ('=' expression)?
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