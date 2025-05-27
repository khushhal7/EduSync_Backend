// EduSync/Controllers/CoursesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduSync.Data;
using EduSync.Models;
using EduSync.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduSync.Controllers
{
    [Route("api/[controller]")] // Defines the base route: /api/courses
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly EduSyncDbContext _context;

        public CoursesController(EduSyncDbContext context)
        {
            _context = context;
        }

        // POST: api/courses
        [HttpPost]
        public async Task<IActionResult> CreateCourse(CourseForCreationDto courseForCreationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // In a real application with authentication, you'd verify the InstructorId
            // or get it from the logged-in user's claims.
            // Also, check if the instructor exists.
            var instructor = await _context.Users.FirstOrDefaultAsync(u => u.UserId == courseForCreationDto.InstructorId && u.Role == "Instructor");
            if (instructor == null)
            {
                return BadRequest("Invalid Instructor ID or user is not an Instructor.");
            }

            var course = new Course
            {
                CourseId = Guid.NewGuid(),
                Title = courseForCreationDto.Title,
                Description = courseForCreationDto.Description,
                InstructorId = courseForCreationDto.InstructorId,
                MediaUrl = courseForCreationDto.MediaUrl
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            // Prepare the CourseDto to return
            var courseToReturn = new CourseDto
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Description = course.Description,
                InstructorId = course.InstructorId,
                InstructorName = instructor.Name, // Assuming instructor object is fetched
                MediaUrl = course.MediaUrl
            };

            return CreatedAtAction(nameof(GetCourseById), new { id = course.CourseId }, courseToReturn);
        }

        // GET: api/courses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetAllCourses()
        {
            var courses = await _context.Courses
                .Include(c => c.Instructor) // Include the related Instructor (User) entity
                .Select(c => new CourseDto
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Description = c.Description,
                    InstructorId = c.InstructorId,
                    InstructorName = c.Instructor.Name, // Access the Name from the included Instructor
                    MediaUrl = c.MediaUrl
                })
                .ToListAsync();

            return Ok(courses);
        }

        // GET: api/courses/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CourseDto>> GetCourseById(Guid id)
        {
            var course = await _context.Courses
                .Include(c => c.Instructor) // Include the related Instructor
                .Where(c => c.CourseId == id)
                .Select(c => new CourseDto
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Description = c.Description,
                    InstructorId = c.InstructorId,
                    InstructorName = c.Instructor.Name,
                    MediaUrl = c.MediaUrl
                })
                .FirstOrDefaultAsync();

            if (course == null)
            {
                return NotFound();
            }

            return Ok(course);
        }

        // PUT: api/courses/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(Guid id, CourseForUpdateDto courseForUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var courseFromDb = await _context.Courses.FindAsync(id);

            if (courseFromDb == null)
            {
                return NotFound($"Course with ID {id} not found.");
            }

            // Optional: Validate the new InstructorId if it's being changed
            if (courseFromDb.InstructorId != courseForUpdateDto.InstructorId)
            {
                var instructor = await _context.Users.FirstOrDefaultAsync(u => u.UserId == courseForUpdateDto.InstructorId && u.Role == "Instructor");
                if (instructor == null)
                {
                    return BadRequest("Invalid new Instructor ID or user is not an Instructor.");
                }
            }

            // Update properties
            courseFromDb.Title = courseForUpdateDto.Title;
            courseFromDb.Description = courseForUpdateDto.Description;
            courseFromDb.InstructorId = courseForUpdateDto.InstructorId;
            courseFromDb.MediaUrl = courseForUpdateDto.MediaUrl;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/courses/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(Guid id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound($"Course with ID {id} not found.");
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CourseExists(Guid id)
        {
            return _context.Courses.Any(e => e.CourseId == id);
        }
    }
}