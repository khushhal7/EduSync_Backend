// In Models/Course.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduSync.Models // Updated namespace
{
    public class Course
    {
        [Key]
        public Guid CourseId { get; set; } // Unique identifier [cite: 12]

        [Required]
        [StringLength(200)]
        public string Title { get; set; } // Course title [cite: 12]

        public string Description { get; set; } // Summary of content [cite: 12]

        [Required]
        public Guid InstructorId { get; set; } // FK to User [cite: 12]

        [ForeignKey("InstructorId")]
        public virtual User Instructor { get; set; }

        public string MediaUrl { get; set; } // Link to Blob Storage [cite: 12]
    }
}