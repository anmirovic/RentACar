using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Databaseaccess.Models
{
    public class Review
    {
        
        
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; } 
        public string Comment { get; set; }
        
        
    }
}