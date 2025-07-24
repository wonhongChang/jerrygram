namespace Application.Commands.Posts
{
    public class DeletePostCommand : ICommand
    {
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
    }
}