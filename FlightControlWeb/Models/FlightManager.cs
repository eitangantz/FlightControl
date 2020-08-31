using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FlightControlWeb;
using FlightControlWeb.Models;

namespace FlightControl.Models
{
    public class FlightManager
    {
        public void AddFlight(Flight f,FlightPlan fp)
        {
            if (Global.idFlightFlightPlanDataBaseDic == null) { Global.idFlightFlightPlanDataBaseDic = new System.Collections.Concurrent.ConcurrentDictionary<string, KeyValuePair<Flight, FlightPlan>>(); }
            Global.idFlightFlightPlanDataBaseDic.GetOrAdd(f.flight_id, new KeyValuePair<Flight, FlightPlan>(f, fp));
        }



        public void DeleteFlight(string id)
        {
            if (Global.idFlightFlightPlanDataBaseDic != null)
            {
                KeyValuePair<Flight, FlightPlan> ignore;
                Global.idFlightFlightPlanDataBaseDic.TryRemove(id, out ignore);
            }
        }



#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public  async Task<List<Flight>> GetAllFlightsByTime(DateTime t)  //no sync with external servers.
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            List<Flight> flights = new List<Flight>();
            if (Global.idFlightFlightPlanDataBaseDic != null)
            {
                IEnumerable<KeyValuePair<Flight, FlightPlan>> pairsflights = Global.idFlightFlightPlanDataBaseDic.Values;

                foreach (KeyValuePair<Flight, FlightPlan> p in pairsflights)
                {
                     if (IsBetween(TimeZoneInfo.ConvertTimeToUtc(t), p.Value))
                      {
                        p.Key.pathForLine = p.Value.segments;
                        p.Key.current_location = CalculateCurrentLocation(TimeZoneInfo.ConvertTimeToUtc(t), p.Value);
                        flights.Add(p.Key);
                      }
                }
            }
            return flights;
        }



        public async Task<List<Flight>> GetAllFlightsByTimeSyncAll(DateTime t)  //need to check this function
        {
            Global.err = "";
            Global.flightServerDic = new ConcurrentDictionary<string, KeyValuePair<Flight, Server>>();
            List<Flight> flights = new List<Flight>();
            flights = AddInternals(flights, t);
            // add external servers's flights:
            List<Flight> externlFlights = null;
            if (Global.serversDataBase != null)
            {
                foreach (Server server in Global.serversDataBase.Values)
                {
                    try
                    {
                        HttpClient client = new HttpClient();
                        client.Timeout = TimeSpan.FromMilliseconds(100);
                        client.DefaultRequestHeaders.Add("User-Agent", "C# console program");
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.BaseAddress = new Uri(server.serverUrl);
                        var respone = await client.GetStringAsync(server.serverUrl + "/api/Flights?relative_to=" + TimeZoneInfo.ConvertTimeToUtc(t).ToString("yyyy-MM-ddTHH:mm:ssZ"));
                        externlFlights = JsonConvert.DeserializeObject<List<Flight>>(respone);
                        flights = await AddExternals(flights, client, externlFlights, server, t);
                    }
                    catch (Exception)
                    {
                        Global.err = server.serverId + " is not valid";
                    }
                }    
            }
           return flights;        
        }














        public async Task<FlightData> GetFlightDataById(string id) // this is not in api requirements only for flightdata representation and the calculate of the exact locations of the markers
        {
            FlightData flightdata = TryToGetFlightDataFromInternals(id);
            if (flightdata != null) { return flightdata; }
            if(Global.serversDataBase != null)
            {
                bool isError = false;
                //get FlightPlan form the
                IEnumerable<KeyValuePair<Flight, Server>> pairs = Global.flightServerDic.Values;
                Flight theFlight = pairs.Where(x => x.Key.flight_id == id).FirstOrDefault().Key;
                Server theServer = pairs.Where(x => x.Key.flight_id == id).FirstOrDefault().Value;
                FlightPlan theFlightPlan = null;
                if ((theFlight != null) && (theServer != null))
                {
                    // send get flightplan request to the server by fligth id
                    string respone = null;
                    try
                    {
                        HttpClient client = new HttpClient();
                        client.BaseAddress = new Uri(theServer.serverUrl);
                        client.Timeout = TimeSpan.FromMilliseconds(100);
                        client.DefaultRequestHeaders.Add("User-Agent", "C# console program");
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        respone = await client.GetStringAsync(theServer.serverUrl + "/api/FlightPlan/" + id);
                        theFlightPlan = JsonConvert.DeserializeObject<FlightPlan>(respone);
                    }
                    catch
                    {
                        isError = true;
                        Global.err = "can't reach to " + id + " flight plan";
                    }
                    if (isError){return new FlightData();}
                    // calculate finish time and finish location
                    if (theFlightPlan != null)
                    {
                        string finishTime = MakeFinishDate(theFlightPlan).ToString();
                        Initial_Location finishLocation = new Initial_Location();
                        finishLocation.longitude = theFlightPlan.segments[theFlightPlan.segments.Length - 1].longitude;
                        finishLocation.latitude = theFlightPlan.segments[theFlightPlan.segments.Length - 1].latitude;
                        FlightData fd = new FlightData(theFlight, finishTime, finishLocation);
                        return fd;
                    }
                }
            }
            return new FlightData();
        }
























        //help functions:















