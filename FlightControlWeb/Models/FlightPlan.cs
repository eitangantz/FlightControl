using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{

    public class FlightPlan
    {
        public int passengers { get; set; }
        public string company_name { get; set; }
        public Initial_Location initial_location { get; set; }
        public Segment[] segments { get; set; }
    }

    public class Initial_Location
    {
        public float longitude { get; set; }//////////////////////////////////////
        public float latitude { get; set; }//////////////////////////////////////
        public DateTime date_time { get; set; }
    }

    public class Segment
    {
        public float longitude { get; set; }//////////////////////////////////////
        public float latitude { get; set; }//////////////////////////////////////
        public int timespan_seconds { get; set; }
    }

}
