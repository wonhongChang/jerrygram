using Application.Common;
using Application.Interfaces;
using Domain.Entities;
using Domain.ValueObjects;

namespace Application.Commands.Posts
{
    public class UpdatePostCommandHandler : ICommandHandler<UpdatePostCommand, Post>
    {
        private readonly IPostRepository _postRepository;
        private readonly IPostTagRepository _postTagRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IBlobService _blobService;
        private readonly IElasticService _elastic;

        public UpdatePostCommandHandler(
            IPostRepository postRepository,
            IPostTagRepository postTagRepository,
            ITagRepository tagRepository,
            IBlobService blobService,
            IElasticService elastic)
        {
            _postRepository = postRepository;
            _postTagRepository = postTagRepository;
            _tagRepository = tagRepository;
            _blobService = blobService;
            _elastic = elastic;
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
            if (!string.IsNullOrWhiteSpace(command.Dto.Caption) && command.Dto.Caption != post.Caption)
            {
                post.Caption = command.Dto.Caption;
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

            await _elastic.IndexPostAsync(new PostIndex
            {
                Id = post.Id,
                Caption = post.Caption ?? string.Empty,
                UserId = post.User.Id,
                Username = post.User.Username,
                CreatedAt = post.CreatedAt,
                Visibility = post.Visibility
            });

            return post;
        }
    }
}