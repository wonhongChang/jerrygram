namespace Application.Queries.Users
{
    public class GetFollowingsQuery : IQuery<object>
    {
        public Guid UserId { get; set; }
    }
}