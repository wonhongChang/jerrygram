using Application.DTOs;

namespace Application.Queries.Posts
{
    public class GetSavedPostsQuery : IQuery<PagedResult<PostListItemDto>>
    {
        public Guid UserId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
