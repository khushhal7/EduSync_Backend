// EduSync/Controllers/ResultsController.cs
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
    public class ResultsController : ControllerBase
    {
        private readonly EduSyncDbContext _context;

        public ResultsController(EduSyncDbContext context)
        {
            _context = context;
        }

        // POST: api/results
        [HttpPost]
        public async Task<IActionResult> CreateResult(ResultForCreationDto resultForCreationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the Assessment exists
            var assessment = await _context.Assessments.FindAsync(resultForCreationDto.AssessmentId);
            if (assessment == null)
            {
                return BadRequest($"Assessment with ID {resultForCreationDto.AssessmentId} not found.");
            }

            // Check if the User (student) exists
            var user = await _context.Users.FindAsync(resultForCreationDto.UserId);
            if (user == null)
            {
                return BadRequest($"User with ID {resultForCreationDto.UserId} not found.");
            }

            // **Role Check: Ensure the user submitting the result is a Student**
            if (user.Role != "Student")
            {
                return BadRequest($"User with ID {resultForCreationDto.UserId} is not a student and cannot submit results.");
            }


            // Optional: Check if score exceeds MaxScore for the assessment
            if (resultForCreationDto.Score > assessment.MaxScore)
            {
                return BadRequest($"Score ({resultForCreationDto.Score}) cannot exceed the maximum score ({assessment.MaxScore}) for this assessment.");
            }


            var result = new Result
            {
                ResultId = Guid.NewGuid(),
                AssessmentId = resultForCreationDto.AssessmentId,
                UserId = resultForCreationDto.UserId,
                Score = resultForCreationDto.Score,
                AttemptDate = DateTime.UtcNow // Set attempt date on the server
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            var resultToReturn = new ResultDto
            {
                ResultId = result.ResultId,
                AssessmentId = result.AssessmentId,
                AssessmentTitle = assessment.Title, // Populate from fetched assessment
                UserId = result.UserId,
                UserName = user.Name, // Populate from fetched user
                Score = result.Score,
                AttemptDate = result.AttemptDate
            };

            return CreatedAtAction(nameof(GetResultById), new { id = result.ResultId }, resultToReturn);
        }

        // GET: api/results/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ResultDto>> GetResultById(Guid id)
        {
            var result = await _context.Results
                .Include(r => r.Assessment) // Include Assessment for AssessmentTitle
                .Include(r => r.User)       // Include User for UserName
                .Where(r => r.ResultId == id)
                .Select(r => new ResultDto
                {
                    ResultId = r.ResultId,
                    AssessmentId = r.AssessmentId,
                    AssessmentTitle = r.Assessment.Title,
                    UserId = r.UserId,
                    UserName = r.User.Name,
                    Score = r.Score,
                    AttemptDate = r.AttemptDate
                })
                .FirstOrDefaultAsync();

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        // GET: api/results/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ResultDto>>> GetResultsForUser(Guid userId)
        {
            // Check if user exists
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            var results = await _context.Results
                .Where(r => r.UserId == userId)
                .Include(r => r.Assessment)
                .Include(r => r.User) // Though User is already known, including for consistency or if other user details were needed
                .Select(r => new ResultDto
                {
                    ResultId = r.ResultId,
                    AssessmentId = r.AssessmentId,
                    AssessmentTitle = r.Assessment.Title,
                    UserId = r.UserId,
                    UserName = r.User.Name, // Or user.Name directly from the user variable
                    Score = r.Score,
                    AttemptDate = r.AttemptDate
                })
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/results/assessment/{assessmentId}
        [HttpGet("assessment/{assessmentId}")]
        public async Task<ActionResult<IEnumerable<ResultDto>>> GetResultsForAssessment(Guid assessmentId)
        {
            // Check if assessment exists
            var assessment = await _context.Assessments.FindAsync(assessmentId);
            if (assessment == null)
            {
                return NotFound($"Assessment with ID {assessmentId} not found.");
            }

            var results = await _context.Results
                .Where(r => r.AssessmentId == assessmentId)
                .Include(r => r.Assessment) // Though Assessment is known, including for consistency
                .Include(r => r.User)
                .Select(r => new ResultDto
                {
                    ResultId = r.ResultId,
                    AssessmentId = r.AssessmentId,
                    AssessmentTitle = r.Assessment.Title, // Or assessment.Title directly
                    UserId = r.UserId,
                    UserName = r.User.Name,
                    Score = r.Score,
                    AttemptDate = r.AttemptDate
                })
                .ToListAsync();

            return Ok(results);
        }
    }
}