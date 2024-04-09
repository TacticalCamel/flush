import auto

/*for(i8 i = 0; i < 10; i += 1){
	
}*/

i8 i = 0;

while(i < 10){
	i += 2;
	
	if(i < 5) i -= 1;
}


/*
struct v2{
	f32 x;
	f32 y;
	
	v2(f32 x, f32 y){}
	
	private f32 length(){}
}

struct v4{
	v2 a;
	v2 b;
}

struct circle{
	circle f;
}

class gen<T1, T2>{
	T1 a;
	T2 b;
	
	T1 setA(T1 _a){}
}
*/