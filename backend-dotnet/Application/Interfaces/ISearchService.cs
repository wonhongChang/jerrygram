namespace Application.Interfaces
{
    public interface ISearchService
    {
        Task<object> SearchAsync(string query, string? userIdStr);
        Task<object> AutocompleteAsync(string query);
    }
}