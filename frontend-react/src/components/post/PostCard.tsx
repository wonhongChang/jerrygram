import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { FiHeart, FiMessageCircle, FiBookmark, FiMoreHorizontal } from 'react-icons/fi';
import { FaHeart } from 'react-icons/fa';
import { Post } from '../../types';
import { postService } from '../../services/postService';
import { formatDistanceToNow } from 'date-fns';

interface PostCardProps {
  post: Post;
  onPostUpdate?: () => void;
}

const PostCard: React.FC<PostCardProps> = ({ post, onPostUpdate }) => {
  const [liked, setLiked] = useState(post.liked);
  const [likes, setLikes] = useState(post.likes);
  const [showComments, setShowComments] = useState(false);

  const handleLike = async () => {
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

  return (
    <div className="bg-white border border-gray-300 rounded-lg mb-6">
      {/* Post Header */}
      <div className="flex items-center justify-between p-4">
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
        <button className="text-gray-600 hover:text-gray-800">
          <FiMoreHorizontal size={20} />
        </button>
      </div>

      {/* Post Image */}
      <div className="w-full aspect-square bg-gray-100">
        <img
          src={post.imageUrl}
          alt={post.caption}
          className="w-full h-full object-cover"
        />
      </div>

      {/* Post Actions */}
      <div className="p-4">
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
            <button
              onClick={() => setShowComments(!showComments)}
              className="text-gray-700 hover:text-gray-500"
            >
              <FiMessageCircle size={24} />
            </button>
          </div>
          <button className="text-gray-700 hover:text-gray-500">
            <FiBookmark size={24} />
          </button>
        </div>

        {/* Likes Count */}
        <div className="font-semibold text-sm mb-2">
          {likes} {likes === 1 ? 'like' : 'likes'}
        </div>

        {/* Caption */}
        <div className="text-sm">
          <Link to={`/${post.user.username}`} className="font-semibold mr-2">
            {post.user.username}
          </Link>
          <span className="text-gray-700">{post.caption}</span>
        </div>

        {/* Timestamp */}
        <div className="text-gray-400 text-xs mt-2 uppercase">
          {formatDistanceToNow(new Date(post.createdAt), { addSuffix: true })}
        </div>
      </div>
    </div>
  );
};

export default PostCard;
