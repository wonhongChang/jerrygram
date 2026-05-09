import React, { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import Layout from '../components/layout/Layout';
import EmptyState from '../components/ui/EmptyState';
import ErrorState from '../components/ui/ErrorState';
import ImageWithFallback from '../components/ui/ImageWithFallback';
import LoadingSpinner from '../components/ui/LoadingSpinner';
import { Post } from '../types';
import { postService } from '../services/postService';
import { getApiErrorMessage } from '../utils/apiData';
import { FiHeart } from 'react-icons/fi';

const ExplorePage: React.FC = () => {
  const [posts, setPosts] = useState<Post[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const loadExplorePosts = useCallback(async () => {
    try {
      setLoading(true);
      setError('');
      const data = await postService.getExplorePosts();
      setPosts(data);
    } catch (error) {
      setError(getApiErrorMessage(error, 'Failed to load explore posts'));
      console.error('Failed to load explore posts:', error);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadExplorePosts();
  }, [loadExplorePosts]);

  if (loading) {
    return (
      <Layout>
        <LoadingSpinner label="Loading explore" className="h-64" />
      </Layout>
    );
  }

  return (
    <Layout>
      <div>
        <div className="mb-6 flex items-end justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold text-gray-950">Explore</h1>
            <p className="mt-1 text-sm text-gray-500">{posts.length} posts</p>
          </div>
        </div>
        {error && <ErrorState message={error} onRetry={loadExplorePosts} />}

        {!error && posts.length === 0 ? (
          <div className="rounded-lg border border-gray-200 bg-white shadow-sm">
            <EmptyState title="Nothing to explore yet" />
          </div>
        ) : (
          <div className="grid grid-cols-3 gap-2 sm:gap-3">
            {posts.map((post) => (
              <Link key={post.id} to={`/p/${post.id}`} className="group relative aspect-square overflow-hidden rounded-md bg-gray-100">
                <ImageWithFallback src={post.imageUrl} alt={post.caption || 'Post image'} className="w-full h-full object-cover" />
                <div className="absolute inset-0 flex items-center justify-center bg-black bg-opacity-0 transition-all duration-200 group-hover:bg-opacity-35">
                  <div className="flex items-center space-x-4 text-white opacity-0 transition-opacity group-hover:opacity-100">
                    <div className="flex items-center space-x-1">
                      <FiHeart size={20} />
                      <span className="text-sm font-semibold">{post.likes}</span>
                    </div>
                  </div>
                </div>
              </Link>
            ))}
          </div>
        )}
      </div>
    </Layout>
  );
};

export default ExplorePage;
