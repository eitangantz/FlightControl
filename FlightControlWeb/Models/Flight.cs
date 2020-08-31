using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlightControlWeb.Models;

namespace FlightControl.Models
{
    public class Flight 
    {
        public string flight_id { get; set; }
        public float longitude { get; set; }//////////////////////////////////////
        public float latitude { get; set; }//////////////////////////////////////
        public int passengers { get; set; }
        public string company_name { get; set; }
        public DateTime date_time { get; set; }
        public bool is_external { get; set; }
        public Initial_Location current_location { get; set; }
        public Segment[] pathForLine { get; set; }
    }
}

