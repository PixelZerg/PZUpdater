using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Xml.Linq;
using System.Linq;
using System.Xml;

namespace PZUpdater
{
    public class Updater
    {
        public Client client = new Client();
        public Dictionary<KeyValuePair<int, string>, int> packets = new Dictionary<KeyValuePair<int, string>, int>();

        public void Update()
        {
            try
            {
                client.Fetch();
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error fetching client!");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(e);
                Console.ResetColor();
                client.Cleanup();
                return;
            }
            try
            {
                client.Decompile();
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error decompiling client!");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(e);
                Console.ResetColor();
                client.Cleanup();
                return;
            }
            try
            {
                this.ParsePacketIDs();
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error parsing packet IDs!");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(e);
                Console.ResetColor();
                client.Cleanup();
                return;
            }
            try
            {
                this.WriteOutput();
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error writing output!");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(e);
                Console.ResetColor();
                client.Cleanup();
                return;
            }
            client.Cleanup();
        }

        public void ParsePacketIDs()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Logger.WriteLine("Parsing packet IDs...");
            Console.ResetColor();
            Logger.Indent();
            string[] raw = client.GetGameServerConnection();
            int top = -1;
            for (int i = 0; i < raw.Length; i++)
            {
                if (raw[i].Contains(Consts.PACKETIDS_TOP))
                {
                    top = i;
                }
                if (top != -1)
                {
                    if (raw[i].Contains("slot") && raw[i].Contains("Integer"))
                    {
                        packets.Add(new KeyValuePair<int, string>(Int32.Parse(Parsing.GetBetween(raw[i], "slotid", "type").Trim()), Parsing.GetBetween(raw[i], "), \"", "\") slotid").Trim()), Int32.Parse(Parsing.GetBetween(raw[i], "Integer(", ")").Trim()));
                    }
                }
            }
            Logger.UnIndent();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Logger.WriteLine("Parsed " + packets.Count + " packet IDs");

            Console.ResetColor();
        }

        public void WriteOutput()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Logger.WriteLine("Writing output...");
            Console.ResetColor();
            Logger.Indent();

            Logger.Write("Writing packets.xml...");

            WritePacketXmls();

            Logger.WriteLine("[OK!]");

            Logger.Write("Writing tiles.xml...");
            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                using (StreamWriter sw = new StreamWriter(Consts.OUTPUT_DIR + "tiles.xml"))
                {
                    sw.WriteLine("<!--Acquired using PixelZerg's PZUpdater-->");
                    sw.WriteLine(XDocument.Parse(wc.DownloadString("http://static.drips.pw/rotmg/production/current/xmlc/GroundTypes.xml")).ToString());
                }
            }
            Logger.WriteLine("[OK!]");

            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

                StringBuilder sbEnemies = new StringBuilder("<Objects>" + Environment.NewLine);
                StringBuilder sbItems = new StringBuilder("<Objects>" + Environment.NewLine);
                StringBuilder sbObjects = new StringBuilder("<Objects>" + Environment.NewLine);

                XDocument doc = XDocument.Parse(wc.DownloadString("http://static.drips.pw/rotmg/production/current/xmlc/Objects.xml"));
                foreach (XElement elem in doc.Element("Objects").Elements("Object"))
                {
                    if (elem.Element("Enemy") != null)
                    {
                        sbEnemies.AppendLine(elem.ToString());
                    }
                    else if (elem.Element("Item") != null)
                    {
                        sbItems.AppendLine(elem.ToString());
                    }
                    else
                    {
                        sbObjects.AppendLine(elem.ToString());
                    }
                }

                sbEnemies.AppendLine("</Objects>");
                sbItems.AppendLine("</Objects>");
                sbObjects.AppendLine("</Objects>");

                Logger.Write("Writing complete gamedata.xml...");
                using (StreamWriter sw = new StreamWriter(Consts.OUTPUT_DIR + "complete gamedata.xml"))
                {
                    sw.WriteLine("<!--Acquired using PixelZerg's PZUpdater-->");
                    sw.WriteLine(doc.ToString());
                }
                Logger.WriteLine("[OK!]");

                Logger.Write("Writing enemies.xml...");
                using (StreamWriter sw = new StreamWriter(Consts.OUTPUT_DIR + "enemies.xml"))
                {
                    sw.WriteLine("<!--Acquired using PixelZerg's PZUpdater-->");
                    sw.WriteLine(sbEnemies.ToString());
                }
                Logger.WriteLine("[OK!]");

                Logger.Write("Writing items.xml...");
                using (StreamWriter sw = new StreamWriter(Consts.OUTPUT_DIR + "items.xml"))
                {
                    sw.WriteLine("<!--Acquired using PixelZerg's PZUpdater-->");
                    sw.WriteLine(sbItems.ToString());
                }
                Logger.WriteLine("[OK!]");

                Logger.Write("Writing objects.xml...");
                using (StreamWriter sw = new StreamWriter(Consts.OUTPUT_DIR + "objects.xml"))
                {
                    sw.WriteLine("<!--Acquired using PixelZerg's PZUpdater-->");
                    sw.WriteLine(sbObjects.ToString());
                }
                Logger.WriteLine("[OK!]");
            }

            Logger.UnIndent();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Logger.WriteLine("Finished writing output!");

            Console.ResetColor();
        }

        private void WritePacketXmls()
        {
            StringBuilder sbK = new StringBuilder();
            StringBuilder sbR = new StringBuilder();
            sbR.AppendLine("<!--Acquired using PixelZerg's PZUpdater-->");
            sbK.AppendLine("<!--Acquired using PixelZerg's PZUpdater-->");

            sbK.AppendLine("<Packets>");
            sbR.AppendLine("<?xml version=\"1.0\" encoding=\"UTF - 8\"?>");
            sbR.AppendLine("<Packets>");
            foreach (var pair in packets)
            {
                string UnparsedName = pair.Key.Value;

                XmlDocument doc = new XmlDocument();
                doc.Load("PacketNames.xml");

                XmlNodeList nodes = doc.DocumentElement.SelectNodes("/packets/packet");
                foreach (XmlNode node in nodes)
                {
                    if (UnparsedName == node.ChildNodes[0].InnerText)
                        UnparsedName = node.ChildNodes[1].InnerText;
                }

                sbK.AppendLine("  <Packet>");
                sbK.AppendLine("    <PacketName>" + UnparsedName + "</PacketName>");
                sbK.AppendLine("    <PacketID>" + pair.Value + "</PacketID>");
                sbK.AppendLine("  </Packet>");

                sbR.AppendLine("\t<Packet id=\"" + UnparsedName + "\" type=\"" + pair.Value + "\"/>");
            }
            sbK.AppendLine("</Packets>");
            sbR.AppendLine("</Packets>");

            using (StreamWriter sw = new StreamWriter(Consts.OUTPUT_DIR + "packets.xml"))
            {
                sw.WriteLine(sbK.ToString());
            }
            using (StreamWriter sw = new StreamWriter(Consts.OUTPUT_DIR + "packets(RealmRelay).xml"))
            {
                sw.WriteLine(sbR.ToString());
            }
        }
    }
}
