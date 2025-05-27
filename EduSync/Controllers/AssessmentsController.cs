// EduSync/Controllers/AssessmentsController.cs
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
    [Route("api/[controller]")]
    [ApiController]
    public class AssessmentsController : ControllerBase
    {
        private readonly EduSyncDbContext _context;

        public AssessmentsController(EduSyncDbContext context)
        {
            _context = context;
        }

        // POST: api/assessments
        [HttpPost]
        public async Task<IActionResult> CreateAssessment(AssessmentForCreationDto assessmentForCreationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the course exists and include its instructor for role validation
            var course = await _context.Courses
                                     .Include(c => c.Instructor) // Include Instructor
                                     .FirstOrDefaultAsync(c => c.CourseId == assessmentForCreationDto.CourseId);
            if (course == null)
            {
                return BadRequest($"Course with ID {assessmentForCreationDto.CourseId} not found.");
            }

            // **Role Check: Ensure the course's instructor is indeed an "Instructor"**
            if (course.Instructor == null || course.Instructor.Role != "Instructor")
            {
                return BadRequest("Assessments can only be added to courses managed by a valid instructor.");
            }

            // In a real application, you'd also check if the logged-in user (instructor)
            // has permission to add an assessment to this course (e.g., is course.InstructorId).

            var assessment = new Assessment
            {
                AssessmentId = Guid.NewGuid(),
                CourseId = assessmentForCreationDto.CourseId,
                Title = assessmentForCreationDto.Title,
                Questions = assessmentForCreationDto.Questions, // Storing JSON string
                MaxScore = assessmentForCreationDto.MaxScore
            };

            _context.Assessments.Add(assessment);
            await _context.SaveChangesAsync();

            var assessmentToReturn = new AssessmentDto
            {
                AssessmentId = assessment.AssessmentId,
                CourseId = assessment.CourseId,
                Title = assessment.Title,
                Questions = assessment.Questions,
                MaxScore = assessment.MaxScore
            };

            return CreatedAtAction(nameof(GetAssessmentById), new { id = assessment.AssessmentId }, assessmentToReturn);
        }

        // GET: api/assessments/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AssessmentDto>> GetAssessmentById(Guid id)
        {
            var assessment = await _context.Assessments
                .Where(a => a.AssessmentId == id)
                .Select(a => new AssessmentDto
                {
                    AssessmentId = a.AssessmentId,
                    CourseId = a.CourseId,
                    Title = a.Title,
                    Questions = a.Questions,
                    MaxScore = a.MaxScore
                })
                .FirstOrDefaultAsync();

            if (assessment == null)
            {
                return NotFound();
            }

            return Ok(assessment);
        }

        // GET: api/courses/{courseId}/assessments
        [HttpGet("/api/courses/{courseId}/assessments")] // Custom route for assessments by course
        public async Task<ActionResult<IEnumerable<AssessmentDto>>> GetAssessmentsForCourse(Guid courseId)
        {
            // Check if the course exists
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                return NotFound($"Course with ID {courseId} not found.");
            }

            var assessments = await _context.Assessments
                .Where(a => a.CourseId == courseId)
                .Select(a => new AssessmentDto
                {
                    AssessmentId = a.AssessmentId,
                    CourseId = a.CourseId,
                    Title = a.Title,
                    Questions = a.Questions,
                    MaxScore = a.MaxScore
                })
                .ToListAsync();

            return Ok(assessments);
        }

        // PUT: api/assessments/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAssessment(Guid id, AssessmentForUpdateDto assessmentForUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var assessmentFromDb = await _context.Assessments
                                               .Include(a => a.Course)
                                               .ThenInclude(c => c.Instructor) // Include Course and its Instructor
                                               .FirstOrDefaultAsync(a => a.AssessmentId == id);

            if (assessmentFromDb == null)
            {
                return NotFound($"Assessment with ID {id} not found.");
            }

            // **Role Check: Ensure the associated course's instructor is valid**
            if (assessmentFromDb.Course?.Instructor == null || assessmentFromDb.Course.Instructor.Role != "Instructor")
            {
                return BadRequest("Cannot update assessment: The associated course does not have a valid instructor.");
            }
            // In a real app, also check if the logged-in user has permission (e.g., is assessmentFromDb.Course.InstructorId)

            assessmentFromDb.Title = assessmentForUpdateDto.Title;
            assessmentFromDb.Questions = assessmentForUpdateDto.Questions;
            assessmentFromDb.MaxScore = assessmentForUpdateDto.MaxScore;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Assessments.Any(e => e.AssessmentId == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // Standard response for a successful PUT
        }

        // DELETE: api/assessments/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssessment(Guid id)
        {
            var assessment = await _context.Assessments
                                         .Include(a => a.Course)
                                         .ThenInclude(c => c.Instructor) // Include Course and its Instructor
                                         .FirstOrDefaultAsync(a => a.AssessmentId == id);

            if (assessment == null)
            {
                return NotFound($"Assessment with ID {id} not found.");
            }

            // **Role Check: Ensure the associated course's instructor is valid**
            if (assessment.Course?.Instructor == null || assessment.Course.Instructor.Role != "Instructor")
            {
                return BadRequest("Cannot delete assessment: The associated course does not have a valid instructor.");
            }
            // In a real app, also check if the logged-in user has permission

            _context.Assessments.Remove(assessment);
            await _context.SaveChangesAsync();

            return NoContent(); // Standard response for a successful DELETE
        }
    }
}