        FlightData TryToGetFlightDataFromInternals(string id)
        {
            FlightData fd = new FlightData();
            if (Global.idFlightFlightPlanDataBaseDic != null)
            {
                IEnumerable<KeyValuePair<Flight, FlightPlan>> pairsFlights = Global.idFlightFlightPlanDataBaseDic.Values;
                FlightPlan fp = pairsFlights.Where(x => x.Key.flight_id == id).FirstOrDefault().Value;
                Flight f = pairsFlights.Where(x => x.Key.flight_id == id).FirstOrDefault().Key;
                if ((fp != null) && (f != null))
                {
                    // calculate finish time and finish location
                    string finishTime = MakeFinishDate(fp).ToString();
                    Initial_Location finishLocation = new Initial_Location();
                    if (fp.segments.Length < 1) { return new FlightData(); }
                    finishLocation.longitude = fp.segments[fp.segments.Length - 1].longitude;
                    finishLocation.latitude = fp.segments[fp.segments.Length - 1].latitude;
                    fd = new FlightData(f, finishTime, finishLocation);
                    return fd;
                }
            }
            return null;
        }


        List<Flight> AddInternals(List<Flight> flights, DateTime t)
        {
            if (Global.idFlightFlightPlanDataBaseDic != null)
            {
                IEnumerable<KeyValuePair<Flight, FlightPlan>> pairsFlights = Global.idFlightFlightPlanDataBaseDic.Values;
                foreach (KeyValuePair<Flight, FlightPlan> p in pairsFlights)
                {
                    if (IsBetween(TimeZoneInfo.ConvertTimeToUtc(t), p.Value))
                    {
                        p.Key.pathForLine = p.Value.segments;
                        p.Key.current_location = CalculateCurrentLocation(TimeZoneInfo.ConvertTimeToUtc(t), p.Value);
                        flights.Add(p.Key);
                    }
                }
            }
            return flights;
        }


        async Task<List<Flight>> AddExternals(List<Flight> flights,HttpClient client ,List<Flight> externlFlights,Server server,DateTime t)
        {
            if (externlFlights != null)
            {
                foreach (Flight externlFlight in externlFlights)
                {
                        externlFlight.is_external = true;
                        var respone1 = await client.GetStringAsync(server.serverUrl + "/api/FlightPlan/" + externlFlight.flight_id);
                        FlightPlan theFlightPlan = JsonConvert.DeserializeObject<FlightPlan>(respone1);
                        externlFlight.pathForLine = theFlightPlan.segments;
                        externlFlight.current_location = CalculateCurrentLocation(TimeZoneInfo.ConvertTimeToUtc(t), theFlightPlan);
                        if (!IsExternalInInternals(flights, externlFlight))
                        {
                            flights.Add(externlFlight);
                            Global.flightServerDic.GetOrAdd(externlFlight.flight_id, new KeyValuePair<Flight, Server>(externlFlight, server));
                        }
                }
            }
            return flights;
        }





        bool IsExternalInInternals(List<Flight> flights, Flight external)
        {
            foreach (Flight internalFlight in flights)
            {
                if (internalFlight.flight_id == external.flight_id)
                {
                    return true;
                }
            }
            return false;
        }



        public bool IsBetween(DateTime relative, FlightPlan fp)
        {
            DateTime startDate = fp.initial_location.date_time;
            TimeZoneInfo.ConvertTimeToUtc(startDate);
            DateTime finishDate = MakeFinishDate(fp);
            TimeZoneInfo.ConvertTimeToUtc(finishDate);
            return relative >= startDate && relative <= finishDate;
        }

        public DateTime MakeFinishDate(FlightPlan fp)
        {
            DateTime finishDate = fp.initial_location.date_time;
            for(int i = 0;i<fp.segments.Length;i++)
            {
                finishDate = finishDate.AddSeconds(fp.segments[i].timespan_seconds);
            }
            return finishDate;
        }
        
        public Initial_Location CalculateCurrentLocation(DateTime relative_to, FlightPlan fp)
        {
            int i = 0;
            TimeSpan diff = new TimeSpan();
            DateTime iniCopy = fp.initial_location.date_time;  // starting TIME
            while(iniCopy < relative_to)
            {
                diff = relative_to - iniCopy;  
                iniCopy = iniCopy.AddSeconds(fp.segments[i].timespan_seconds);
                i++;
            }
            i--;
            Segment a = new Segment();
            if (i == 0) {
                a.latitude = fp.initial_location.latitude;
                a.longitude = fp.initial_location.longitude;
             } else {
                a = fp.segments[i - 1];
            }
            Segment b = fp.segments[i];
            double fraction = diff.TotalSeconds / b.timespan_seconds;
            Initial_Location currentLocation = Interpolate(fraction, a, b);
            return currentLocation;
        }

            
        public Initial_Location Interpolate(double frac,Segment a, Segment b)
        {
            Initial_Location curr = new Initial_Location();
              float diffXcordinate =  b.latitude - a.latitude;//////////////////////////////////////
              float diffYcordinate = b.longitude - a.longitude;//////////////////////////////////////
              float Lat = a.latitude + diffXcordinate * (float)frac;//////////////////////////////////////
              float lng = a.longitude + diffYcordinate * (float)frac;//////////////////////////////////////
              curr.latitude = Lat;//////////////////////////////////////
              curr.longitude = lng;//////////////////////////////////////
              return curr;
        }




    }
}
