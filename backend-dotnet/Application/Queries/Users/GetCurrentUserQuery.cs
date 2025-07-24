namespace Application.Queries.Users
{
    public class GetCurrentUserQuery : IQuery<object>
    {
        public Guid UserId { get; set; }
    }
}