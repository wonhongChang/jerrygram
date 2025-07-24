using Application.DTOs;

namespace Application.Queries.Posts
{
    public class GetPublicPostsQuery : IQuery<PagedResult<PostListItemDto>>
    {
        public Guid? CurrentUserId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}