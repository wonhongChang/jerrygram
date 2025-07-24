using Application.DTOs;
using Application.Interfaces;

namespace Application.Queries.Posts
{
    public class GetUserFeedQueryHandler : IQueryHandler<GetUserFeedQuery, PagedResult<PostListItemDto>>
    {
        private readonly IPostRepository _postRepository;
        private readonly IUserFollowRepository _userFollowRepository;

        public GetUserFeedQueryHandler(
            IPostRepository postRepository,
            IUserFollowRepository userFollowRepository)
        {
            _postRepository = postRepository;
            _userFollowRepository = userFollowRepository;
        }

        public async Task<PagedResult<PostListItemDto>> HandleAsync(GetUserFeedQuery query)
        {
            var followingIds = await _userFollowRepository.GetFollowingIdsAsync(query.UserId);

            return await _postRepository.GetUserFeedAsync(followingIds, query.UserId, query.Page, query.PageSize);
        }
    }
}