using Application.DTOs;
using Domain.Entities;

namespace Application.Commands.Posts
{
    public class CreatePostCommand : ICommand<PostListItemDto>
    {
        public PostUploadDto Dto { get; set; } = null!;
        public Guid UserId { get; set; }
    }
}