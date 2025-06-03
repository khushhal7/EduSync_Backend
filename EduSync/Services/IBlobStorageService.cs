// In Services/IBlobStorageService.cs (Backend Project)
using Microsoft.AspNetCore.Http;
using System.IO; // Required for Stream
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models; // Required for BlobDownloadResult (or a custom class)

namespace EduSync.Services // Or your project's namespace
{
    // Custom class to hold download details
    public class BlobDownloadInfo
    {
        public Stream Content { get; set; }
        public string ContentType { get; set; }
        public string? FileName { get; set; } // Optional: to suggest a download filename
    }

    public interface IBlobStorageService
    {
        /// <summary>
        /// Uploads a file to Azure Blob Storage and returns its public URL.
        /// </summary>
        /// <param name="file">The file to upload, received as IFormFile.</param>
        /// <param name="fileName">The desired name for the blob (can include path prefixes for organization).</param>
        /// <returns>The public URL of the uploaded blob.</returns>
        Task<string> UploadFileAsync(IFormFile file, string fileName);

        /// <summary>
        /// Downloads a file from Azure Blob Storage.
        /// </summary>
        /// <param name="blobName">The name (including any path) of the blob to download.</param>
        /// <returns>A BlobDownloadInfo object containing the file stream, content type, and an optional filename; or null if blob not found.</returns>
        Task<BlobDownloadInfo?> DownloadFileAsync(string blobName);

        // We can add other methods later, e.g.:
        // Task DeleteFileAsync(string blobName);
    }
}
