using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace Databaseaccess.Models
{
    public class Reservation
    {
    
        [Required]
        public DateTime ReservationDate { get; set; }
        [Required]
        public int Duration { get; set; } //in days
      
        [JsonIgnore]
        public List<Review>? Reviews { get; set; }
        
    }
}