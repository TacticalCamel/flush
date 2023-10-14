//file module
module example

//module imports
import math
import system
import auto

//in parameters
in f32 x, f32 y, f32 z

//out parameters
out i32 a, i32 b, i32 c

//variable declarations
i32 i = 16;
f32 j = 0.12;

string str = "this is a string";
char char_regular = 'a';
char char_escaped = '\n';
char char_unicode = '\u0032';

object custom_obj = object{
    Data = "this string",
    Length = 11,
    OnChange = (Event e) -> {
    	Length = Data.Length;
    	Console.Print(f"{Data} {Length}");
    }
};


//for block
for(i32 k = 0; k < 10; k++){
    Console.Print(k);
}
/*for(i32 k = 0; k < 10; k++) Console.Print(k);
for(i32 k = 0; k < 10; k++) ;

for(i32 k in array){
	Console.Print(k);
}*/

//if block
if(n > 10){
    n += 1 + 3 * 4;
}
else if(n < 0){
    n = 0;
}
else{
    n = null;
}

//while block
while(n > 0){
    n = example_function(n, 5);
    n = example_class.example_function(1, 2);
    n--;
}
else{
	n += 5;
	goto ret_label;
}

vec2 v = vec2{
	x = 1,
	y = 2
};

a = v.x;
b = v.y;
c = v.Length;
Console.Print(v);

label ret_label;
return;

//function definition
i32 example_function(i32 h, i32 g){
    /*
        multi
        line
        comment
    */
    return g * h; //line comment
}

//class definition
class vec2 {
	f32 x;
	f32 y;
	
	f32 Length(){
		return math.sqrt(x * x + y * y);
	}
}

//return statement
return -1;

