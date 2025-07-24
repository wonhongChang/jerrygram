using Application.Interfaces;
using Domain.Entities;

namespace Application.Commands.Comments
{
    public class DeleteCommentCommandHandler : ICommandHandler<DeleteCommentCommand>
    {
        private readonly ICommentRepository _commentRepository;
        private readonly INotificationRepository _notificationRepository;

        public DeleteCommentCommandHandler(
            ICommentRepository commentRepository,
            INotificationRepository notificationRepository)
        {
            _commentRepository = commentRepository;
            _notificationRepository = notificationRepository;
        }

        public async Task<bool> HandleAsync(DeleteCommentCommand command)
        {
            var comment = await _commentRepository.GetByIdAsync(command.CommentId);
            if (comment == null)
                throw new KeyNotFoundException("Comment not found.");

            if (comment.UserId != command.UserId)
                throw new UnauthorizedAccessException("You are not the owner of this comment.");

            _commentRepository.Remove(comment);

            var notification = await _notificationRepository.GetCommentNotificationAsync(comment.PostId, command.UserId);
            if (notification != null)
                _notificationRepository.Remove(notification);

            await _commentRepository.SaveChangesAsync();
            return true;
        }
    }
}