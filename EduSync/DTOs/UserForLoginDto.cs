// EduSync/DTOs/UserForLoginDto.cs
using System.ComponentModel.DataAnnotations;

namespace EduSync.DTOs
{
    public class UserForLoginDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
    }
}