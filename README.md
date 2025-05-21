USAGE:

Create JT file in same location and with same name as source file:  
*JTfy path/to/file.3dxml*

Change name and/or location of JT file:  
*JTfy --input path/to/file_name.3dxml --output different/path/to/new_file_name.jt*

Produce Standard (as opposed to Monolithic) JT file structure:  
*JTfy --input path/to/file.3dxml --monolithic False*

| Option           | Description                             |
| ---------------- | --------------------------------------- |
| -i, --input      | Required. Path to input, 3DXML, file    |
| -o, --output     | Path to output, JT, file                |
| -m, --monolithic | (Default: true) Produces single JT file |
| --help           | Display this help screen.               |
| --version        | Display version information.            |
