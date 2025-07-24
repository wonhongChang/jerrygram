using Persistence.Data;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories
{
    public class PostTagRepository : Repository<PostTag>, IPostTagRepository
    {
        public PostTagRepository(AppDbContext context) : base(context)
        {
        }

        public async Task CreatePostTagAsync(Guid postId, Guid tagId)
        {
            var postTag = new PostTag
            {
                PostId = postId,
                TagId = tagId
            };
            
            Add(postTag);
            await SaveChangesAsync();
        }

        public async Task<List<Guid>> GetPostIdsByTagAsync(string tagName)
        {
            return await _context.PostTags
                .Include(pt => pt.Tag)
                .Where(pt => pt.Tag.Name == tagName)
                .Select(pt => pt.PostId)
                .ToListAsync();
        }
    }
}