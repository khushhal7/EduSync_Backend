// EduSync/DTOs/CourseForUpdateDto.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace EduSync.DTOs
{
    public class CourseForUpdateDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title can't be longer than 200 characters.")]
        public string Title { get; set; }

        [StringLength(1000, ErrorMessage = "Description can't be longer than 1000 characters.")]
        public string Description { get; set; }

        // InstructorId might not be updatable by a regular instructor,
        // or only by an admin. For now, we'll allow it to be updated for simplicity.
        // If it shouldn't be updatable, you would remove it from this DTO.
        [Required(ErrorMessage = "Instructor ID is required.")]
        public Guid InstructorId { get; set; }

        public string MediaUrl { get; set; }
    }
}