using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Xml.Linq;
using System.Linq;

namespace PZUpdater
{
    public class Updater
    {
        public Client client = new Client();
        public Dictionary<KeyValuePair<int,string>, int> packets = new Dictionary<KeyValuePair<int,string>, int>();

        public void Update()
        {
            try
            {
                client.Fetch();
            }
            catch(Exception e)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error fetching client!");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(e);
                Console.ResetColor();
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
                        packets.Add(new KeyValuePair<int,string>(Int32.Parse(Parsing.GetBetween(raw[i], "slotid", "type").Trim()),Parsing.GetBetween(raw[i], "), \"", "\") slotid").Trim()), Int32.Parse(Parsing.GetBetween(raw[i], "Integer(", ")").Trim()));
                    }
                }
            }
            Logger.UnIndent();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Logger.WriteLine("Parsed " + packets.Count+" packet IDs");
            
            Console.ResetColor();
        }

        public void WriteOutput()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Logger.WriteLine("Writing output...");
            Console.ResetColor();
            Logger.Indent();

            Logger.Write("Writing packets.xml...");
            using (StreamWriter sw = new StreamWriter(Consts.OUTPUT_DIR + "packets.xml"))
            {
                sw.WriteLine(GetPacketsXML());
            }
            Logger.WriteLine("[OK!]");

            Logger.Write("Writing tiles.xml...");
            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                using (StreamWriter sw = new StreamWriter(Consts.OUTPUT_DIR + "tiles.xml"))
                {
                    sw.WriteLine("<!--Acquired using PixelZerg's PZUpdater-->");
                    sw.WriteLine(XDocument.Parse(wc.DownloadString("https://static.drips.pw/rotmg/production/current/xmlc/GroundTypes.xml")).ToString());
                }
            }
            Logger.WriteLine("[OK!]");

            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

                StringBuilder sbEnemies = new StringBuilder("<Objects>" + Environment.NewLine);
                StringBuilder sbItems = new StringBuilder("<Objects>" + Environment.NewLine);
                StringBuilder sbObjects = new StringBuilder("<Objects>" + Environment.NewLine);

                XDocument doc = XDocument.Parse(wc.DownloadString("https://static.drips.pw/rotmg/production/current/xmlc/Objects.xml"));
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

        private string GetPacketsXML()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<!--Acquired using PixelZerg's PZUpdater-->");
            sb.AppendLine("<Packets>");
            foreach (var pair in packets)
            {
                string name = GetPacketIdName(pair.Key.Key);
                sb.AppendLine("\t<Packet>");
                sb.AppendLine("\t\t<PacketName>" + ((name != null) ? name : pair.Key.Value) + "</PacketName>");
                sb.AppendLine("\t\t<PacketID>" + pair.Value + "</PacketID>");
                sb.AppendLine("\t</Packet>");
            }
            sb.AppendLine("</Packets>");
            return sb.ToString();
        }

        private string GetPacketIdName(int slotid)
        {
            switch (slotid)
            {
                case 1: return "FAILURE";
                case 2: return "CREATE_SUCCESS";
                case 3: return "CREATE";
                case 4: return "PLAYERSHOOT";
                case 5: return "MOVE";
                case 6: return "PLAYERTEXT";
                case 7: return "TEXT";
                case 8: return "SHOOT2";
                case 9: return "DAMAGE";
                case 10: return "UPDATE";
                case 11: return "UPDATEACK";
                case 12: return "NOTIFICATION";
                case 13: return "NEW_TICK";
                case 14: return "INVSWAP";
                case 15: return "USEITEM";
                case 16: return "SHOW_EFFECT";
                case 17: return "HELLO";
                case 18: return "GOTO";
                case 19: return "INVDROP";
                case 20: return "INVRESULT";
                case 21: return "RECONNECT";
                case 22: return "PING";
                case 23: return "PONG";
                case 24: return "MAPINFO";
                case 25: return "LOAD";
                case 26: return "PIC";
                case 27: return "SETCONDITION";
                case 28: return "TELEPORT";
                case 29: return "USEPORTAL";
                case 30: return "DEATH";
                case 31: return "BUY";
                case 32: return "BUYRESULT";
                case 33: return "AOE";
                case 34: return "GROUNDDAMAGE";
                case 35: return "PLAYERHIT";
                case 36: return "ENEMYHIT";
                case 37: return "AOEACK";
                case 38: return "SHOOTACK";
                case 39: return "OTHERHIT";
                case 40: return "SQUAREHIT";
                case 41: return "GOTOACK";
                case 42: return "EDITACCOUNTLIST";
                case 43: return "ACCOUNTLIST";
                case 44: return "QUESTOBJID";
                case 45: return "CHOOSENAME";
                case 46: return "NAMERESULT";
                case 47: return "CREATEGUILD";
                case 48: return "CREATEGUILDRESULT";
                case 49: return "GUILDREMOVE";
                case 50: return "GUILDINVITE";
                case 51: return "ALLYSHOOT";
                case 52: return "SHOOT";
                case 53: return "REQUESTTRADE";
                case 54: return "TRADEREQUESTED";
                case 55: return "TRADESTART";
                case 56: return "CHANGETRADE";
                case 57: return "TRADECHANGED";
                case 58: return "ACCEPTTRADE";
                case 59: return "CANCELTRADE";
                case 60: return "TRADEDONE";
                case 61: return "TRADEACCEPTED";
                case 62: return "CLIENTSTAT";
                case 63: return "CHECKCREDITS";
                case 64: return "ESCAPE";
                case 65: return "FILE";
                case 66: return "INVITEDTOGUILD";
                case 67: return "JOINGUILD";
                case 68: return "CHANGEGUILDRANK";
                case 69: return "PLAYSOUND";
                case 70: return "GLOBAL_NOTIFICATION";
                case 71: return "RESKIN";
                case 72: return "PETYARDCOMMAND";
                case 73: return "PETCOMMAND";
                case 74: return "UPDATEPET";
                case 75: return "NEWABILITYUNLOCKED";
                case 76: return "UPGRADEPETYARDRESULT";
                case 77: return "EVOLVEPET";
                case 78: return "REMOVEPET";
                case 79: return "HATCHEGG";
                case 80: return "ENTER_ARENA";
                case 81: return "ARENANEXTWAVE";
                case 82: return "ARENADEATH";
                case 83: return "LEAVEARENA";
                case 84: return "VERIFYEMAILDIALOG";
                case 85: return "RESKIN2";
                case 86: return "PASSWORDPROMPT";
                case 87: return "VIEWQUESTS";
                case 88: return "TINKERQUEST";
                case 89: return "QUESTFETCHRESPONSE";
                case 90: return "QUESTREDEEMRESPONSE";
                case 91: return "PET_CHANGE_FORM_MSG";
                case 92: return "KEY_INFO_REQUEST";
                case 93: return "KEY_INFO_RESPONSE";
            }
            return null;
        }
    }
}
