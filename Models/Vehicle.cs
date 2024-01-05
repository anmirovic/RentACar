using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace Databaseaccess.Models
{
    public class Vehicle
    {
        
        public string VehicleType { get; set; }
        public string Brand { get; set; }
        public double DailyPrice { get; set; }
        public bool Availability { get; set; }

        [JsonIgnore]
        public List<Reservation> Reservations { get; set; }
        [JsonIgnore]
        public List<Review> Reviews { get; set; }
    }
}