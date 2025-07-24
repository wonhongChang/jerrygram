using Application.DTOs;

namespace Application.Queries.Posts
{
    public class GetPostByIdQuery : IQuery<PostListItemDto>
    {
        public Guid PostId { get; set; }
        public Guid? CurrentUserId { get; set; }
    }
}