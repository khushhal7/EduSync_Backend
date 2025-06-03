// In Controllers/FilesController.cs (Backend Project)
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using EduSync.Services; // For IBlobStorageService
using System.Net.Mime; // Required for ContentDisposition

namespace EduSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly ILogger<FilesController> _logger;

        public FilesController(IBlobStorageService blobStorageService, ILogger<FilesController> logger)
        {
            _blobStorageService = blobStorageService;
            _logger = logger;
        }

        // POST: api/files/upload
        [HttpPost("upload")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)] // Should be an object { url: "..." }
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        // TODO: Add authorization, e.g., [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded or file is empty." });
            }

            if (file.Length > 10 * 1024 * 1024) // 10 MB limit example
            {
                return BadRequest(new { message = "File size exceeds the 10MB limit." });
            }

            try
            {
                var originalFileName = Path.GetFileName(file.FileName);
                var sanitizedFileName = Path.GetInvalidFileNameChars()
                    .Aggregate(originalFileName, (current, c) => current.Replace(c.ToString(), string.Empty))
                    .Replace(" ", "_");

                if (string.IsNullOrWhiteSpace(sanitizedFileName))
                {
                    sanitizedFileName = "uploadedfile";
                }

                var uniqueBlobName = $"{Guid.NewGuid()}_{sanitizedFileName}";

                _logger.LogInformation("Attempting to upload file: {OriginalFileName}, Blob Name: {BlobName}, ContentType: {ContentType}, Size: {Size}",
                    file.FileName, uniqueBlobName, file.ContentType, file.Length);

                string fileUrl = await _blobStorageService.UploadFileAsync(file, uniqueBlobName);

                _logger.LogInformation("File uploaded successfully. URL: {FileUrl}", fileUrl);

                return Ok(new { url = fileUrl, blobName = uniqueBlobName }); // Return blobName as well
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning("Argument error during file upload: {ErrorMessage}", argEx.Message);
                return BadRequest(new { message = argEx.Message });
            }
            catch (ApplicationException appEx)
            {
                _logger.LogError(appEx, "Application error during file upload to blob storage.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"An error occurred during upload: {appEx.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during file upload.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred while uploading the file. Please try again." });
            }
        }

        // GET: api/files/download/{*blobName}
        [HttpGet("download/{*blobName}")] // Route allows for paths in blobName
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileStreamResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        // TODO: Add authorization if downloads should be restricted
        public async Task<IActionResult> DownloadFile(string blobName)
        {
            if (string.IsNullOrWhiteSpace(blobName))
            {
                return BadRequest(new { message = "Blob name must be provided." });
            }

            try
            {
                _logger.LogInformation("Attempting to download blob: {BlobName}", blobName);
                BlobDownloadInfo? downloadInfo = await _blobStorageService.DownloadFileAsync(blobName);

                if (downloadInfo == null || downloadInfo.Content == null)
                {
                    _logger.LogWarning("Blob not found or content is null for blob: {BlobName}", blobName);
                    return NotFound(new { message = "The requested file was not found." });
                }

                // Ensure the stream is at the beginning before returning it
                downloadInfo.Content.Position = 0;

                // Determine a user-friendly download filename.
                // The blobName might include GUIDs or paths. We can try to extract a more original-like name.
                // For now, we'll use the blobName itself, or you could derive it.
                var downloadFileName = Path.GetFileName(blobName); // Gets the last part of the path
                // If uniqueBlobName was "GUID_actualFileName.ext", this gets "GUID_actualFileName.ext"
                // You might want to strip the GUID prefix for a cleaner download name if you stored it.

                _logger.LogInformation("Streaming file for download: {DownloadFileName}, ContentType: {ContentType}", downloadFileName, downloadInfo.ContentType);

                return File(downloadInfo.Content, downloadInfo.ContentType, downloadFileName);
                // The third parameter (fileDownloadName) in File() sets Content-Disposition to attachment.
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning("Argument error during file download for blob {BlobName}: {ErrorMessage}", blobName, argEx.Message);
                return BadRequest(new { message = argEx.Message });
            }
            catch (ApplicationException appEx)
            {
                _logger.LogError(appEx, "Application error during file download for blob {BlobName}", blobName);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"An error occurred while downloading the file: {appEx.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during file download for blob {BlobName}", blobName);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred while downloading the file." });
            }
        }
    }
}
