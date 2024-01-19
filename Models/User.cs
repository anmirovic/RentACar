using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;


namespace Databaseaccess.Models
{
    public class User
    {
        public string Id { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Role { get; set; }

        [JsonIgnore]
        public List<Reservation>? Reservations { get; set; }
        [JsonIgnore]
        public List<Review>? Reviews { get; set; }
        [JsonIgnore]
        public List<Vehicle>? Vehicles { get; set; }
        
       
        
    }
}