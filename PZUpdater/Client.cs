using System;
using System.IO;
using System.Net;
using System.Collections.Generic;

namespace PZUpdater
{
    public class Client : IDisposable
    {

        public FileInfo clientFile = new FileInfo(System.IO.Path.GetFullPath(System.IO.Path.Combine(Consts.rabcdasmDir.FullName,"client.swf")));

        public List<FileInfo> abcFiles = new List<FileInfo>();
        public List<DirectoryInfo> expDirs = new List<DirectoryInfo>();

        public Client()
        {
        }

        public bool CheckRABCDAsm()
        {
            return true;
        }

        public void Fetch()
        {
            Logger.Write("Fetching client...");
            try
            {
                if (clientFile.Exists)
                {
                    clientFile.Delete();
                }

                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                    wc.DownloadFile("http://realmeye.com/appspot", clientFile.FullName);
                }
                Logger.WriteLine("[OK!]");
            }
            catch
            {
                Logger.WriteLine("[FAIL!]");
                Logger.UnIndent();
                throw;
            }
        }

        public void Decompile()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Logger.WriteLine("Decompiling client...");
            Console.ResetColor();
            Logger.Indent();

            if (Consts.UseDMD)
            {
                Program.RunCommand("rdmd", Consts.RABCDASM_DIR_ALT +"abcexport.d \"" + clientFile.FullName + "\"");
            }
            else
            {
                Program.RunCommand(Consts.RABCDASM_DIR + "abcexport.exe", "\"" + clientFile.FullName + "\"");
            }

            foreach (FileInfo f in Consts.rabcdasmDir.GetFiles("*.abc", SearchOption.TopDirectoryOnly))
            {
                abcFiles.Add(f);
                if (Consts.UseDMD)
                {
                    Program.RunCommand("rdmd", Consts.RABCDASM_DIR_ALT + "rabcdasm.d \"" + f.FullName + "\"");
                }
                else
                {
                    Program.RunCommand(Consts.RABCDASM_DIR + "rabcdasm.exe", "\"" + f.FullName + "\"");
                }

                DirectoryInfo d = new DirectoryInfo(Consts.RABCDASM_DIR + Path.GetFileNameWithoutExtension(f.FullName));
                if (d.Exists)
                {
                    expDirs.Add(d);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Logger.WriteLine("Could not locate the generated directory for \""+f.Name+"\"!");
                    Console.ResetColor();
                }
            }

            Logger.UnIndent();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Logger.WriteLine("Finished decompiling!");
            
            Console.ResetColor();
        }

        public string[] GetGameServerConnection()
        {
            foreach (DirectoryInfo d in expDirs)
            {
                string[] located = LocateGameServerConnection(d);
                if (located != null)
                {
                    Logger.WriteLine("Successfully located GameServerConnection file!");
                    return located;
                }
            }
            throw new Exception("Could not locate GameServerConnection");
        }

        private string[] LocateGameServerConnection(DirectoryInfo d)
        {
            Logger.Write("Locating GameServerConnection in \""+d.Name+"\"...");
            string[] raw = null;

            FileInfo file = new FileInfo(Path.Combine(d.FullName, "kabam/rotmg/messaging/impl/GameServerConnection.class.asasm"));
            if (file.Exists)
            {
                raw = File.ReadAllLines(file.FullName);
            }
            else
            {
                foreach (FileInfo f in d.GetFiles("*.class.asasm", SearchOption.AllDirectories))
                {
                    string praw = File.ReadAllText(f.FullName);
                    if (praw.Contains(Consts.PACKETIDS_TOP))
                    {
                        file = f;
                        raw = praw.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                        break;
                    }
                }
                if (raw == null)
                {
                    Logger.WriteLine("[FAIL!]");
                    return null;
                }
            }
            Logger.WriteLine("[OK!]");
            return raw;
        }

        public void Cleanup(bool altMessage = true)
        {
            if (!altMessage)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Logger.WriteLine("Cleaning up...");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("Cleaning up...");
                Console.ResetColor();
            }
            try
            {
                this.clientFile.Delete();
            }
            catch (IOException) { }
            try
            {
                foreach (FileInfo f in this.abcFiles)
                {
                    f.Delete();
                }
            }
            catch (IOException) { }
            try
            {
                foreach (DirectoryInfo d in this.expDirs)
                {
                    d.Delete(true);
                }
            }
            catch (IOException) { }
            if (!altMessage)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Logger.WriteLine("Finished cleaning up!");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("[OK!]");
                Console.ResetColor();
            }
        }

        public void Dispose()
        {
            this.Cleanup();
        }
    }
}