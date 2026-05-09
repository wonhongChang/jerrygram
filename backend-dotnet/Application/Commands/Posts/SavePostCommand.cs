namespace Application.Commands.Posts
{
    public class SavePostCommand : ICommand
    {
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
    }
}
