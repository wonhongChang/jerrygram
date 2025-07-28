using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services
{
    public class BlobService : IBlobService
    {
        private readonly string _connectionString;

        public BlobService(IConfiguration config)
        {
            // SECURITY: Use environment variable for production
            _connectionString = config["AzureBlobStorage:ConnectionString"] 
                ?? throw new InvalidOperationException("Azure Blob Storage connection string must be configured via environment variable 'AzureBlobStorage__ConnectionString'");
        }

        private BlobContainerClient GetContainer(string containerName)
        {
            var container = new BlobContainerClient(_connectionString, containerName);
            container.CreateIfNotExists(PublicAccessType.Blob);
            return container;
        }

        public async Task<string> UploadAsync(Stream fileStream, string fileName, string containerName)
        {
            var container = GetContainer(containerName);

            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
            var blob = container.GetBlobClient(uniqueFileName);

            await blob.UploadAsync(fileStream, overwrite: true);

            return blob.Uri.ToString();
        }

        public async Task DeleteAsync(string blobUrl)
        {
            var uri = new Uri(blobUrl);
            var containerName = uri.Segments[1].TrimEnd('/');
            var blobName = Path.GetFileName(uri.LocalPath);
            
            var container = GetContainer(containerName);
            var blob = container.GetBlobClient(blobName);
            await blob.DeleteIfExistsAsync();
        }

        public async Task DeleteAsync(string blobUrl, string containerName)
        {
            var uri = new Uri(blobUrl);
            var blobName = Path.GetFileName(uri.LocalPath);
            
            var container = GetContainer(containerName);
            var blob = container.GetBlobClient(blobName);
            await blob.DeleteIfExistsAsync();
        }
    }
}