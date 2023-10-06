//file module
module test

//module imports
import auto
import module_one
import module_two

//in parameters
in f32 x, f32 y, f32 z

//out parameters
out i32 a, i32 b, i32 c

//variable declarations
string str = "this is a string\n";
string str2;
char ch = '\t';
ch = '\u2224';
f32 n = 0;

//for block
for(i32 i = 0; i < 10; i++){
    n++;
}

//if block
if(n > 10){
    n /= 1 + 3 * 4;
}
else if(n < 0){
    n *= -01.234;
    n++++++;
}
else{
    n = null;
}

//while block
while(false){
    n = example_function(n, 5);
    
    _module._class._function(1, 2);
    
    break;
}

//function definition
i32 example_function(i32 h, i32 g){
    /*
        multi
        line
        comment
    */
    return g * h; //line comment
}

//object initializer
object custom_obj = obj_type{
    attr1 = "this string",
    attr2 = 65,
    attr3 = null
};

//array initializer
array arr = [0, 1, 2, 3, 4 * 5 - -3];

//class definition
class vec2 {
	f32 x;
	f32 y;
	
	f32 length(){
		return math.sqrt(x * x + y * y);
	}
	
}

//return statement
return -1;

