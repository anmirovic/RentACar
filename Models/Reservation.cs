using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace Databaseaccess.Models
{
    public class Reservation
    {
        // CREATE (n:User {Username: "dianne", email: "dia@themail.com", password:"hHJgYzI26pIaO", role: "user"});
        public int Id { get; set; }
        public DateTime ReservationDate { get; set; }
        public int Duration { get; set; } //in days
      
        public List<Review> Reviews { get; set; }
        
    }
}