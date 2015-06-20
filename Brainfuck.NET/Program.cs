using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Brainfuck.NET
{
    public static class Program
    {
        public static int Main(string[] argv)
        {
            if (argv == null || argv.Length != 2)
            {
                Console.Error.WriteLine("Invalid parameters.");
                var ownName = Assembly.GetEntryAssembly().GetName().Name;

                Console.Error.WriteLine("Usage:");
                Console.Error.WriteLine("{0} [in.bf] [out.exe]", ownName);
                return 1;
            }

            string bfFile = argv[0];
            string outFile = argv[1];
            try
            {
                var compiler = new BrainfuckCompiler();
                using (var inFile = File.OpenRead(bfFile))
                using (var reader = new StreamReader(inFile))
                    compiler.Compile(reader.ReadToEnd(), outFile);
            }
            catch (Exception e)
            {
                throw;
            }
            return 0;
        }
    }
}
