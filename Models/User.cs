using System;
using System.ComponentModel.DataAnnotations;

namespace HseBackend.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;
        
        [Required]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty; // In a real app, store Hash

        public string? JobNumber { get; set; }
        public string? Mobile { get; set; }
        public string? Department { get; set; }
        public string? ProfilePicture { get; set; } // Base64 string

        public string Role { get; set; } = "Employee"; // Employee, Inspector, Supervisor, Recipient
    }
}
