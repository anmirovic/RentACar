using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace Databaseaccess.Models
{
    public class Vehicle
    {
        public string Id { get; set; }
        [Required]
        public string VehicleType { get; set; }
        [Required]
        public string Brand { get; set; }
        [Required]
        public double DailyPrice { get; set; }
        [Required]
        public bool Availability { get; set; }

        [JsonIgnore]
        public List<Reservation>? Reservations { get; set; }
        [JsonIgnore]
        public List<Review>? Reviews { get; set; }
    }
}