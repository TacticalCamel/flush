module examples

import core
import system

in string root_path, list<string> types
out bool success, i32 count

success = false;
count = 0;

list<string> paths = file.get_files(root_path): {
	on_error: (error e) -> {
		log("error: " + e.message);
		circuit.exit();
	}
};

for(string path in paths){
	if(file.is_directory(path)) skip;
	if(!types.contains(file.get_extension(path))) skip;
	
	file.delete(path);
	count++;
}

success = true;
