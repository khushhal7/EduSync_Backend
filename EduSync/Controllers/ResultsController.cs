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
using EduSync.Services; // <-- Add this for IEventHubService
using System.Text.Json;  // <-- Add this for JsonSerializer

namespace EduSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly EduSyncDbContext _context;
        private readonly IEventHubService _eventHubService; // <-- Inject IEventHubService

        // Update constructor to inject IEventHubService
        public ResultsController(EduSyncDbContext context, IEventHubService eventHubService)
        {
            _context = context;
            _eventHubService = eventHubService;
        }

        // POST: api/results
        [HttpPost]
        public async Task<IActionResult> CreateResult(ResultForCreationDto resultForCreationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var assessment = await _context.Assessments.FindAsync(resultForCreationDto.AssessmentId);
            if (assessment == null)
            {
                return BadRequest($"Assessment with ID {resultForCreationDto.AssessmentId} not found.");
            }

            var user = await _context.Users.FindAsync(resultForCreationDto.UserId);
            if (user == null)
            {
                return BadRequest($"User with ID {resultForCreationDto.UserId} not found.");
            }

            if (user.Role != "Student")
            {
                return BadRequest($"User with ID {resultForCreationDto.UserId} is not a student and cannot submit results.");
            }

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
                AttemptDate = DateTime.UtcNow
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync(); // Save result to the database

            var resultToReturn = new ResultDto
            {
                ResultId = result.ResultId,
                AssessmentId = result.AssessmentId,
                AssessmentTitle = assessment.Title,
                UserId = result.UserId,
                UserName = user.Name,
                Score = result.Score,
                AttemptDate = result.AttemptDate
            };

            // --- Send event to Event Hub AFTER successful save ---
            try
            {
                // Serialize the result DTO (or a custom event object) to JSON
                string eventDataJson = JsonSerializer.Serialize(resultToReturn);
                await _eventHubService.SendEventAsync(eventDataJson);
                System.Diagnostics.Debug.WriteLine($"Successfully sent quiz attempt event to Event Hub for ResultId: {result.ResultId}");
            }
            catch (Exception ex)
            {
                // Log the error from sending to Event Hub, but don't fail the HTTP response
                // because the primary operation (saving the result) was successful.
                // In a production system, use a proper logger (e.g., ILogger).
                System.Diagnostics.Debug.WriteLine($"Error sending quiz attempt event to Event Hub for ResultId {result.ResultId}: {ex.Message}");
                // Optionally, you might want to add this to a dead-letter queue or retry mechanism for events.
            }
            // --- End Send event to Event Hub ---

            return CreatedAtAction(nameof(GetResultById), new { id = result.ResultId }, resultToReturn);
        }

        // GET: api/results/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ResultDto>> GetResultById(Guid id)
        {
            var result = await _context.Results
                .Include(r => r.Assessment)
                .Include(r => r.User)
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
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            var results = await _context.Results
                .Where(r => r.UserId == userId)
                .Include(r => r.Assessment)
                .Include(r => r.User)
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
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/results/assessment/{assessmentId}
        [HttpGet("assessment/{assessmentId}")]
        public async Task<ActionResult<IEnumerable<ResultDto>>> GetResultsForAssessment(Guid assessmentId)
        {
            var assessment = await _context.Assessments.FindAsync(assessmentId);
            if (assessment == null)
            {
                return NotFound($"Assessment with ID {assessmentId} not found.");
            }

            var results = await _context.Results
                .Where(r => r.AssessmentId == assessmentId)
                .Include(r => r.Assessment)
                .Include(r => r.User)
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
                .ToListAsync();

            return Ok(results);
        }
    }
}
