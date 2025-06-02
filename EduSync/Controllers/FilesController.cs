// In Controllers/FilesController.cs (Backend Project)
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using EduSync.Services; // For IBlobStorageService

namespace EduSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly ILogger<FilesController> _logger; // Optional: for logging

        public FilesController(IBlobStorageService blobStorageService, ILogger<FilesController> logger)
        {
            _blobStorageService = blobStorageService;
            _logger = logger;
        }

        // POST: api/files/upload
        [HttpPost("upload")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        // You might want to add authorization here later, e.g., [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded or file is empty." });
            }

            // Basic validation for file type or size (optional, can be more extensive)
            // Example: Limit file size to 10MB
            if (file.Length > 10 * 1024 * 1024) // 10 MB
            {
                return BadRequest(new { message = "File size exceeds the 10MB limit." });
            }

            // Example: Allow only certain content types
            // var allowedContentTypes = new[] { "image/jpeg", "image/png", "application/pdf", "video/mp4" };
            // if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            // {
            //     return BadRequest(new { message = $"File type '{file.ContentType}' is not allowed." });
            // }

            try
            {
                // Generate a unique file name to prevent overwriting and ensure uniqueness.
                // You can include a folder structure here, e.g., "coursemedia/{courseId}/"
                // For now, just a GUID prefix + original file name (sanitized).

                // Sanitize original file name to remove invalid characters for a URL/blob name
                var originalFileName = Path.GetFileName(file.FileName);
                var sanitizedFileName = Path.GetInvalidFileNameChars()
                    .Aggregate(originalFileName, (current, c) => current.Replace(c.ToString(), string.Empty))
                    .Replace(" ", "_"); // Replace spaces with underscores

                if (string.IsNullOrWhiteSpace(sanitizedFileName))
                {
                    sanitizedFileName = "uploadedfile"; // Fallback if original name is all invalid chars
                }

                // Create a more unique blob name
                var uniqueBlobName = $"{Guid.NewGuid()}_{sanitizedFileName}";

                _logger.LogInformation("Attempting to upload file: {FileName}, Blob Name: {BlobName}, ContentType: {ContentType}, Size: {Size}",
                    file.FileName, uniqueBlobName, file.ContentType, file.Length);

                string fileUrl = await _blobStorageService.UploadFileAsync(file, uniqueBlobName);

                _logger.LogInformation("File uploaded successfully. URL: {FileUrl}", fileUrl);

                // Return the public URL of the uploaded file
                return Ok(new { url = fileUrl });
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning("Argument error during file upload: {ErrorMessage}", argEx.Message);
                return BadRequest(new { message = argEx.Message });
            }
            catch (ApplicationException appEx) // Catch specific exception from BlobStorageService
            {
                _logger.LogError(appEx, "Application error during file upload to blob storage.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"An error occurred during upload: {appEx.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during file upload.");
                // In a production environment, you might not want to return ex.Message directly
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred while uploading the file. Please try again." });
            }
        }
    }
}
