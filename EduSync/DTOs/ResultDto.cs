// EduSync/DTOs/ResultDto.cs
using System;

namespace EduSync.DTOs
{
    public class ResultDto
    {
        public Guid ResultId { get; set; } // [cite: 16]
        public Guid AssessmentId { get; set; } // [cite: 16]
        public string AssessmentTitle { get; set; } // Populated from related Assessment
        public Guid UserId { get; set; } // [cite: 16]
        public string UserName { get; set; } // Populated from related User
        public int Score { get; set; } // [cite: 16]
        public DateTime AttemptDate { get; set; } // [cite: 16]
    }
}