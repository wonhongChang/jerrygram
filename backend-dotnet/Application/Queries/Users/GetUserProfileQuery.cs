namespace Application.Queries.Users
{
    public class GetUserProfileQuery : IQuery<object>
    {
        public string Username { get; set; } = null!;
    }
}