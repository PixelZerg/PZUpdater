using System;
using System.IO;
using System.Net;
using System.Collections.Generic;

namespace PZUpdater
{
    public class Client
    {

        public FileInfo clientFile = new FileInfo("client.swf");

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
            Console.Write("Fetching client...");
            try
            {
                if (clientFile.Exists)
                {
                    clientFile.Delete();
                }

                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                    wc.DownloadFile("http://realmeye.com/appspot", "client.swf");
                }
                Console.WriteLine("[OK!]");
            }
            catch
            {
                Console.WriteLine("[FAIL!]");
                throw;
            }
        }

        public void Decompile()
        {
            if (Consts.UseDMD)
            {
                Program.RunCommand("rdmd", "abcexport.d \"" + clientFile.FullName + "\"");
            }
            else
            {
                Program.RunCommand("abcexport.exe", "\"" + clientFile.FullName + "\""); //untested
            }

            foreach (FileInfo f in Consts.curDir.GetFiles("*.abc", SearchOption.TopDirectoryOnly))
            {
                abcFiles.Add(f);
                if (Consts.UseDMD)
                {
                    Program.RunCommand("rdmd", "rabcdasm.d \"" + f.FullName + "\"");
                }
                else
                {
                    Program.RunCommand("rabcdasm.exe", "\"" + f.FullName + "\""); //untested
                }

                DirectoryInfo d = new DirectoryInfo(Path.GetFileNameWithoutExtension(f.Name));
                if (d.Exists)
                {
                    expDirs.Add(d);
                    ParsePacketIDs(d);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Could not locate the generated directory!");
                    Console.ResetColor();
                }
            }
        }

        public void ParsePacketIDs(DirectoryInfo d)
        {
            Console.WriteLine("Parsing packet IDs for \"" + d.Name + "\"...");

            Console.Write("\tLocating GameServerConnection...");
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
                    if (praw.Contains("\"FAILURE\") slotid 1"))
                    {
                        file = f;
                        raw = praw.Split(new string[]{ Environment.NewLine }, StringSplitOptions.None);
                        break;
                    }
                }
                if (raw == null)
                {
                    Console.WriteLine("[FAIL!]");
                    return;
                }
            }
            Console.WriteLine("[OK!]");

            Console.WriteLine(string.Join(Environment.NewLine, raw));
        }


    }
}