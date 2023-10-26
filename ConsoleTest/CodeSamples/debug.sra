module debug

import core
import system 

in i32 limit
out null

for(i32 i = 0; i < limit; i++){
	print(i * 2 + 4 % 5);
}