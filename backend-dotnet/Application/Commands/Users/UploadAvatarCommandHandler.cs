using Application.Common;
using Application.Interfaces;

namespace Application.Commands.Users
{
    public class UploadAvatarCommandHandler : ICommandHandler<UploadAvatarCommand, object>
    {
        private readonly IUserRepository _userRepository;
        private readonly IBlobService _blobService;
        private readonly IElasticService _elasticService;

        public UploadAvatarCommandHandler(
            IUserRepository userRepository,
            IBlobService blobService,
            IElasticService elasticService)
        {
            _userRepository = userRepository;
            _blobService = blobService;
            _elasticService = elasticService;
        }

        public async Task<object> HandleAsync(UploadAvatarCommand command)
        {
            var user = await _userRepository.GetByIdAsync(command.UserId);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
            {
                await _blobService.DeleteAsync(user.ProfileImageUrl, BlobContainers.Profile);
            }

            var imageUrl = await _blobService.UploadAsync(command.Dto.Avatar.OpenReadStream(), command.Dto.Avatar.FileName, BlobContainers.Profile);
            user.ProfileImageUrl = imageUrl;

            await _userRepository.SaveChangesAsync();

            // Update the user's profile in the search index
            await _elasticService.IndexUserAsync(new UserIndex
            {
                Id = user.Id,
                Username = user.Username,
                ProfileImageUrl = user.ProfileImageUrl
            });

            return new { imageUrl };
        }
    }
}