using Application.DTOs;
using Application.Interfaces;

namespace Application.Queries.Posts
{
    public class GetExplorePostsQueryHandler : IQueryHandler<GetExplorePostsQuery, List<PostListItemDto>>
    {
        private readonly IPostRepository _postRepository;
        private readonly IRecommendClient _recommendClient;

        public GetExplorePostsQueryHandler(
            IPostRepository postRepository,
            IRecommendClient recommendClient)
        {
            _postRepository = postRepository;
            _recommendClient = recommendClient;
        }

        public async Task<List<PostListItemDto>> HandleAsync(GetExplorePostsQuery query)
        {
            if (query.UserId == null)
            {
                return await _postRepository.GetPopularPostsAsync();
            }

            var recommended = await _recommendClient.GetRecommendationsAsync(query.UserId.Value);

            if (recommended.Count > 0)
                return recommended;

            return await _postRepository.GetPopularPostsNotFollowedAsync(query.UserId.Value);
        }
    }
}