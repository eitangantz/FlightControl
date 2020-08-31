using FlightControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlightControlWeb.Models;

namespace FlightControlWeb
{
    public class FlightData
    {
        public FlightData()
        {
        }

        public FlightData(Flight f,string finishtime, Initial_Location finishlocation)
        {
            this.flight_id = f.flight_id;
            this.longitude = f.longitude;
            this.latitude = f.latitude;
            this.passengers = f.passengers;
            this.company_name = f.company_name;
            this.date_time = f.date_time.ToString();
            this.is_external = f.is_external;
            this.finish_time = finishtime;
            this.finish_location = finishlocation;
        }

        public string flight_id { get; set; }
        public float longitude { get; set; }//////////////////////////////////////
        public float latitude { get; set; }//////////////////////////////////////
        public int passengers { get; set; }
        public string company_name { get; set; }
        public string date_time { get; set; }
        public bool is_external { get; set; }

        public string finish_time { get; set; }

        public Initial_Location finish_location { get; set; }

    }
}
