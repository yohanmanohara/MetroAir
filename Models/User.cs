using System;
using System.ComponentModel.DataAnnotations;

namespace MetroAir.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty; 

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "User";

        public string FullName  { get; set; } = string.Empty; 

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

      
        public string PhoneNumber { get; set; } = string.Empty;

      
        public string Address { get; set; } = string.Empty;

        
        public string City { get; set; } = string.Empty;

       
        public string State { get; set; } = string.Empty;

       
        public string ZipCode { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
