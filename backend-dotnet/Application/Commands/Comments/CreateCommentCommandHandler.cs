using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Commands.Comments
{
    public class CreateCommentCommandHandler : ICommandHandler<CreateCommentCommand, CommentResponseDto>
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IPostRepository _postRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationRepository _notificationRepository;

        public CreateCommentCommandHandler(
            ICommentRepository commentRepository,
            IPostRepository postRepository,
            IUserRepository userRepository,
            INotificationRepository notificationRepository)
        {
            _commentRepository = commentRepository;
            _postRepository = postRepository;
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
        }

        public async Task<CommentResponseDto> HandleAsync(CreateCommentCommand command)
        {
            var post = await _postRepository.GetPostWithUserAsync(command.PostId);
            if (post == null)
                throw new KeyNotFoundException("Post not found.");

            var user = await _userRepository.GetByIdAsync(command.UserId);
            if (user == null)
                throw new UnauthorizedAccessException("User not found.");

            var comment = new Comment
            {
                PostId = command.PostId,
                UserId = command.UserId,
                Content = command.Dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _commentRepository.Add(comment);

            // Save notification if commenter is not the post owner
            if (post.UserId != command.UserId)
            {
                var notification = new Notification
                {
                    RecipientId = post.UserId,
                    FromUserId = command.UserId,
                    Type = NotificationType.Comment,
                    PostId = command.PostId,
                    Message = $"{user.Username} commented on your post.",
                    CreatedAt = DateTime.UtcNow
                };

                _notificationRepository.Add(notification);
            }

            await _commentRepository.SaveChangesAsync();

            return new CommentResponseDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                User = new SimpleUserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    ProfileImageUrl = user.ProfileImageUrl
                }
            };
        }
    }
}