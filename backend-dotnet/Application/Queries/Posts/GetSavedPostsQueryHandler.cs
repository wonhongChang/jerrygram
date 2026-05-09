using Application.DTOs;
using Application.Interfaces;

namespace Application.Queries.Posts
{
    public class GetSavedPostsQueryHandler : IQueryHandler<GetSavedPostsQuery, PagedResult<PostListItemDto>>
    {
        private readonly IPostSaveRepository _postSaveRepository;
        private readonly ICacheService _cacheService;

        public GetSavedPostsQueryHandler(
            IPostSaveRepository postSaveRepository,
            ICacheService cacheService)
        {
            _postSaveRepository = postSaveRepository;
            _cacheService = cacheService;
        }

        public async Task<PagedResult<PostListItemDto>> HandleAsync(GetSavedPostsQuery query)
        {
            var cacheKey = $"saved_posts_{query.UserId}_page_{query.Page}_{query.PageSize}";
            var cached = await _cacheService.GetAsync<PagedResult<PostListItemDto>>(cacheKey);
            if (cached != null)
                return cached;

            var result = await _postSaveRepository.GetSavedPostsAsync(query.UserId, query.Page, query.PageSize);
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(15));

            return result;
        }
    }
}
