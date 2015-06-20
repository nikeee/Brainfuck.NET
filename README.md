# Brainfuck.NET

Compiles Brainfuck code to a .NET CIL executable.

usage:
```
bfnet infile.bf outfile.exe
```

You can also reference the generated exe from other CIL languages. Every generated exe exposes a function `void BrainfuckApplication.Run(string[])`. Of course, the `string[]` is pretty useless here.
