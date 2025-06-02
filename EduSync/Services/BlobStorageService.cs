// In Services/BlobStorageService.cs (Backend Project)
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration; // Required for IConfiguration
using System;
using System.IO;
using System.Threading.Tasks;

namespace EduSync.Services // Or your project's namespace
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly string _connectionString;
        private readonly string _containerName;
        // private readonly IConfiguration _configuration; // Not strictly needed if only accessing specific keys

        public BlobStorageService(IConfiguration configuration)
        {
            // _configuration = configuration; // Store if needed for other things
            _connectionString = configuration["AzureBlobStorage:ConnectionString"];
            _containerName = configuration["AzureBlobStorage:ContainerName"];

            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Azure Blob Storage ConnectionString is not configured. Check configuration section 'AzureBlobStorage:ConnectionString'.");
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
                        // Overwrite is true by default for UploadAsync with BlobUploadOptions 
                        // unless specific BlobRequestConditions are set to prevent it.
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
    }
}
