namespace Application.Commands.Comments
{
    public class DeleteCommentCommand : ICommand
    {
        public Guid CommentId { get; set; }
        public Guid UserId { get; set; }
    }
}