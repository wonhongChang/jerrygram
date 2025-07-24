using Application.DTOs;

namespace Application.Commands.Users
{
    public class UploadAvatarCommand : ICommand<object>
    {
        public Guid UserId { get; set; }
        public AvatarUploadDto Dto { get; set; } = null!;
    }
}