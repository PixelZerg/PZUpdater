using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZUpdater
{
    public static class Logger
    {
        private static int indent = 0;
        private static bool wasNewLine = true;

        private static IEnumerable<string> Chunk(this string str, int chunkSize)
        {
            for (int i = 0; i < str.Length; i += chunkSize)
                yield return str.Substring(i, Math.Min(chunkSize, str.Length - i));
        }

        public static void WriteLine(string ss)
        {
            if (wasNewLine)
            {
                string[] split = ss.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

                foreach (string s in split)
                {
                    foreach (string chunk in s.Chunk(Console.WindowWidth - 1 - 8))
                    {
                        for (int i = 0; i < indent; i++)
                        {
                            Console.Write("        ");
                        }
                        Console.WriteLine(chunk);
                    }
                }
            }
            else
            {
                Console.WriteLine(ss);
            }
            wasNewLine = true;
        }

        public static void Write(string s)
        {
            if (wasNewLine)
            {
                for (int i = 0; i < indent; i++)
                {
                    Console.Write("        ");
                }
            }
            Console.Write(s);
            wasNewLine = false;
        }

        public static void Indent()
        {
            indent++;
        }

        public static void UnIndent()
        {
            if (indent > 0)
            {
                indent--;
            }
        }
    }
}
