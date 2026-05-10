namespace Application.Common
{
    public sealed class UploadFile
    {
        private readonly Func<Stream> _openReadStream;

        public UploadFile(string fileName, string contentType, long length, Func<Stream> openReadStream)
        {
            FileName = fileName;
            ContentType = contentType;
            Length = length;
            _openReadStream = openReadStream;
        }

        public string FileName { get; }
        public string ContentType { get; }
        public long Length { get; }

        public Stream OpenReadStream()
        {
            return _openReadStream();
        }
    }
}
