using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Irish_Beauty_Product.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        [Required]
        public string Username { get; set; } = " ";
        [Required]
        public string Password { get; set; } = " ";
        public string Role { get; set; } = " ";   
        public string Status { get; set; } = "Active";
      
    }
}
