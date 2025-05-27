// EduSync/DTOs/AssessmentForUpdateDto.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace EduSync.DTOs
{
    public class AssessmentForUpdateDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title can't be longer than 200 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Questions are required.")]
        public string Questions { get; set; } // Quiz content as a JSON string

        [Required(ErrorMessage = "Max score is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Max score must be at least 1.")]
        public int MaxScore { get; set; }
        // Note: CourseId is typically not updated for an existing assessment.
        // If it were, you'd include it and validate it.
    }
}