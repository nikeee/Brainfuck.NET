# Brainfuck.NET [![Travis Build Status](https://travis-ci.org/nikeee/Brainfuck.NET.svg)](https://travis-ci.org/nikeee/Brainfuck.NET) [![Windows Build Status](https://ci.appveyor.com/api/projects/status/1veilf8a8768x0k7?svg=true)](https://ci.appveyor.com/project/nikeee/brainfuck-net)

Compiles Brainfuck code to a .NET CIL executable.

usage:
```
bfnet infile.bf outfile.exe
```

You can also reference the generated exe from other CIL languages. Every generated exe exposes a function `void BrainfuckApplication.Run(string[])`. Of course, the `string[]` is pretty useless here.
