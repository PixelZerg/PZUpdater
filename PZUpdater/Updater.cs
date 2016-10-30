using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZUpdater
{
    public class Updater
    {
        public Client client = new Client();

        public void Update()
        {
            client.Fetch();
            client.Decompile();
            Console.WriteLine(string.Join(Environment.NewLine, client.LocateGameServerConnection()));
        }
    }
}
