import React, { useCallback, useEffect, useRef, useState } from 'react';
import { Link } from 'react-router-dom';
import { FiTrash2 } from 'react-icons/fi';
import { Comment } from '../../types';
import { commentService } from '../../services/commentService';
import { useAuth } from '../../contexts/AuthContext';
import Avatar from '../ui/Avatar';
import LoadingSpinner from '../ui/LoadingSpinner';
import { formatRelativeTime, getApiErrorMessage } from '../../utils/apiData';

interface CommentSectionProps {
  postId: string;
  focusInputSignal?: number | null;
}

const CommentSection: React.FC<CommentSectionProps> = ({ postId, focusInputSignal }) => {
  const { user } = useAuth();
  const inputRef = useRef<HTMLInputElement>(null);
  const [comments, setComments] = useState<Comment[]>([]);
  const [newComment, setNewComment] = useState('');
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');

  const loadComments = useCallback(async () => {
    try {
      setError('');
      setLoading(true);
      const data = await commentService.getComments(postId);
      setComments(data);
    } catch (error) {
      setError(getApiErrorMessage(error, 'Failed to load comments'));
      console.error('Failed to load comments:', error);
    } finally {
      setLoading(false);
    }
  }, [postId]);

  useEffect(() => {
    loadComments();
  }, [loadComments]);

  useEffect(() => {
    if (focusInputSignal == null || loading) return;

    const timeoutId = window.setTimeout(() => {
      inputRef.current?.focus();
    }, 0);

    return () => window.clearTimeout(timeoutId);
  }, [focusInputSignal, loading]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newComment.trim() || submitting) return;

    setSubmitting(true);
    try {
      const comment = await commentService.createComment(postId, { content: newComment });
      setComments((current) => [...current, comment]);
      setNewComment('');
      setError('');
    } catch (error) {
      setError(getApiErrorMessage(error, 'Failed to post comment'));
      console.error('Failed to post comment:', error);
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (commentId: string) => {
    if (!window.confirm('Are you sure you want to delete this comment?')) return;

    try {
      await commentService.deleteComment(commentId);
      setComments((current) => current.filter((comment) => comment.id !== commentId));
      setError('');
    } catch (error) {
      setError(getApiErrorMessage(error, 'Failed to delete comment'));
      console.error('Failed to delete comment:', error);
    }
  };

  if (loading) {
    return <LoadingSpinner label="Loading comments" className="py-4" />;
  }

  return (
    <div className="space-y-4">
      {error && <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-600">{error}</p>}

      <div className="max-h-96 space-y-3 overflow-y-auto pr-1">
        {comments.length === 0 ? (
          <p className="py-4 text-center text-sm text-gray-400">No comments yet</p>
        ) : (
          comments.map((comment) => (
            <div key={comment.id} className="flex gap-3 rounded-md px-1 py-1">
              <Link to={`/${comment.user.username}`} className="flex-shrink-0">
                <Avatar src={comment.user.profileImageUrl} username={comment.user.username} size="sm" />
              </Link>
              <div className="flex-1 min-w-0">
                <div className="flex items-start justify-between gap-2">
                  <div>
                    <Link to={`/${comment.user.username}`} className="font-semibold text-sm hover:text-gray-600">
                      {comment.user.username}
                    </Link>
                    <p className="text-sm text-gray-700 break-words">{comment.content}</p>
                  </div>
                  {user?.id === comment.user.id && (
                    <button
                      onClick={() => handleDelete(comment.id)}
                      className="text-gray-400 hover:text-red-500 flex-shrink-0"
                      title="Delete comment"
                      aria-label="Delete comment"
                    >
                      <FiTrash2 size={14} />
                    </button>
                  )}
                </div>
                <div className="text-xs text-gray-400 mt-1">{formatRelativeTime(comment.createdAt)}</div>
              </div>
            </div>
          ))
        )}
      </div>

      <form onSubmit={handleSubmit} className="border-t border-gray-200 pt-4">
        <div className="flex gap-2">
          <input
            ref={inputRef}
            type="text"
            value={newComment}
            onChange={(e) => setNewComment(e.target.value)}
            placeholder="Add a comment..."
            maxLength={1000}
            className="min-w-0 flex-1 rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary"
          />
          <button
            type="submit"
            disabled={!newComment.trim() || submitting}
            className="rounded-md bg-primary px-4 py-2 text-sm font-semibold text-white hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {submitting ? 'Posting...' : 'Post'}
          </button>
        </div>
      </form>
    </div>
  );
};

export default CommentSection;
