using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{
    public class ServerManager
    {
        public IEnumerable<Server> GetServers()
        {
            if (Global.serversDataBase == null) { Global.serversDataBase = new ConcurrentDictionary<string, Server>(); }
            return Global.serversDataBase.Values;
        }
        public void AddServer(Server server)
        {
            if (Global.serversDataBase == null) { Global.serversDataBase = new ConcurrentDictionary<string, Server>(); }
            Global.serversDataBase.GetOrAdd(server.serverId,server);
        }
        public void DeleteServer(string id)
        {
            if (Global.serversDataBase != null)
            {
                ConcurrentDictionary<string, Server> servers = Global.serversDataBase;
                foreach (KeyValuePair<string, Server> s in servers)
                {
                    if (s.Key == id)
                    {
                        Server ignore;
                        servers.TryRemove(s.Key, out ignore);
                    }
                }
            }
        }
    }
}
