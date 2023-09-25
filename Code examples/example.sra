//module imports
import moduleone
import moduletwo

//in parameters
in f32 x, f32 y, i32 z

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
    n *= 10;
    n++++++;
}
else{
    n = null;
}

//while block
while(false){
    n = example_function(n, 5);
    
    module.class.function(1, 2);
}

i32 example_function(i32 h, i32 g){
    /*
        multi
        line
        comment
    */
    return g * h; //line comment
}

object custom_obj = {
    attr1 = "this string",
    attr2 = 65,
    attr3 = null
};

//generic type
list<i32> list = [0, 1, 2, 3, 4];

//array
array<i32> = [5, 6, 7, 8];

return -1;
