using Application.Interfaces;

namespace Application.Queries.Users
{
    public class GetUserProfileQueryHandler : IQueryHandler<GetUserProfileQuery, object>
    {
        private readonly IUserRepository _userRepository;

        public GetUserProfileQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<object> HandleAsync(GetUserProfileQuery query)
        {
            var user = await _userRepository.GetUserProfileByUsernameAsync(query.Username);
            return user ?? throw new KeyNotFoundException($"User with username '{query.Username}' not found");
        }
    }
}