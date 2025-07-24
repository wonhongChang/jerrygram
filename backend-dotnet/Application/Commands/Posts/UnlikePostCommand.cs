namespace Application.Commands.Posts
{
    public class UnlikePostCommand : ICommand
    {
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
    }
}