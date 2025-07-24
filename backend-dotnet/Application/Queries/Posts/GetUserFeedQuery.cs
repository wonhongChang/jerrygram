using Application.DTOs;

namespace Application.Queries.Posts
{
    public class GetUserFeedQuery : IQuery<PagedResult<PostListItemDto>>
    {
        public Guid UserId { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}