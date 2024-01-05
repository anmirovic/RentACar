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
        // CREATE (n:User {Username: "dianne", email: "dia@themail.com", password:"hHJgYzI26pIaO", role: "user"});
        public int Id { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; } 
        public string Comment { get; set; }
        
        
    }
}