// EduSync/DTOs/UserForRegistrationDto.cs
using System.ComponentModel.DataAnnotations;

namespace EduSync.DTOs
{
    public class UserForRegistrationDto
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name can't be longer than 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [StringLength(100, ErrorMessage = "Email can't be longer than 100 characters.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string Password { get; set; } // Plain text password from the user

        [Required(ErrorMessage = "Role is required.")]
        public string Role { get; set; } // Expected values: "Student" or "Instructor"
    }
}