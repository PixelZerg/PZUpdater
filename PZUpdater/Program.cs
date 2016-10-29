using System;
using System.Diagnostics;

namespace PZUpdater
{
    class Program
    {
        public static void Main(string[] args)
        {
            Client c = new Client();
            c.Fetch();
            c.Decompile();
        }

        public static void RunCommand(string workingDirectory, string file, string args)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("> " + file + " " + args);
                ProcessStartInfo i = new ProcessStartInfo();
                i.CreateNoWindow = true;
                i.WindowStyle = ProcessWindowStyle.Hidden;
                i.WorkingDirectory = workingDirectory;
                i.FileName = file;
                i.Arguments = args;
                i.RedirectStandardOutput = true;
                i.RedirectStandardError = true;
                i.UseShellExecute = false;
                Process p = new Process();
                p.StartInfo = i;
                p.Start();
                p.WaitForExit();
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                string so = p.StandardOutput.ReadToEnd();
                string se = p.StandardError.ReadToEnd();
                if (!string.IsNullOrWhiteSpace(so))
                {
                    Console.WriteLine(">> " + so);
                }
                if (!string.IsNullOrWhiteSpace(se))
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(">> " + se);
                }
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error running command!");
                Console.ResetColor();
                throw;
            }
            Console.ResetColor();
        }

        public static void RunCommand(string file, string args)
        {
            RunCommand(Environment.CurrentDirectory, file, args);
        }
    }
}
