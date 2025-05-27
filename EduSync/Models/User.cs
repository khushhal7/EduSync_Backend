// In Models/User.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace EduSync.Models // Updated namespace
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; } // Unique identifier [cite: 10]

        [Required]
        [StringLength(100)]
        public string Name { get; set; } // Full name [cite: 10]

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } // Email address [cite: 10]

        [Required]
        public string Role { get; set; } // Student/Instructor [cite: 10]

        [Required]
        public string PasswordHash { get; set; } // Secure password storage [cite: 10]

        // New properties for Password Reset
        public string? PasswordResetToken { get; set; } // Nullable string for the reset token
        public DateTime? ResetTokenExpiry { get; set; } // Nullable DateTime for token expiration
    }
}
