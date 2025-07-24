using Application.Interfaces;

namespace Application.Queries.Users
{
    public class GetFollowingsQueryHandler : IQueryHandler<GetFollowingsQuery, object>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserFollowRepository _userFollowRepository;

        public GetFollowingsQueryHandler(
            IUserRepository userRepository,
            IUserFollowRepository userFollowRepository)
        {
            _userRepository = userRepository;
            _userFollowRepository = userFollowRepository;
        }

        public async Task<object> HandleAsync(GetFollowingsQuery query)
        {
            var userExists = await _userRepository.GetByIdAsync(query.UserId);
            if (userExists == null)
                return null!;

            var followings = await _userFollowRepository.GetFollowingsAsync(query.UserId);
            return followings;
        }
    }
}