module examples

import core
import auto

in i32 a, f32 b
out void


i32 x = 1 << 16;
i32 y = (x - 1) * 11 + 1;
f32 z = x / 3;

x++;
y -= x * 2; 
