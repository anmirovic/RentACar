using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace Databaseaccess.Models
{
    public class Vehicle
    {
        // CREATE (n:User {Username: "dianne", email: "dia@themail.com", password:"hHJgYzI26pIaO", role: "user"});
        public int Id { get; set; }
        public string VehicleType { get; set; }
        public string Brand { get; set; }
        public double DailyPrice { get; set; }
        public bool Availability { get; set; }

        public List<Reservation> Reservations { get; set; }
        public List<Review> Reviews { get; set; }
    }
}