using Application.DTOs;
using Application.Interfaces;

namespace Application.Queries.Posts
{
    public class GetPostLikesQueryHandler : IQueryHandler<GetPostLikesQuery, PagedResult<SimpleUserDto>>
    {
        private readonly IPostLikeRepository _postLikeRepository;

        public GetPostLikesQueryHandler(IPostLikeRepository postLikeRepository)
        {
            _postLikeRepository = postLikeRepository;
        }

        public async Task<PagedResult<SimpleUserDto>> HandleAsync(GetPostLikesQuery query)
        {
            return await _postLikeRepository.GetPostLikesAsync(query.PostId, query.Page, query.PageSize);
        }
    }
}