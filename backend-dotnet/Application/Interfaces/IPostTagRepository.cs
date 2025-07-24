using Domain.Entities;

namespace Application.Interfaces
{
    public interface IPostTagRepository : IRepository<PostTag>
    {
        Task CreatePostTagAsync(Guid postId, Guid tagId);
        Task<List<Guid>> GetPostIdsByTagAsync(string tagName);
    }
}