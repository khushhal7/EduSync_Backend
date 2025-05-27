// In Models/Result.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduSync.Models // Updated namespace
{
    public class Result
    {
        [Key]
        public Guid ResultId { get; set; } // Unique identifier [cite: 16]

        [Required]
        public Guid AssessmentId { get; set; } // FK to Assessment [cite: 16]

        [ForeignKey("AssessmentId")]
        public virtual Assessment Assessment { get; set; }

        [Required]
        public Guid UserId { get; set; } // FK to User [cite: 16]

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public int Score { get; set; } // Achieved score [cite: 16]

        public DateTime AttemptDate { get; set; } // Test attempt time [cite: 16]
    }
}