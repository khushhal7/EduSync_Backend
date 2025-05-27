// In Models/Assessment.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduSync.Models // Updated namespace
{
    public class Assessment
    {
        [Key]
        public Guid AssessmentId { get; set; } // Unique identifier [cite: 14]

        [Required]
        public Guid CourseId { get; set; } // FK to Course [cite: 14]

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } // Test title [cite: 14]

        public string Questions { get; set; } // Quiz content (JSON) [cite: 14]

        public int MaxScore { get; set; } // Maximum marks [cite: 14]
    }
}