using Application.Interfaces;

namespace Application.Queries.Users
{
    public class GetCurrentUserQueryHandler : IQueryHandler<GetCurrentUserQuery, object>
    {
        private readonly IUserRepository _userRepository;

        public GetCurrentUserQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<object> HandleAsync(GetCurrentUserQuery query)
        {
            var user = await _userRepository.GetByIdAsync(query.UserId);

            if (user == null)
                return null!;

            return new
            {
                user.Id,
                user.Username,
                user.Email,
                user.ProfileImageUrl,
                user.CreatedAt
            };
        }
    }
}