using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Queries.Posts
{
    public class GetPostByIdQueryHandler : IQueryHandler<GetPostByIdQuery, PostListItemDto>
    {
        private readonly IPostRepository _postRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<GetPostByIdQueryHandler> _logger;

        public GetPostByIdQueryHandler(
            IPostRepository postRepository,
            ICacheService cacheService,
            ILogger<GetPostByIdQueryHandler> logger)
        {
            _postRepository = postRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<PostListItemDto> HandleAsync(GetPostByIdQuery query)
        {
            var cacheKey = $"post_details_{query.PostId}_{query.CurrentUserId}";
            
            var cached = await _cacheService.GetAsync<PostListItemDto>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("Post {PostId} retrieved from cache", query.PostId);
                return cached;
            }

            // Check if post exists and get visibility permissions in one call
            var result = await _postRepository.GetPostDtoAsync(query.PostId, query.CurrentUserId);
            
            if (result == null)
            {
                throw new KeyNotFoundException("Post not found");
            }

            // Get the post entity to check visibility rules
            var post = await _postRepository.GetPostWithUserAndLikesAsync(query.PostId);
            
            if (post == null)
            {
                throw new KeyNotFoundException("Post not found");
            }

            // Check visibility permissions
            if (post.Visibility == PostVisibility.Private && post.UserId != query.CurrentUserId)
            {
                throw new UnauthorizedAccessException("You don't have permission to view this post");
            }

            if (post.Visibility == PostVisibility.FollowersOnly && query.CurrentUserId.HasValue && post.UserId != query.CurrentUserId)
            {
                var isFollowing = await _postRepository.IsFollowerAsync(query.CurrentUserId.Value, post.UserId);
                
                if (!isFollowing)
                {
                    throw new UnauthorizedAccessException("You don't have permission to view this post");
                }
            }

            // Cache for 30 minutes
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30));

            _logger.LogInformation("Post {PostId} retrieved successfully", query.PostId);
            return result;
        }
    }
}