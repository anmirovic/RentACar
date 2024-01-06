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
        [Required]
        public string Username { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Role { get; set; }

        [JsonIgnore]
        [BindRequired]
        public List<Reservation>? Reservations { get; set; }
        [JsonIgnore]
        [BindRequired]
        public List<Review>? Reviews { get; set; }
        
       
        
    }
}