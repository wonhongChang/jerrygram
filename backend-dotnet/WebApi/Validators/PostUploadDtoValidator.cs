using FluentValidation;
using Application.DTOs;

namespace WebApi.Validators
{
    public class PostUploadDtoValidator : AbstractValidator<PostUploadDto>
    {
        private readonly string[] _allowedImageTypes = { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        private const long MaxImageSize = 10 * 1024 * 1024; // 10MB

        public PostUploadDtoValidator()
        {
            RuleFor(x => x.Caption)
                .MaximumLength(2200).WithMessage("Caption cannot exceed 2200 characters");

            When(x => x.Image != null, () =>
            {
                RuleFor(x => x.Image!.Length)
                    .LessThanOrEqualTo(MaxImageSize)
                    .WithMessage($"Image size cannot exceed {MaxImageSize / (1024 * 1024)}MB");

                RuleFor(x => x.Image!.ContentType)
                    .Must(contentType => _allowedImageTypes.Contains(contentType?.ToLower()))
                    .WithMessage("Only JPEG, PNG, GIF, and WebP images are allowed");
            });

            // At least one of caption or image must be provided
            RuleFor(x => x)
                .Must(x => !string.IsNullOrWhiteSpace(x.Caption) || (x.Image != null && x.Image.Length > 0))
                .WithMessage("Either caption or image must be provided");
        }
    }
}