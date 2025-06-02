// In Services/IBlobStorageService.cs (Backend Project)
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace EduSync.Services // Or your project's namespace
{
    public interface IBlobStorageService
    {
        /// <summary>
        /// Uploads a file to Azure Blob Storage and returns its public URL.
        /// </summary>
        /// <param name="file">The file to upload, received as IFormFile.</param>
        /// <param name="fileName">The desired name for the blob (can include path prefixes for organization).</param>
        /// <returns>The public URL of the uploaded blob.</returns>
        Task<string> UploadFileAsync(IFormFile file, string fileName);

        // We can add other methods later, e.g.:
        // Task DeleteFileAsync(string fileName);
        // Task<Stream> DownloadFileAsync(string fileName);
    }
}
