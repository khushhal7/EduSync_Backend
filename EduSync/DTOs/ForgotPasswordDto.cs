// In DTOs/ForgotPasswordDto.cs (Backend Project)
using System.ComponentModel.DataAnnotations;

namespace EduSync.DTOs // Ensure this namespace matches your project's DTOs namespace
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }
    }
}
