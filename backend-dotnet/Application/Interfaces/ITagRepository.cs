using Domain.Entities;

namespace Application.Interfaces
{
    public interface ITagRepository : IRepository<Tag>
    {
        Task<Dictionary<string, Tag>> GetExistingTagsAsync(IEnumerable<string> tagNames);
        Task<Dictionary<string, Tag>> GetTagsByNamesAsync(IEnumerable<string> tagNames);
        Task<Tag> CreateTagAsync(string tagName);
    }
}