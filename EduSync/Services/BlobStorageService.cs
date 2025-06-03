// In Services/BlobStorageService.cs (Backend Project)
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace EduSync.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly string _connectionString;
        private readonly string _containerName;

        public BlobStorageService(IConfiguration configuration)
        {
            //_connectionString = configuration.GetConnectionString("MyBlobStorage");
            _connectionString = configuration["MyBlobStorage:ConnectionString"];
            _containerName = configuration["MyBlobStorage:ContainerName"];

            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Azure Blob Storage ConnectionString (named 'MyBlobStorage') is not configured correctly in the ConnectionStrings section.");
            }
            if (string.IsNullOrEmpty(_containerName))
            {
                throw new InvalidOperationException("Azure Blob Storage ContainerName is not configured. Check configuration section 'AzureBlobStorage:ContainerName'.");
            }
        }

        public async Task<string> UploadFileAsync(IFormFile file, string fileName)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File cannot be null or empty.", nameof(file));
            }
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("A unique blob name (fileName) must be provided.", nameof(fileName));
            }

            try
            {
                var blobServiceClient = new BlobServiceClient(_connectionString);
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);

                await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                BlobClient blobClient = blobContainerClient.GetBlobClient(fileName);

                using (var stream = file.OpenReadStream())
                {
                    var uploadOptions = new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
                    };
                    await blobClient.UploadAsync(stream, uploadOptions);
                }

                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error uploading file '{fileName}' to Azure Blob Storage: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw new ApplicationException($"An error occurred while uploading the file: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Downloads a file from Azure Blob Storage.
        /// </summary>
        /// <param name="blobName">The name (including any path) of the blob to download.</param>
        /// <returns>A BlobDownloadInfo object containing the file stream, content type, and filename; or null if blob not found.</returns>
        public async Task<BlobDownloadInfo?> DownloadFileAsync(string blobName)
        {
            if (string.IsNullOrWhiteSpace(blobName))
            {
                throw new ArgumentException("Blob name cannot be null or empty.", nameof(blobName));
            }

            try
            {
                var blobServiceClient = new BlobServiceClient(_connectionString);
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);
                BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);

                if (await blobClient.ExistsAsync())
                {
                    // Download the blob's properties to get the content type
                    BlobProperties properties = await blobClient.GetPropertiesAsync();

                    // Download the blob's content to a memory stream
                    // This is suitable for reasonably sized files. For very large files,
                    // you might stream directly to the HTTP response in the controller.
                    var memoryStream = new MemoryStream();
                    await blobClient.DownloadToAsync(memoryStream);
                    memoryStream.Position = 0; // Reset stream position to the beginning for reading

                    return new BlobDownloadInfo
                    {
                        Content = memoryStream,
                        ContentType = properties.ContentType,
                        FileName = blobClient.Name // The blobName itself can be used as a suggestion
                    };
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Blob '{blobName}' not found in container '{_containerName}'.");
                    return null; // Or throw a FileNotFoundException
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error downloading file '{blobName}' from Azure Blob Storage: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                // Consider more specific exception handling or re-throwing a custom exception
                throw new ApplicationException($"An error occurred while downloading the file: {ex.Message}", ex);
            }
        }
    }
}
