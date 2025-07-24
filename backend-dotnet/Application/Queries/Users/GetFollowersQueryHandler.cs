using Application.Interfaces;

namespace Application.Queries.Users
{
    public class GetFollowersQueryHandler : IQueryHandler<GetFollowersQuery, object>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserFollowRepository _userFollowRepository;

        public GetFollowersQueryHandler(
            IUserRepository userRepository,
            IUserFollowRepository userFollowRepository)
        {
            _userRepository = userRepository;
            _userFollowRepository = userFollowRepository;
        }

        public async Task<object> HandleAsync(GetFollowersQuery query)
        {
            var userExists = await _userRepository.GetByIdAsync(query.UserId);
            if (userExists == null)
                return null!;

            var followers = await _userFollowRepository.GetFollowersAsync(query.UserId);
            return followers;
        }
    }
}