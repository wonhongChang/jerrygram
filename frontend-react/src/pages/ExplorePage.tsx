import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import Layout from '../components/layout/Layout';
import { Post } from '../types';
import { postService } from '../services/postService';
import { FiHeart } from 'react-icons/fi';

const ExplorePage: React.FC = () => {
  const [posts, setPosts] = useState<Post[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadExplorePosts();
  }, []);

  const loadExplorePosts = async () => {
    try {
      const data = await postService.getExplorePosts();
      setPosts(data);
    } catch (error) {
      console.error('Failed to load explore posts:', error);
    } finally {
      setLoading(false);
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

  return (
    <Layout>
      <div className="max-w-6xl mx-auto">
        <h1 className="text-2xl font-bold mb-6">Explore</h1>
        <div className="grid grid-cols-3 gap-1 md:gap-4">
          {posts.map((post) => (
            <Link
              key={post.id}
              to={`/p/${post.id}`}
              className="relative aspect-square bg-gray-100 group overflow-hidden"
            >
              <img
                src={post.imageUrl}
                alt={post.caption}
                className="w-full h-full object-cover"
              />
              <div className="absolute inset-0 bg-black bg-opacity-0 group-hover:bg-opacity-30 transition-all duration-200 flex items-center justify-center">
                <div className="opacity-0 group-hover:opacity-100 transition-opacity flex items-center space-x-4 text-white font-semibold">
                  <div className="flex items-center space-x-1">
                    <FiHeart size={20} />
                    <span>{post.likes}</span>
                  </div>
                </div>
              </div>
            </Link>
          ))}
        </div>
      </div>
    </Layout>
  );
};

export default ExplorePage;
