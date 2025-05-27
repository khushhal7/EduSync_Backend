// EduSync/DTOs/AssessmentDto.cs
using System;

namespace EduSync.DTOs
{
    public class AssessmentDto
    {
        public Guid AssessmentId { get; set; }
        public Guid CourseId { get; set; }
        public string Title { get; set; }
        public string Questions { get; set; } // Quiz content as a JSON string
        public int MaxScore { get; set; }
        // You might also want to include CourseTitle here if needed, requiring a join.
    }
}