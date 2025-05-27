// EduSync/DTOs/CourseDto.cs
using System;

namespace EduSync.DTOs
{
    public class CourseDto
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid InstructorId { get; set; }
        public string InstructorName { get; set; } // We can populate this by joining with the User table
        public string MediaUrl { get; set; }
    }
}