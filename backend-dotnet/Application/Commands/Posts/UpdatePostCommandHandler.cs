using Application.Common;
using Application.Interfaces;
using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Posts
{
    public class UpdatePostCommandHandler : ICommandHandler<UpdatePostCommand, Post>
    {
        private readonly IPostRepository _postRepository;
        private readonly IPostTagRepository _postTagRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IBlobService _blobService;
        private readonly IElasticService _elastic;
        private readonly ICacheService _cacheService;
        private readonly ILogger<UpdatePostCommandHandler> _logger;

        public UpdatePostCommandHandler(
            IPostRepository postRepository,
            IPostTagRepository postTagRepository,
            ITagRepository tagRepository,
            IBlobService blobService,
            IElasticService elastic,
            ICacheService cacheService,
            ILogger<UpdatePostCommandHandler> logger)
        {
            _postRepository = postRepository;
            _postTagRepository = postTagRepository;
            _tagRepository = tagRepository;
            _blobService = blobService;
            _elastic = elastic;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<Post> HandleAsync(UpdatePostCommand command)
        {
            var post = await _postRepository.GetPostWithUserAndTagsAsync(command.PostId);

            if (post == null)
                throw new KeyNotFoundException("Post not found.");

            if (post.UserId != command.UserId)
                throw new UnauthorizedAccessException("You are not the owner of this post.");

            var oldTags = post.PostTags.Select(pt => pt.Tag).ToList();

            bool captionChanged = false;
            if (command.Dto.Caption != null && command.Dto.Caption != post.Caption)
            {
                post.Caption = string.IsNullOrWhiteSpace(command.Dto.Caption) ? null : command.Dto.Caption;
                captionChanged = true;
            }

            if (command.Dto.Visibility != null)
                post.Visibility = command.Dto.Visibility.Value;

            if (command.Dto.Image != null && command.Dto.Image.Length > 0)
            {
                if (!string.IsNullOrEmpty(post.ImageUrl))
                    await _blobService.DeleteAsync(post.ImageUrl, BlobContainers.Post);

                post.ImageUrl = await _blobService.UploadAsync(command.Dto.Image.OpenReadStream(), command.Dto.Image.FileName, BlobContainers.Post);
            }

            if (captionChanged)
            {
                var postCaption = PostCaption.Create(post.Caption);
                var newTagSet = new HashSet<string>(postCaption.Hashtags);
                var oldTagSet = new HashSet<string>(oldTags.Select(t => t.Name));

                foreach (var old in oldTags)
                {
                    if (!newTagSet.Contains(old.Name))
                    {
                        var link = post.PostTags.FirstOrDefault(pt => pt.TagId == old.Id);
                        if (link != null)
                            _postTagRepository.Remove(link);
                    }
                }

                var existingTags = await _tagRepository.GetTagsByNamesAsync(postCaption.Hashtags);

                foreach (var tagName in postCaption.Hashtags)
                {
                    if (!oldTagSet.Contains(tagName))
                    {
                        if (!existingTags.TryGetValue(tagName, out var tag))
                        {
                            tag = new Tag { Id = Guid.NewGuid(), Name = tagName };
                            _tagRepository.Add(tag);
                        }

                        _postTagRepository.Add(new PostTag
                        {
                            PostId = post.Id,
                            TagId = tag.Id
                        });

                        await _elastic.IndexTagAsync(new TagIndex
                        {
                            Id = tag.Id.ToString(),
                            Name = tag.Name
                        });
                    }
                }
            }

            await _postRepository.SaveChangesAsync();

            try
            {
                await _elastic.IndexPostAsync(new PostIndex
                {
                    Id = post.Id,
                    UserId = post.UserId,
                    Caption = post.Caption ?? string.Empty,
                    Tags = post.Hashtags.ToList(),
                    ImageUrl = post.ImageUrl,
                    Username = post.User.Username,
                    CreatedAt = post.CreatedAt,
                    Visibility = post.Visibility
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to index updated post {PostId}", post.Id);
            }

            _cacheService.RemoveByPattern($"post_details_{post.Id}");
            _cacheService.RemoveByPattern("public_posts");
            _cacheService.RemoveByPattern("user_feed");
            _cacheService.RemoveByPattern("explore_posts");
            _cacheService.RemoveByPattern("saved_posts");

            return post;
        }
    }
}
