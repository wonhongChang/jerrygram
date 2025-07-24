using Application.DTOs;

namespace Application.Queries.Posts
{
    public class GetExplorePostsQuery : IQuery<List<PostListItemDto>>
    {
        public Guid? UserId { get; set; }
    }
}