using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlightControl.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FlightControlWeb.Models;

namespace FlightControlWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightPlanController : ControllerBase
    {
        private FlightManager flightsManager = new FlightManager();

        // GET: api/FlightPlan/5
        [HttpGet("{id}", Name = "Get1")]
        public FlightPlan Get(string id)
        {
            IEnumerable<KeyValuePair<Flight, FlightPlan>> pairsflights = Global.idFlightFlightPlanDataBaseDic.Values;
            FlightPlan fp = pairsflights.Where(x => x.Key.flight_id == id).FirstOrDefault().Value;
            return fp;
        }

        // POST: api/FlightPlan
        [HttpPost]
        public void Post(FlightPlan flightPlan)
        {
            string idForFlight = GenerateIdForFlight(flightPlan.company_name, flightPlan.initial_location.date_time.Second.ToString(), flightPlan.passengers);
            Flight NewFlight = new Flight {
                flight_id = idForFlight,
                latitude = flightPlan.initial_location.latitude,
                longitude = flightPlan.initial_location.longitude,
                is_external = false,
                company_name = flightPlan.company_name,
                passengers = flightPlan.passengers,
                date_time = flightPlan.initial_location.date_time
            };
            flightsManager.AddFlight(NewFlight, flightPlan);
        }
        //help function:
        public string GenerateIdForFlight(string company, string seconds, int passengers)
        {
            string id = "";
            id += company[0].ToString() + company[1].ToString() + seconds + passengers.ToString()[0].ToString() + passengers.ToString()[passengers.ToString().Length-1].ToString();
            return id;
        }
    }
}
