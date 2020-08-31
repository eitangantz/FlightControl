using FlightControl.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlightControlWeb.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FlightControlWeb
{
    public static class Global
    {
        static ConcurrentDictionary<string, KeyValuePair<Flight, FlightPlan>> dic = new ConcurrentDictionary<string, KeyValuePair<Flight, FlightPlan>>();

        static ConcurrentDictionary<string,Server> servers = new ConcurrentDictionary<string, Server>();

        static ConcurrentDictionary<string, KeyValuePair<Flight, Server>> flightServerDictionary = new ConcurrentDictionary<string, KeyValuePair<Flight, Server>>();
        static string error = "";

        public static ConcurrentDictionary<string, KeyValuePair<Flight, FlightPlan>> idFlightFlightPlanDataBaseDic 
        { 
            get
            {
                return dic;
            }
            set
            {
                dic = value;
            }
        }
        public static ConcurrentDictionary<string, Server> serversDataBase
        {
            get
            {
                return servers;
            }
            set
            {
                servers = value;
            }
        }
        public static ConcurrentDictionary<string, KeyValuePair<Flight, Server>> flightServerDic
        {
            get
            {
                return flightServerDictionary;
            }
            set
            {
                flightServerDictionary = value;
            }
        }
        public static string err
        {
            get
            {
                return error;
            }
            set
            {
                error = value;
            }
        }

    }
}
