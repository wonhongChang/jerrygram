import React, { useEffect, useState } from 'react';
import Layout from '../components/layout/Layout';
import PostCard from '../components/post/PostCard';
import { Post } from '../types';
import { postService } from '../services/postService';

const HomePage: React.FC = () => {
  const [posts, setPosts] = useState<Post[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    loadFeed();
  }, []);

  const loadFeed = async () => {
    try {
      setLoading(true);
      const response = await postService.getFeed();
      setPosts(response.items);
    } catch (err: any) {
      setError('Failed to load feed');
      console.error('Failed to load feed:', err);
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

  if (error) {
    return (
      <Layout>
        <div className="text-center text-red-600 py-8">{error}</div>
      </Layout>
    );
  }

  return (
    <Layout>
      <div className="max-w-2xl mx-auto">
        {posts.length === 0 ? (
          <div className="text-center py-12">
            <p className="text-gray-500 text-lg mb-4">No posts to show</p>
            <p className="text-gray-400 text-sm">
              Follow users to see their posts in your feed
            </p>
          </div>
        ) : (
          posts.map((post) => (
            <PostCard key={post.id} post={post} onPostUpdate={loadFeed} />
          ))
        )}
      </div>
    </Layout>
  );
};

export default HomePage;
