namespace Application.Queries.Users
{
    public class GetFollowersQuery : IQuery<object>
    {
        public Guid UserId { get; set; }
    }
}