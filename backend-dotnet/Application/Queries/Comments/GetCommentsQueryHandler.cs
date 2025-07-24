using Application.Interfaces;

namespace Application.Queries.Comments
{
    public class GetCommentsQueryHandler : IQueryHandler<GetCommentsQuery, object>
    {
        private readonly ICommentRepository _commentRepository;

        public GetCommentsQueryHandler(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public async Task<object> HandleAsync(GetCommentsQuery query)
        {
            return await _commentRepository.GetCommentsWithUsersByPostIdAsync(query.PostId);
        }
    }
}