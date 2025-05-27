// EduSync/DTOs/AssessmentForCreationDto.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace EduSync.DTOs
{
    public class AssessmentForCreationDto
    {
        [Required(ErrorMessage = "Course ID is required.")]
        public Guid CourseId { get; set; } // FK to Course

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title can't be longer than 200 characters.")]
        public string Title { get; set; } // Test title

        [Required(ErrorMessage = "Questions are required.")]
        public string Questions { get; set; } // Quiz content as a JSON string

        [Required(ErrorMessage = "Max score is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Max score must be at least 1.")]
        public int MaxScore { get; set; } // Maximum marks
    }
}