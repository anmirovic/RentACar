using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace Databaseaccess.Models
{
    public class User
    {
        // CREATE (n:User {Username: "dianne", email: "dia@themail.com", password:"hHJgYzI26pIaO", role: "user"});
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }

        public List<Reservation> Reservations { get; set; }
        public List<Review> Reviews { get; set; }
        
    }
}