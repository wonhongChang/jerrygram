namespace Application.Interfaces
{
    public interface IBlobService
    {
        Task<string> UploadAsync(Stream fileStream, string fileName, string containerName);
        Task DeleteAsync(string blobUrl);
        Task DeleteAsync(string blobUrl, string containerName);
    }
}