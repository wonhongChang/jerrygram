using FluentValidation;
using Application.DTOs;

namespace WebApi.Validators
{
    public class CreateCommentDtoValidator : AbstractValidator<CreateCommentDto>
    {
        public CreateCommentDtoValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Comment content is required")
                .MaximumLength(500).WithMessage("Comment cannot exceed 500 characters")
                .MinimumLength(1).WithMessage("Comment must have at least 1 character");
        }
    }
}