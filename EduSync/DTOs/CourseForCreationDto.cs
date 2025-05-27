// EduSync/DTOs/CourseForCreationDto.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace EduSync.DTOs
{
    public class CourseForCreationDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title can't be longer than 200 characters.")]
        public string Title { get; set; }

        [StringLength(1000, ErrorMessage = "Description can't be longer than 1000 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Instructor ID is required.")]
        public Guid InstructorId { get; set; } // Later, this might be inferred from the logged-in user

        public string MediaUrl { get; set; } // For now, a simple string URL
    }
}