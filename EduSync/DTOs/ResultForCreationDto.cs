// EduSync/DTOs/ResultForCreationDto.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace EduSync.DTOs
{
    public class ResultForCreationDto
    {
        [Required(ErrorMessage = "Assessment ID is required.")]
        public Guid AssessmentId { get; set; } // FK to Assessment [cite: 16]

        [Required(ErrorMessage = "User ID is required.")]
        public Guid UserId { get; set; } // FK to User (Student) [cite: 16]

        [Required(ErrorMessage = "Score is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Score cannot be negative.")]
        public int Score { get; set; } // Achieved score [cite: 16]
    }
}