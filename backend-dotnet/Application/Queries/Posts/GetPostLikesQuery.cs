using Application.DTOs;

namespace Application.Queries.Posts
{
    public class GetPostLikesQuery : IQuery<PagedResult<SimpleUserDto>>
    {
        public Guid PostId { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}