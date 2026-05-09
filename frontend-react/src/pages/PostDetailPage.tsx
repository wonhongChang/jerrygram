import React, { useCallback, useEffect, useRef, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import Layout from '../components/layout/Layout';
import CommentSection from '../components/post/CommentSection';
import Avatar from '../components/ui/Avatar';
import EmptyState from '../components/ui/EmptyState';
import ErrorState from '../components/ui/ErrorState';
import ImageWithFallback from '../components/ui/ImageWithFallback';
import LoadingSpinner from '../components/ui/LoadingSpinner';
import { Post } from '../types';
import { postService } from '../services/postService';
import { useAuth } from '../contexts/AuthContext';
import { formatRelativeTime, getApiErrorMessage } from '../utils/apiData';
import { FiBookmark, FiHeart, FiMessageCircle, FiTrash2 } from 'react-icons/fi';
import { FaHeart } from 'react-icons/fa';

const PostDetailPage: React.FC = () => {
  const { postId } = useParams<{ postId: string }>();
  const { user } = useAuth();
  const navigate = useNavigate();
  const [post, setPost] = useState<Post | null>(null);
  const [liked, setLiked] = useState(false);
  const [saved, setSaved] = useState(false);
  const [likes, setLikes] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [commentFocusSignal, setCommentFocusSignal] = useState<number | null>(null);
  const commentsRef = useRef<HTMLDivElement>(null);

  const loadPost = useCallback(async () => {
    if (!postId) return;

    try {
      setLoading(true);
      setError('');
      const data = await postService.getPost(postId);
      setPost(data);
      setLiked(data.liked);
      setSaved(data.saved);
      setLikes(data.likes);
    } catch (error) {
      setError(getApiErrorMessage(error, 'Failed to load post'));
      console.error('Failed to load post:', error);
    } finally {
      setLoading(false);
    }
  }, [postId]);

  useEffect(() => {
    loadPost();
  }, [loadPost]);

  const handleLike = async () => {
    if (!post) return;

    try {
      if (liked) {
        await postService.unlikePost(post.id);
        setLikes((current) => Math.max(0, current - 1));
      } else {
        await postService.likePost(post.id);
        setLikes((current) => current + 1);
      }
      setLiked((current) => !current);
    } catch (error) {
      setError(getApiErrorMessage(error, 'Failed to update like'));
      console.error('Failed to like/unlike post:', error);
    }
  };

  const handleDelete = async () => {
    if (!post) return;
    if (!window.confirm('Are you sure you want to delete this post?')) return;

    try {
      await postService.deletePost(post.id);
      navigate('/');
    } catch (error) {
      setError(getApiErrorMessage(error, 'Failed to delete post'));
      console.error('Failed to delete post:', error);
    }
  };

  const handleSave = async () => {
    if (!post) return;

    try {
      if (saved) {
        await postService.unsavePost(post.id);
      } else {
        await postService.savePost(post.id);
      }
      setSaved((current) => !current);
      setError('');
    } catch (error) {
      setError(getApiErrorMessage(error, 'Failed to update saved post'));
      console.error('Failed to save/unsave post:', error);
    }
  };

  const focusComments = () => {
    commentsRef.current?.scrollIntoView({ behavior: 'smooth', block: 'center' });
    setCommentFocusSignal((current) => (current ?? 0) + 1);
  };

  if (loading) {
    return (
      <Layout>
        <LoadingSpinner label="Loading post" className="h-64" />
      </Layout>
    );
  }

  if (error && !post) {
    return (
      <Layout>
        <ErrorState message={error} onRetry={loadPost} />
      </Layout>
    );
  }

  if (!post) {
    return (
      <Layout>
        <EmptyState title="Post not found" description="The post you're looking for does not exist." />
      </Layout>
    );
  }

  const isOwnPost = user?.id === post.user.id;

  return (
    <Layout>
      <div className="mx-auto max-w-5xl">
        {error && (
          <div className="mb-4">
            <ErrorState message={error} />
          </div>
        )}

        <div className="overflow-hidden rounded-lg border border-gray-200 bg-white shadow-sm md:flex">
          <div className="flex items-center justify-center bg-black md:w-3/5">
            <ImageWithFallback
              src={post.imageUrl}
              alt={post.caption || 'Post image'}
              className="w-full h-auto max-h-[600px] object-contain"
              fallbackClassName="min-h-[320px]"
            />
          </div>

          <div className="md:w-2/5 flex flex-col">
            <div className="flex items-center justify-between p-4 border-b border-gray-200">
              <Link to={`/${post.user.username}`} className="flex items-center space-x-3">
                <Avatar src={post.user.profileImageUrl} username={post.user.username} size="md" />
                <span className="font-semibold text-sm">{post.user.username}</span>
              </Link>
              {isOwnPost && (
                <button onClick={handleDelete} className="rounded-md p-2 text-gray-600 hover:bg-red-50 hover:text-red-500" aria-label="Delete post">
                  <FiTrash2 size={20} />
                </button>
              )}
            </div>

            {post.caption && (
              <div className="p-4 border-b border-gray-200">
                <div className="flex gap-3">
                  <Link to={`/${post.user.username}`}>
                    <Avatar src={post.user.profileImageUrl} username={post.user.username} size="sm" />
                  </Link>
                  <div>
                    <Link to={`/${post.user.username}`} className="font-semibold text-sm hover:text-gray-600">
                      {post.user.username}
                    </Link>
                    <p className="text-sm text-gray-700 mt-1 break-words">{post.caption}</p>
                    <div className="mt-2 text-xs text-gray-400">{formatRelativeTime(post.createdAt)}</div>
                  </div>
                </div>
              </div>
            )}

            <div ref={commentsRef} className="flex-1 overflow-y-auto p-4">
              <CommentSection postId={post.id} focusInputSignal={commentFocusSignal} />
            </div>

            <div className="border-t border-gray-200 p-4">
              <div className="flex items-center justify-between mb-2">
                <div className="flex items-center space-x-4">
                  <button onClick={handleLike} className="rounded-md p-2 text-gray-700 transition-colors hover:bg-gray-100 hover:text-gray-950" aria-label={liked ? 'Unlike post' : 'Like post'}>
                    {liked ? <FaHeart size={24} className="text-red-500" /> : <FiHeart size={24} />}
                  </button>
                  <button onClick={focusComments} className="rounded-md p-2 text-gray-700 hover:bg-gray-100 hover:text-gray-950" aria-label="Comments">
                    <FiMessageCircle size={24} />
                  </button>
                </div>
                <button
                  onClick={handleSave}
                  className={`rounded-md p-2 hover:bg-gray-100 hover:text-gray-950 ${saved ? 'text-gray-950' : 'text-gray-700'}`}
                  aria-label={saved ? 'Unsave post' : 'Save post'}
                >
                  <FiBookmark size={24} fill={saved ? 'currentColor' : 'none'} />
                </button>
              </div>

              <div className="font-semibold text-sm">
                {likes} {likes === 1 ? 'like' : 'likes'}
              </div>
            </div>
          </div>
        </div>
      </div>
    </Layout>
  );
};

export default PostDetailPage;
