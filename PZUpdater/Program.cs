using System;
using System.Diagnostics;

namespace PZUpdater
{
    class Program
    {
        public static void Main(string[] args)
        {
            if (!Consts.rabcdasmDir.Exists)
            {
                //woah this is pretty bad. Will be dealt with later
                //Consts.rabcdasmDir.Create();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("FATAL: Your RABCDAsm directory is missing");
                Console.ResetColor();
            }
            if (!System.IO.Directory.Exists(Consts.OUTPUT_DIR))
            {
                System.IO.Directory.CreateDirectory(Consts.OUTPUT_DIR);
            }

            if (Consts.IsLinux)
            {
                Consts.UseDMD = true;
                Console.ForegroundColor = ConsoleColor.White;
                Logger.WriteLine("You are on linux. This program requires dmd to be installed.");
                Console.ResetColor();
                //try
                //{
                //    Process p = new Process();
                //    p.StartInfo.FileName = "rdmd";
                //    p.StartInfo.Arguments = "--help";
                //    p.StartInfo.RedirectStandardOutput = true;
                //    p.StartInfo.RedirectStandardError = true;
                //    p.Start();
                //    p.WaitForExit();

                //    Console.ForegroundColor = ConsoleColor.Green;
                //    Logger.WriteLine("dmd is installed");
                //    Console.ResetColor();
                //}
                //catch
                //{
                //    Console.ForegroundColor = ConsoleColor.Red;
                //    Logger.WriteLine("FATAL: dmd is not installed!");
                //    Console.ResetColor();
                //}
            }
            else
            {
                Consts.UseDMD = false;

                Console.ForegroundColor = ConsoleColor.White;
                Logger.WriteLine("You are not on linux.");
                Console.ResetColor();
            }
            new Updater().Update();

            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }

        public static void RunCommand(string workingDirectory, string file, string args)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Logger.WriteLine("> " + file + " " + args);
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
                    Logger.WriteLine(">> " + so);
                }
                if (!string.IsNullOrWhiteSpace(se))
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Logger.WriteLine(">> " + se);
                }
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Logger.WriteLine("Error running command!");
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
