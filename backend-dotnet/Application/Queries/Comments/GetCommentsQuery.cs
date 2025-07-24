namespace Application.Queries.Comments
{
    public class GetCommentsQuery : IQuery<object>
    {
        public Guid PostId { get; set; }
    }
}