// It is responsible for interacting with Azure Blob Storage.Rather than having controllers or business logic communicate directly with Azure Storage, they call this service through the IBlobStorageService interface.
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Ecommerce.Contracts.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Ecommerce.BLL
{
    public class BlobStorageService : IBlobStorageService
    {
        //Connection of Azure Storage Account
        // Blob is file data stored in cloud with unique URI
        private readonly BlobServiceClient _blobServiceClient;

        public BlobStorageService(IConfiguration configuration)
        {
            // Try reading Connection String first (needed for Contributor role without role-assignment access)
            string connectionString = configuration["AzureStorage:ConnectionString"];
            if (!string.IsNullOrEmpty(connectionString))
            {
                _blobServiceClient = new BlobServiceClient(connectionString);
                return;
            }

            // Fallback: Read storage account endpoint from configuration (uses Managed Identity)
            string blobUri = configuration["AzureStorage:BlobServiceUri"];
            if (string.IsNullOrEmpty(blobUri))
            {
                throw new ArgumentNullException("AzureStorage:ConnectionString or AzureStorage:BlobServiceUri is missing.");
            }

            _blobServiceClient = new BlobServiceClient(new Uri(blobUri), new DefaultAzureCredential());
        }

        // containerName is the blob container name (similar to a folder).
        // contentType is the MIME type of the file (e.g., image/webp, image/png, application/pdf).
        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string containerName, string contentType)
        {
            // Gets a client object representing the specified container.
            // This does NOT create the container in Azure Storage.
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            // Creates the container only if it does not already exist.
            // PublicAccessType.Blob allows anonymous read access to blobs,
            // but prevents anonymous listing of the container.
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            // Gets a client object representing the target blob (file).
            // It simply represents the blob's path and does NOT create the blob.
            var blobClient = containerClient.GetBlobClient(fileName);

            // Configures upload options for the blob.
            // Here we set the HTTP Content-Type (MIME type) so browsers
            // and other clients know how to handle the uploaded file.
            // BlobUploadOptions can also configure metadata, tags,
            // access tier, transfer options, and encryption settings.
            var options = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                }
            };

            // Uploads the file stream to Azure Blob Storage.
            // If the blob does not exist, it is created at the path
            // represented by blobClient.
            await blobClient.UploadAsync(fileStream, options);

            // Returns the absolute URI of the uploaded blob.
            // This can be stored in the database for later retrieval.
            return blobClient.Uri.ToString();
        }

        public async Task DeleteFileAsync(string fileUrl, string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var uri = new Uri(fileUrl);
            var fileName = Path.GetFileName(uri.LocalPath);

            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
        }
    }
}
