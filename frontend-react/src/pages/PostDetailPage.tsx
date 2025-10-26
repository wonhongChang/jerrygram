import React, { useEffect, useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import Layout from '../components/layout/Layout';
import CommentSection from '../components/post/CommentSection';
import { Post } from '../types';
import { postService } from '../services/postService';
import { useAuth } from '../contexts/AuthContext';
import { formatDistanceToNow } from 'date-fns';
import { FiHeart, FiMessageCircle, FiBookmark, FiMoreHorizontal } from 'react-icons/fi';
import { FaHeart } from 'react-icons/fa';

const PostDetailPage: React.FC = () => {
  const { postId } = useParams<{ postId: string }>();
  const { user } = useAuth();
  const navigate = useNavigate();
  const [post, setPost] = useState<Post | null>(null);
  const [liked, setLiked] = useState(false);
  const [likes, setLikes] = useState(0);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadPost();
  }, [postId]);

  const loadPost = async () => {
    if (!postId) return;

    try {
      const data = await postService.getPost(postId);
      setPost(data);
      setLiked(data.liked);
      setLikes(data.likes);
    } catch (error) {
      console.error('Failed to load post:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleLike = async () => {
    if (!post) return;

    try {
      if (liked) {
        await postService.unlikePost(post.id);
        setLikes(likes - 1);
      } else {
        await postService.likePost(post.id);
        setLikes(likes + 1);
      }
      setLiked(!liked);
    } catch (error) {
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
      console.error('Failed to delete post:', error);
    }
  };

  if (loading) {
    return (
      <Layout>
        <div className="flex justify-center items-center h-64">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary"></div>
        </div>
      </Layout>
    );
  }

  if (!post) {
    return (
      <Layout>
        <div className="text-center py-12">
          <h2 className="text-2xl font-semibold mb-2">Post not found</h2>
          <p className="text-gray-500">The post you're looking for doesn't exist.</p>
        </div>
      </Layout>
    );
  }

  const isOwnPost = user?.id === post.user.id;

  return (
    <Layout>
      <div className="max-w-5xl mx-auto">
        <div className="bg-white border border-gray-300 rounded-lg overflow-hidden md:flex">
          {/* Image Section */}
          <div className="md:w-3/5 bg-black flex items-center justify-center">
            <img
              src={post.imageUrl}
              alt={post.caption}
              className="w-full h-auto max-h-[600px] object-contain"
            />
          </div>

          {/* Details Section */}
          <div className="md:w-2/5 flex flex-col">
            {/* Post Header */}
            <div className="flex items-center justify-between p-4 border-b border-gray-300">
              <Link to={`/${post.user.username}`} className="flex items-center space-x-3">
                {post.user.profileImageUrl ? (
                  <img
                    src={post.user.profileImageUrl}
                    alt={post.user.username}
                    className="w-10 h-10 rounded-full object-cover"
                  />
                ) : (
                  <div className="w-10 h-10 rounded-full bg-gray-300" />
                )}
                <span className="font-semibold text-sm">{post.user.username}</span>
              </Link>
              {isOwnPost && (
                <div className="relative">
                  <button
                    onClick={handleDelete}
                    className="text-gray-600 hover:text-red-500"
                  >
                    <FiMoreHorizontal size={20} />
                  </button>
                </div>
              )}
            </div>

            {/* Caption */}
            <div className="p-4 border-b border-gray-300">
              <div className="flex gap-3">
                <Link to={`/${post.user.username}`}>
                  {post.user.profileImageUrl ? (
                    <img
                      src={post.user.profileImageUrl}
                      alt={post.user.username}
                      className="w-8 h-8 rounded-full object-cover"
                    />
                  ) : (
                    <div className="w-8 h-8 rounded-full bg-gray-300" />
                  )}
                </Link>
                <div>
                  <Link
                    to={`/${post.user.username}`}
                    className="font-semibold text-sm hover:text-gray-600"
                  >
                    {post.user.username}
                  </Link>
                  <p className="text-sm text-gray-700 mt-1">{post.caption}</p>
                  <div className="text-xs text-gray-400 mt-2 uppercase">
                    {formatDistanceToNow(new Date(post.createdAt), { addSuffix: true })}
                  </div>
                </div>
              </div>
            </div>

            {/* Comments */}
            <div className="flex-1 overflow-y-auto p-4">
              <CommentSection postId={post.id} />
            </div>

            {/* Actions */}
            <div className="border-t border-gray-300 p-4">
              <div className="flex items-center justify-between mb-2">
                <div className="flex items-center space-x-4">
                  <button
                    onClick={handleLike}
                    className="text-gray-700 hover:text-gray-500 transition-colors"
                  >
                    {liked ? (
                      <FaHeart size={24} className="text-red-500" />
                    ) : (
                      <FiHeart size={24} />
                    )}
                  </button>
                  <button className="text-gray-700 hover:text-gray-500">
                    <FiMessageCircle size={24} />
                  </button>
                </div>
                <button className="text-gray-700 hover:text-gray-500">
                  <FiBookmark size={24} />
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
