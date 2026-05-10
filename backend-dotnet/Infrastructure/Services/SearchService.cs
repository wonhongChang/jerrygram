using Application.Common;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class SearchService : ISearchService
    {
        private readonly IPostRepository _postRepository;
        private readonly IUserFollowRepository _userFollowRepository;
        private readonly IPostTagRepository _postTagRepository;
        private readonly IElasticService _elastic;
        private readonly ICacheService _cacheService;
        private readonly ILogger<SearchService> _logger;

        public SearchService(
            IPostRepository postRepository,
            IUserFollowRepository userFollowRepository,
            IPostTagRepository postTagRepository,
            IElasticService elastic,
            ICacheService cacheService,
            ILogger<SearchService> logger)
        {
            _postRepository = postRepository;
            _userFollowRepository = userFollowRepository;
            _postTagRepository = postTagRepository;
            _elastic = elastic;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<object> SearchAsync(string query, string? userIdStr)
        {
            Guid? userId = Guid.TryParse(userIdStr, out var parsedUserId) ? parsedUserId : null;
            List<Guid> followingIds = [];

            if (userId != null)
            {
                followingIds = await _userFollowRepository.GetFollowingIdsAsync(userId.Value);
            }

            if (query.StartsWith('#'))
            {
                var tag = query.TrimStart('#').ToLowerInvariant();

                var postIds = await _postTagRepository.GetPostIdsByTagAsync(tag);
                var posts = await _postRepository.GetPostsByIdsAsync(postIds);

                var filtered = posts.Where(p => CanViewPost(p.Visibility, p.UserId, userId, followingIds));

                return new
                {
                    query,
                    users = Array.Empty<object>(),
                    hashtags = new[] { tag },
                    posts = filtered.Select(p => new
                    {
                        p.Id,
                        p.Caption,
                        p.ImageUrl,
                        p.CreatedAt,
                        likes = p.Likes.Count,
                        liked = userId != null && p.Likes.Any(l => l.UserId == userId),
                        user = new
                        {
                            p.User.Id,
                            p.User.Username,
                            p.User.ProfileImageUrl
                        }
                    })
                };
            }
            else
            {
                var posts = await _elastic.SearchPostsAsync(query);
                var users = await _elastic.SearchUsersAsync(query);
                var tags = await _elastic.SearchTagsAsync(query.ToLowerInvariant());

                var filtered = posts.Where(p => CanViewPost(p.Visibility, p.UserId, userId, followingIds));

                return new
                {
                    query,
                    hashtags = tags.Select(t => t.Name),
                    users = users.Select(u => new
                    {
                        u.Id,
                        u.Username,
                        u.ProfileImageUrl
                    }),
                    posts = filtered.Select(p => new
                    {
                        p.Id,
                        p.Caption,
                        p.ImageUrl,
                        p.CreatedAt,
                        likes = 0,
                        liked = false,
                        user = new
                        {
                            Id = p.UserId,
                            p.Username
                        }
                    })
                };
            }
        }

        public async Task<object> AutocompleteAsync(string query)
        {
            var normalizedQuery = query.Trim().ToLower();
            var cacheKey = $"autocomplete:{normalizedQuery}";

            var cached = await _cacheService.GetAsync<object>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("Autocomplete cache hit for query: {Query}", query);
                return cached;
            }

            _logger.LogDebug("Autocomplete cache miss for query: {Query}", query);

            var result = await GetAutocompleteFromElasticsearch(query);

            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

            _logger.LogInformation("Autocomplete result cached for query: {Query}", query);
            return result;
        }

        private async Task<object> GetAutocompleteFromElasticsearch(string query)
        {
            query = query.Trim();

            if (query.StartsWith('#'))
            {
                var keyword = query.TrimStart('#').ToLowerInvariant();
                var tags = await _elastic.SearchTagsAsync(keyword, 3);
                var users = await _elastic.SearchUsersAsync(keyword, 10);

                if (tags.Count != 0)
                {
                    return new
                    {
                        query,
                        mode = "tag",
                        posts = Array.Empty<object>(),
                        hashtags = tags.Select(t => t.Name),
                        tags = tags.Select(t => t.Name),
                        users = users.Select(u => new
                        {
                            u.Id,
                            u.Username,
                            u.ProfileImageUrl
                        }),
                        cached_at = DateTime.UtcNow
                    };
                }
                else
                {
                    return new
                    {
                        query,
                        mode = "tag",
                        posts = Array.Empty<object>(),
                        hashtags = Array.Empty<string>(),
                        tags = Array.Empty<string>(),
                        users = Array.Empty<string>(),
                        fallback = $"Search for \"{query}\"",
                        cached_at = DateTime.UtcNow
                    };
                }
            }

            var tagResults = await _elastic.SearchTagsAsync(query.ToLowerInvariant(), 3);
            var userResults = await _elastic.SearchUsersAsync(query, 10);

            return new
            {
                query,
                mode = "default",
                posts = Array.Empty<object>(),
                hashtags = tagResults.Select(t => t.Name),
                tags = tagResults.Select(t => t.Name),
                users = userResults.Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.ProfileImageUrl
                }),
                cached_at = DateTime.UtcNow
            };
        }

        private static bool CanViewPost(
            PostVisibility visibility,
            Guid ownerId,
            Guid? currentUserId,
            IReadOnlyCollection<Guid> followingIds)
        {
            if (visibility == PostVisibility.Public)
                return true;

            if (!currentUserId.HasValue)
                return false;

            if (ownerId == currentUserId.Value)
                return true;

            return visibility == PostVisibility.FollowersOnly && followingIds.Contains(ownerId);
        }
    }
}
