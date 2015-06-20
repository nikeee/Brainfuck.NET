using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brainfuck.NET.Tests
{
    class Program
    {
        public static int Main(string[] args)
        {
            const string assembly = @"..\..\..\Brainfuck.NET\bin\Release\bfnet.exe";

            if (!File.Exists(assembly))
                return 255;


            var p = Process.Start(assembly, "\"..\\..\\helloworld.bf\" hw.exe");
            p.WaitForExit();
            if (p.ExitCode != 0)
            {
                Console.WriteLine("Exit code != 0");
                return p.ExitCode;
            }

            var psi = new ProcessStartInfo("hw.exe") { RedirectStandardOutput = true, UseShellExecute = false };
            p = Process.Start(psi);

            if (p == null)
                return 254;

            var hwOutput = p.StandardOutput.ReadToEnd();

            Console.WriteLine("HW output:");
            Console.WriteLine(hwOutput);
            Console.WriteLine("----------");

            const string expected = "Hello World!\n\r";

            if (hwOutput != expected)
            {
                Console.WriteLine("Output doesn't match expectations.");
                return 2;
            }
            Console.WriteLine("Test done.");
            return 0;
        }
    }
}
