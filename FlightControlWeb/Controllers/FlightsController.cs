using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlightControl.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FlightControlWeb;

namespace FlightControl.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightsController : ControllerBase
    {
        private FlightManager flightsManager = new FlightManager();
        // GET: api/Flights
        [HttpGet]
        public Task<List<Flight>> GetAllFlights(DateTime relative_to)
        {
            if (Request.QueryString.Value.Contains("sync_all"))
            {
                return flightsManager.GetAllFlightsByTimeSyncAll(relative_to);
            }
            return flightsManager.GetAllFlightsByTime(relative_to);
        }

        [HttpGet("{id}", Name = "Get")]// this is not in api requirements only for flightdata representation
        public Task<FlightData> Get(string id)
        {
            return flightsManager.GetFlightDataById(id);
        }
        // DELETE: api/Flights/5
        [HttpDelete("{id}")]
        public void Delete(string id)
        {
            flightsManager.DeleteFlight(id);
        }
    }
}
