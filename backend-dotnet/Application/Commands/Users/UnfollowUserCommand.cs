namespace Application.Commands.Users
{
    public class UnfollowUserCommand : ICommand
    {
        public Guid CurrentUserId { get; set; }
        public Guid TargetUserId { get; set; }
    }
}