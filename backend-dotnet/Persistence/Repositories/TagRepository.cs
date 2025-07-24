using Persistence.Data;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories
{
    public class TagRepository : Repository<Tag>, ITagRepository
    {
        public TagRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Dictionary<string, Tag>> GetExistingTagsAsync(IEnumerable<string> tagNames)
        {
            return await _dbSet
                .Where(t => tagNames.Contains(t.Name))
                .ToDictionaryAsync(t => t.Name);
        }

        public async Task<Dictionary<string, Tag>> GetTagsByNamesAsync(IEnumerable<string> tagNames)
        {
            return await _dbSet
                .Where(t => tagNames.Contains(t.Name))
                .ToDictionaryAsync(t => t.Name);
        }

        public async Task<Tag> CreateTagAsync(string tagName)
        {
            var tag = new Tag 
            { 
                Id = Guid.NewGuid(), 
                Name = tagName 
            };
            
            Add(tag);
            return tag;
        }
    }
}