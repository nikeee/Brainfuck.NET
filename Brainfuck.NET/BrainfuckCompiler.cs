using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Brainfuck.NET
{
    class BrainfuckCompiler
    {
        public int ApplicationMemory { get; private set; }

        #region Ctors

        public BrainfuckCompiler()
            : this(30000)
        { }

        public BrainfuckCompiler(int applicationMemory)
        {
            ApplicationMemory = applicationMemory;
            _tokens = CreateTokenList();
        }

        #endregion

        public void Compile(string code, string outFile)
        {
            const string className = "BrainfuckApplication";
            const string compileTempName = "compile.tmp"; // Path.GetTempFileName();

            string fileName = System.IO.Path.GetFileNameWithoutExtension(outFile);

            if (fileName == null)
                throw new ArgumentException("Invalid outFile.");

            /*
            string extension = System.IO.Path.GetExtension(outFile);
            string outdir = System.IO.Path.GetDirectoryName(outFile);
            */



            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(fileName), AssemblyBuilderAccess.RunAndSave);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(fileName, compileTempName, false);

            var typeBuilder = moduleBuilder.DefineType(className, TypeAttributes.Public);

            var methodBuilder = typeBuilder.DefineMethod("Run",
                MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard,
                typeof(void), new[] { typeof(string[]) });
            assemblyBuilder.SetEntryPoint(methodBuilder, PEFileKinds.ConsoleApplication);

            var emitter = new BrainfuckEmitter(methodBuilder.GetILGenerator());
            emitter.SetupLocals(ApplicationMemory);

            for (int i = 0; i < code.Length; ++i)
            {
                var c = code[i];
                if (!IsValidToken(c))
                    continue;

                int counter = 0;
                if (i != code.Length - 1)
                {
                    for (int j = i; j < code.Length; ++j)
                    {
                        if (code[j] != c)
                            break;
                        counter++;
                    }
                    i += counter - 1;
                }
                else
                    counter = 1;

                switch (c)
                {
                    case '+':
                        emitter.EmitCellIncrement(counter);
                        break;
                    case '-':
                        emitter.EmitCellDecrement(counter);
                        break;
                    case '<':
                        emitter.EmitPtrDecrement(counter);
                        break;
                    case '>':
                        emitter.EmitPtrIncrement(counter);
                        break;
                    case '.':
                        emitter.EmitConsoleWrite(counter);
                        break;
                    case ',':
                        emitter.EmitConsoleRead(counter);
                        break;
                    case '[':
                        emitter.EmitLoopHead(counter);
                        break;
                    case ']':
                        emitter.EmitLoopEnd(counter);
                        break;
                    default:
                        continue;
                }
            }
            emitter.Finish();
            typeBuilder.CreateType();
            assemblyBuilder.Save(compileTempName);
            if (File.Exists(outFile))
                File.Delete(outFile);
            File.Move(compileTempName, outFile);
        }

        private char[] CreateTokenList()
        {
            return new[] { '+', '-', '<', '>', '[', ']', '.', ',' };
        }
        private readonly char[] _tokens;
        private bool IsValidToken(char t)
        {
            return _tokens.Contains(t);
        }
    }
}
