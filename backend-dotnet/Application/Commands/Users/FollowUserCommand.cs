namespace Application.Commands.Users
{
    public class FollowUserCommand : ICommand
    {
        public Guid CurrentUserId { get; set; }
        public Guid TargetUserId { get; set; }
    }
}