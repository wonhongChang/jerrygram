using Application.DTOs;
using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Queries.Posts
{
    public class GetPublicPostsQueryHandler : IQueryHandler<GetPublicPostsQuery, PagedResult<PostListItemDto>>
    {
        private readonly IPostRepository _postRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<GetPublicPostsQueryHandler> _logger;

        public GetPublicPostsQueryHandler(
            IPostRepository postRepository,
            ICacheService cacheService,
            ILogger<GetPublicPostsQueryHandler> logger)
        {
            _postRepository = postRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<PagedResult<PostListItemDto>> HandleAsync(GetPublicPostsQuery query)
        {
            var cacheKey = $"public_posts_page_{query.Page}_{query.PageSize}_{query.CurrentUserId}";
            
            var cached = await _cacheService.GetAsync<PagedResult<PostListItemDto>>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("Public posts page {Page} retrieved from cache", query.Page);
                return cached;
            }

            var result = await _postRepository.GetPublicPostsAsync(query.CurrentUserId, query.Page, query.PageSize);

            // Cache for 15 minutes
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(15));

            _logger.LogInformation("Retrieved {Count} public posts for page {Page}", result.Items.Count, query.Page);
            return result;
        }
    }
}