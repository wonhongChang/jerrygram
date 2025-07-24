using Application.Common;

namespace Application.Interfaces
{
    public interface IElasticService
    {
        Task IndexUserAsync(UserIndex userIndex);
        Task IndexPostAsync(PostIndex postIndex);
        Task IndexTagAsync(TagIndex tagIndex);
        Task UpdateUserAsync(UserIndex userIndex);
        Task UpdatePostAsync(PostIndex postIndex);
        Task DeletePostAsync(string postId);
        Task<List<PostIndex>> SearchPostsAsync(string query);
        Task<List<UserIndex>> SearchUsersAsync(string query, int size = 10);
        Task<List<TagIndex>> SearchTagsAsync(string query, int size = 3);
    }
}