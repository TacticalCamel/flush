import abc
import abc

// dec, hex, bin literal
i8 x = 4;
i32 y = 4 - 0b111 * 0xffeeddcc;

// float literal
f32 z = 3.1415926;

// str, char literal
str str_normal = "this is a string" + '!';
str str_escaped = "escaped string\r\n" + '\t' + '\u0000';

// bool, null literal
bool b = true | false;
object o = null;