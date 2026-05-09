namespace Application.Commands.Posts
{
    public class UnsavePostCommand : ICommand
    {
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
    }
}
