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

        public BlobStorageService(IConfiguration configuration)
        {
            // Retrieve connection string using GetConnectionString for "MyBlobStorage"
            _connectionString = configuration["MyBlobStorage:ConnectionString"];

            // Retrieve container name from configuration (e.g., appsettings or App Service application settings)
            // Assumes a structure like: "AzureBlobStorage": { "ContainerName": "coursemedia" }
            // Or in App Service Application Settings: AzureBlobStorage__ContainerName
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
    }
}
