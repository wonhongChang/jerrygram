using Application.DTOs;
using Domain.Entities;

namespace Application.Commands.Posts
{
    public class UpdatePostCommand : ICommand<Post>
    {
        public Guid PostId { get; set; }
        public UpdatePostDto Dto { get; set; } = null!;
        public Guid UserId { get; set; }
    }
}