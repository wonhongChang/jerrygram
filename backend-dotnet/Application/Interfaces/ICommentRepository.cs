using Domain.Entities;

namespace Application.Interfaces
{
    public interface ICommentRepository : IRepository<Comment>
    {
        Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(Guid postId);
        Task<object> GetCommentsWithUsersByPostIdAsync(Guid postId);
    }
}