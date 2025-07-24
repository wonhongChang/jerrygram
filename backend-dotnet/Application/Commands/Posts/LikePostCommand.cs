namespace Application.Commands.Posts
{
    public class LikePostCommand : ICommand
    {
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
    }
}