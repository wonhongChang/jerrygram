using Application.DTOs;

namespace Application.Commands.Comments
{
    public class CreateCommentCommand : ICommand<CommentResponseDto>
    {
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public CreateCommentDto Dto { get; set; } = null!;
    }
}