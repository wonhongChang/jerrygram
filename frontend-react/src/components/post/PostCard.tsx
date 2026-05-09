import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { FiBookmark, FiHeart, FiMessageCircle, FiMoreHorizontal, FiTrash2 } from 'react-icons/fi';
import { FaHeart } from 'react-icons/fa';
import { Post } from '../../types';
import { postService } from '../../services/postService';
import { useAuth } from '../../contexts/AuthContext';
import Avatar from '../ui/Avatar';
import ImageWithFallback from '../ui/ImageWithFallback';
import CommentSection from './CommentSection';
import { formatRelativeTime } from '../../utils/apiData';

interface PostCardProps {
  post: Post;
  onPostUpdate?: () => void;
}

const PostCard: React.FC<PostCardProps> = ({ post, onPostUpdate }) => {
  const { user } = useAuth();
  const [liked, setLiked] = useState(post.liked);
  const [likes, setLikes] = useState(post.likes);
  const [saved, setSaved] = useState(post.saved);
  const [showComments, setShowComments] = useState(false);
  const [showOptions, setShowOptions] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [visible, setVisible] = useState(true);
  const isOwnPost = user?.id === post.user.id;

  useEffect(() => {
    setLiked(post.liked);
    setLikes(post.likes);
    setSaved(post.saved);
    setVisible(true);
  }, [post.id, post.liked, post.likes, post.saved]);

  const handleLike = async () => {
    try {
      if (liked) {
        await postService.unlikePost(post.id);
        setLikes((current) => Math.max(0, current - 1));
      } else {
        await postService.likePost(post.id);
        setLikes((current) => current + 1);
      }
      setLiked((current) => !current);
      onPostUpdate?.();
    } catch (error) {
      console.error('Failed to like/unlike post:', error);
    }
  };

  const handleSave = async () => {
    try {
      if (saved) {
        await postService.unsavePost(post.id);
      } else {
        await postService.savePost(post.id);
      }
      setSaved((current) => !current);
      onPostUpdate?.();
    } catch (error) {
      console.error('Failed to save/unsave post:', error);
    }
  };

  const handleDelete = async () => {
    if (!isOwnPost || deleting) return;
    if (!window.confirm('Are you sure you want to delete this post?')) return;

    setDeleting(true);
    try {
      await postService.deletePost(post.id);
      setVisible(false);
      onPostUpdate?.();
    } catch (error) {
      console.error('Failed to delete post:', error);
    } finally {
      setDeleting(false);
    }
  };

  if (!visible) return null;

  return (
    <article className="overflow-hidden rounded-lg border border-gray-200 bg-white shadow-sm">
      <div className="flex items-center justify-between px-4 py-3 sm:px-5">
        <Link to={`/${post.user.username}`} className="flex items-center space-x-3">
          <Avatar src={post.user.profileImageUrl} username={post.user.username} size="md" />
          <span className="min-w-0">
            <span className="block truncate text-sm font-semibold text-gray-950">{post.user.username}</span>
            <span className="block text-xs text-gray-500">{formatRelativeTime(post.createdAt)}</span>
          </span>
        </Link>
        <div className="relative">
          <button
            type="button"
            onClick={() => setShowOptions((current) => !current)}
            className="rounded-md p-2 text-gray-500 hover:bg-gray-100 hover:text-gray-900"
            aria-label="Post options"
          >
            <FiMoreHorizontal size={20} />
          </button>

          {showOptions && (
            <div className="absolute right-0 top-full z-20 mt-1 w-40 rounded-md border border-gray-200 bg-white p-1 shadow-lg">
              <Link
                to={`/p/${post.id}`}
                className="block rounded-md px-3 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50"
                onClick={() => setShowOptions(false)}
              >
                View post
              </Link>
              {isOwnPost && (
                <button
                  type="button"
                  onClick={handleDelete}
                  disabled={deleting}
                  className="flex w-full items-center gap-2 rounded-md px-3 py-2 text-left text-sm font-medium text-red-600 hover:bg-red-50 disabled:opacity-50"
                >
                  <FiTrash2 size={15} />
                  {deleting ? 'Deleting...' : 'Delete'}
                </button>
              )}
            </div>
          )}
        </div>
      </div>

      <Link to={`/p/${post.id}`} className="block aspect-[4/5] w-full bg-gray-100 sm:aspect-square">
        <ImageWithFallback
          src={post.imageUrl}
          alt={post.caption || 'Post image'}
          className="w-full h-full object-cover"
        />
      </Link>

      <div className="px-4 py-4 sm:px-5">
        <div className="mb-3 flex items-center justify-between">
          <div className="flex items-center gap-2">
            <button
              onClick={handleLike}
              className="flex h-10 w-10 items-center justify-center rounded-md text-gray-700 transition-colors hover:bg-gray-100 hover:text-gray-950"
              aria-label={liked ? 'Unlike post' : 'Like post'}
            >
              {liked ? <FaHeart size={24} className="text-red-500" /> : <FiHeart size={24} />}
            </button>
            <button
              onClick={() => setShowComments((current) => !current)}
              className="flex h-10 w-10 items-center justify-center rounded-md text-gray-700 hover:bg-gray-100 hover:text-gray-950"
              aria-label="Toggle comments"
            >
              <FiMessageCircle size={24} />
            </button>
          </div>
          <button
            onClick={handleSave}
            className={`flex h-10 w-10 items-center justify-center rounded-md transition-colors hover:bg-gray-100 ${
              saved ? 'text-gray-950' : 'text-gray-700 hover:text-gray-950'
            }`}
            aria-label={saved ? 'Unsave post' : 'Save post'}
          >
            <FiBookmark size={24} fill={saved ? 'currentColor' : 'none'} />
          </button>
        </div>

        <div className="mb-2 text-sm font-semibold text-gray-950">
          {likes} {likes === 1 ? 'like' : 'likes'}
        </div>

        {post.caption && (
          <div className="text-sm leading-6">
            <Link to={`/${post.user.username}`} className="mr-2 font-semibold text-gray-950">
              {post.user.username}
            </Link>
            <span className="text-gray-700 break-words">{post.caption}</span>
          </div>
        )}

        {showComments && (
          <div className="mt-4 border-t border-gray-100 pt-4">
            <CommentSection postId={post.id} />
          </div>
        )}
      </div>
    </article>
  );
};

export default PostCard;
