using System;

namespace PZUpdater
{
    public static class Consts
    {
        public static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }
        public static bool UseDMD = false;
        public static readonly System.IO.DirectoryInfo curDir = new System.IO.DirectoryInfo(Environment.CurrentDirectory);
    }
}

