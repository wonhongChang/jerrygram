using Application.Common;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Posts
{
    public class CreatePostCommandHandler : ICommandHandler<CreatePostCommand, PostListItemDto>
    {
        private readonly IPostRepository _postRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IPostTagRepository _postTagRepository;
        private readonly IBlobService _blobService;
        private readonly IElasticService _elastic;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CreatePostCommandHandler> _logger;

        public CreatePostCommandHandler(
            IPostRepository postRepository,
            IUserRepository userRepository,
            ITagRepository tagRepository,
            IPostTagRepository postTagRepository,
            IBlobService blobService,
            IElasticService elastic,
            ICacheService cacheService,
            ILogger<CreatePostCommandHandler> logger)
        {
            _postRepository = postRepository;
            _userRepository = userRepository;
            _tagRepository = tagRepository;
            _postTagRepository = postTagRepository;
            _blobService = blobService;
            _elastic = elastic;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<PostListItemDto> HandleAsync(CreatePostCommand command)
        {
            _logger.LogInformation("Creating post for user {UserId}", command.UserId);

            string? imageUrl = null;
            if (command.Dto.Image != null && command.Dto.Image.Length > 0)
            {
                imageUrl = await _blobService.UploadAsync(command.Dto.Image.OpenReadStream(), command.Dto.Image.FileName, BlobContainers.Post);
                _logger.LogInformation("Image uploaded successfully");
            }

            var post = new Post
            {
                Id = Guid.NewGuid(),
                Caption = command.Dto.Caption,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow,
                UserId = command.UserId,
                Visibility = command.Dto.Visibility
            };

            _postRepository.Add(post);

            // Handle hashtags
            await ProcessHashtagsAsync(post, command.Dto.Caption ?? string.Empty);

            await _postRepository.SaveChangesAsync();

            // Index for Elasticsearch
            await IndexPostAsync(post);

            // Clear relevant caches
            _cacheService.RemoveByPattern("public_posts");
            _cacheService.RemoveByPattern($"user_feed_{command.UserId}");

            _logger.LogInformation("Post {PostId} created successfully", post.Id);

            return await BuildPostListItemDto(post);
        }

        private async Task ProcessHashtagsAsync(Post post, string caption)
        {
            var postCaption = PostCaption.Create(caption);
            if (!postCaption.HasHashtags) return;

            var existingTags = await _tagRepository.GetExistingTagsAsync(postCaption.Hashtags);

            foreach (var tagName in postCaption.Hashtags)
            {
                if (!existingTags.TryGetValue(tagName, out var tag))
                {
                    tag = await _tagRepository.CreateTagAsync(tagName);
                    existingTags[tagName] = tag;
                }

                await _postTagRepository.CreatePostTagAsync(post.Id, tag.Id);

                await _elastic.IndexTagAsync(new TagIndex
                {
                    Id = tag.Id.ToString(),
                    Name = tag.Name
                });
            }
        }

        private async Task IndexPostAsync(Post post)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(post.UserId);
                if (user != null)
                {
                    await _elastic.IndexPostAsync(new PostIndex
                    {
                        Id = post.Id,
                        Caption = post.Caption ?? string.Empty,
                        CreatedAt = post.CreatedAt,
                        Username = user.Username
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to index post {PostId}", post.Id);
                // Don't throw - indexing failure shouldn't fail post creation
            }
        }

        private async Task<PostListItemDto> BuildPostListItemDto(Post post)
        {
            var user = await _userRepository.GetByIdAsync(post.UserId);
            
            return new PostListItemDto
            {
                Id = post.Id,
                Caption = post.Caption,
                ImageUrl = post.ImageUrl,
                CreatedAt = post.CreatedAt,
                Likes = 0, // New post has no likes
                Liked = false,
                User = user != null ? new SimpleUserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    ProfileImageUrl = user.ProfileImageUrl
                } : null!
            };
        }

    }
}