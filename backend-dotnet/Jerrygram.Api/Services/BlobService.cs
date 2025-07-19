using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Jerrygram.Api.Services
{
    public class BlobService
    {
        private readonly string _connectionString;
        private readonly IConfiguration _config;

        public BlobService(IConfiguration config)
        {
            _config = config;
            _connectionString = config["AzureBlobStorage:ConnectionString"]!;
        }

        private BlobContainerClient GetContainer(string containerKey)
        {
            var containerName = _config[$"AzureBlobStorage:{containerKey}"];
            var container = new BlobContainerClient(_connectionString, containerName);
            container.CreateIfNotExists(PublicAccessType.Blob);
            return container;
        }

        public async Task<string> UploadAsync(IFormFile file, string containerKey)
        {
            var container = GetContainer(containerKey);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var blob = container.GetBlobClient(fileName);

            using var stream = file.OpenReadStream();
            await blob.UploadAsync(stream, overwrite: true);

            return blob.Uri.ToString();
        }

        public async Task DeleteAsync(string fileUrl, string containerKey)
        {
            var container = GetContainer(containerKey);

            var uri = new Uri(fileUrl);
            var blobName = Path.GetFileName(uri.LocalPath);
            var blob = container.GetBlobClient(blobName);
            await blob.DeleteIfExistsAsync();
        }
    }
}
