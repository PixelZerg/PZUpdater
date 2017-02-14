using System;
using System.Diagnostics;

namespace PZUpdater
{
    class Program
    {
        public static void Main(string[] args)
        {
            bool overrideDMD = false;
            foreach (string arg in args)
            {
                if (arg == "--help" || arg == "-h" || arg == "?")
                {
                    Console.WriteLine("Arguments:");
                    Console.WriteLine("\t-q, --quet     = Quiet mode, output will be supressed.");
                    Console.WriteLine("\t--nodmd        = Regardless of your operating system, using dmd will be disabled.");
                    Console.WriteLine("\t--dmd          = Regardless of your operating system, using dmd will be enabled.");
                    Console.WriteLine("\t--help, ?, -h  = Show this message.");
                    return;
                }
                if (arg == "-q" || arg == "--quiet")
                {
                    Consts.Quiet = true;
                }
                if (arg == "--nodmd")
                {
                    Consts.UseDMD = false;
                    overrideDMD = true;
                }
                if (arg == "--dmd")
                {
                    Consts.UseDMD = true;
                    overrideDMD = true;
                }
            }

            if (!Consts.rabcdasmDir.Exists)
            {
                //woah this is pretty bad. Will be dealt with later
                //Consts.rabcdasmDir.Create();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("FATAL: Your RABCDAsm directory is missing");
                Console.ResetColor();
            }
            if (!System.IO.Directory.Exists(Consts.OUTPUT_DIR_WO))
            {
                System.IO.Directory.CreateDirectory(Consts.OUTPUT_DIR_WO);
            }

            if (!overrideDMD)
            {
                if (Consts.IsLinux)
                {
                    Consts.UseDMD = true;
                    Console.ForegroundColor = ConsoleColor.White;
                    Logger.WriteLine("You are on linux. This program requires dmd to be installed.");
                    Console.ResetColor();
                }
                else
                {
                    Consts.UseDMD = false;

                    Console.ForegroundColor = ConsoleColor.White;
                    Logger.WriteLine("You are not on linux.");
                    Console.ResetColor();
                }
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
