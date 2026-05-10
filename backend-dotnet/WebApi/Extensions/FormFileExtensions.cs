using Application.Common;
using Microsoft.AspNetCore.Http;

namespace WebApi.Extensions
{
    public static class FormFileExtensions
    {
        public static UploadFile? ToUploadFile(this IFormFile? file)
        {
            if (file == null)
                return null;

            return new UploadFile(
                file.FileName,
                file.ContentType,
                file.Length,
                file.OpenReadStream);
        }
    }
}
