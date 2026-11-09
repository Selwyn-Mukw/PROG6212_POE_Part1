using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Api.Services
{
    public class AzureBlobStorageService
    {
        private readonly BlobContainerClient _profileContainerClient;
        private readonly BlobContainerClient _eventBannerContainerClient;

        public AzureBlobStorageService(IConfiguration configuration)
        {
            string connectionString =
                configuration["AzureBlobStorage:ConnectionString"]
                ?? throw new InvalidOperationException(
                    "Azure Blob Storage connection string is missing.");

            string profileContainerName =
                configuration["AzureBlobStorage:ContainerName"]
                ?? throw new InvalidOperationException(
                    "Profile picture container name is missing.");

            string eventBannerContainerName =
                configuration["AzureBlobStorage:EventBannerContainerName"]
                ?? throw new InvalidOperationException(
                    "Event banner container name is missing.");

            var blobServiceClient =
                new BlobServiceClient(connectionString);

            _profileContainerClient =
                blobServiceClient.GetBlobContainerClient(
                    profileContainerName);

            _eventBannerContainerClient =
                blobServiceClient.GetBlobContainerClient(
                    eventBannerContainerName);
        }

        // PROFILE PICTURE METHODS

        public async Task<string> UploadAsync(IFormFile file)
        {
            string extension = Path.GetExtension(file.FileName);
            string blobName = $"{Guid.NewGuid()}{extension}";

            BlobClient blobClient =
                _profileContainerClient.GetBlobClient(blobName);

            using var stream = file.OpenReadStream();

            await blobClient.UploadAsync(
                stream,
                new BlobHttpHeaders
                {
                    ContentType = file.ContentType
                });

            return blobName;
        }

        public async Task<(byte[] Content, string ContentType)>
            DownloadAsync(string blobName)
        {
            BlobClient blobClient =
                _profileContainerClient.GetBlobClient(blobName);

            var response = await blobClient.DownloadContentAsync();

            byte[] content = response.Value.Content.ToArray();

            string contentType =
                response.Value.Details.ContentType
                ?? "application/octet-stream";

            return (content, contentType);
        }

        public async Task DeleteAsync(string blobName)
        {
            if (string.IsNullOrWhiteSpace(blobName))
                return;

            BlobClient blobClient =
                _profileContainerClient.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync();
        }

        // EVENT BANNER METHODS

        public async Task<string> UploadEventBannerAsync(IFormFile file)
        {
            string extension = Path.GetExtension(file.FileName);
            string blobName = $"{Guid.NewGuid()}{extension}";

            BlobClient blobClient =
                _eventBannerContainerClient.GetBlobClient(blobName);

            using var stream = file.OpenReadStream();

            await blobClient.UploadAsync(
                stream,
                new BlobHttpHeaders
                {
                    ContentType = file.ContentType
                });

            return blobName;
        }

        public async Task<(byte[] Content, string ContentType)>
            DownloadEventBannerAsync(string blobName)
        {
            BlobClient blobClient =
                _eventBannerContainerClient.GetBlobClient(blobName);

            var response = await blobClient.DownloadContentAsync();

            byte[] content = response.Value.Content.ToArray();

            string contentType =
                response.Value.Details.ContentType
                ?? "application/octet-stream";

            return (content, contentType);
        }

        public async Task DeleteEventBannerAsync(string blobName)
        {
            if (string.IsNullOrWhiteSpace(blobName))
                return;

            BlobClient blobClient =
                _eventBannerContainerClient.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync();
        }
    }
}