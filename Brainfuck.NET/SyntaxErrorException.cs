﻿using System;

namespace Brainfuck.NET
{
    public class SyntaxErrorException : Exception
    {
        public SyntaxErrorException(string message)
            : base(message)
        { }
    }
}
