using Neo4j.Driver;
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
        public string Id { get; set; }
        //[Required]
        //public DateTime ReservationDate { get; set; }
        //[Required]
        //public int Duration { get; set; } //in days
        //[Required]
        //public DateTime PickupDate { get; set; }
        //[Required]
        //public DateTime ReturnDate { get; set; }
        [Required]
        public DateTime PickupDate { get; set; }
        [Required]
        public DateTime ReturnDate { get; set; }
        [JsonIgnore]
        public List<Review>? Reviews { get; set; }
        
    }
}