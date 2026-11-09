using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Api.Services
{
    public class AzureBlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public AzureBlobStorageService(IConfiguration configuration)
        {
            string connectionString =
                configuration["AzureBlobStorage:ConnectionString"]
                ?? throw new InvalidOperationException(
                    "Azure Blob Storage connection string is missing.");

            string containerName =
                configuration["AzureBlobStorage:ContainerName"]
                ?? throw new InvalidOperationException(
                    "Azure Blob Storage container name is missing.");

            var blobServiceClient =
                new BlobServiceClient(connectionString);

            _containerClient =
                blobServiceClient.GetBlobContainerClient(containerName);
        }

        // Upload profile picture
        public async Task<string> UploadAsync(IFormFile file)
        {
            string extension = Path.GetExtension(file.FileName);

            string blobName = $"{Guid.NewGuid()}{extension}";

            BlobClient blobClient =
                _containerClient.GetBlobClient(blobName);

            using var stream = file.OpenReadStream();

            await blobClient.UploadAsync(
                stream,
                new BlobHttpHeaders
                {
                    ContentType = file.ContentType
                });

            return blobName;
        }

        // Download profile picture
        public async Task<(byte[] Content, string ContentType)>
            DownloadAsync(string blobName)
        {
            BlobClient blobClient =
                _containerClient.GetBlobClient(blobName);

            var response = await blobClient.DownloadContentAsync();

            byte[] content = response.Value.Content.ToArray();

            string contentType =
                response.Value.Details.ContentType
                ?? "application/octet-stream";

            return (content, contentType);
        }
        // Delete profile picture
        public async Task DeleteAsync(string blobName)
        {
            if (string.IsNullOrWhiteSpace(blobName))
                return;

            BlobClient blobClient =
                _containerClient.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync();
        }
    }
}