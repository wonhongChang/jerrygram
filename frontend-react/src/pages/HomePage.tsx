import React, { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import Layout from '../components/layout/Layout';
import PostCard from '../components/post/PostCard';
import EmptyState from '../components/ui/EmptyState';
import ErrorState from '../components/ui/ErrorState';
import LoadingSpinner from '../components/ui/LoadingSpinner';
import { Post } from '../types';
import { postService } from '../services/postService';
import { getApiErrorMessage } from '../utils/apiData';
import { FiCompass, FiPlus, FiSettings } from 'react-icons/fi';

const HomePage: React.FC = () => {
  const [posts, setPosts] = useState<Post[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const loadFeed = useCallback(async () => {
    try {
      setLoading(true);
      setError('');
      const response = await postService.getFeed();

      if (response.items.length > 0) {
        setPosts(response.items);
        return;
      }

      const publicPosts = await postService.getPosts();
      setPosts(publicPosts.items);
    } catch (err: any) {
      setError(getApiErrorMessage(err, 'Failed to load feed'));
      console.error('Failed to load feed:', err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadFeed();
  }, [loadFeed]);

  if (loading) {
    return (
      <Layout>
        <LoadingSpinner label="Loading feed" className="h-64" />
      </Layout>
    );
  }

  return (
    <Layout>
      <div className="grid gap-6 lg:grid-cols-[minmax(0,620px)_280px] lg:items-start">
        <section className="min-w-0">
          <div className="mb-5 flex items-end justify-between gap-4">
            <div>
              <h1 className="text-2xl font-bold text-gray-950">Home</h1>
              <p className="mt-1 text-sm text-gray-500">{posts.length} posts</p>
            </div>
            <Link
              to="/create"
              className="inline-flex items-center gap-2 rounded-md bg-gray-950 px-4 py-2 text-sm font-semibold text-white hover:bg-gray-800"
            >
              <FiPlus size={17} />
              New post
            </Link>
          </div>

          {error && <ErrorState message={error} onRetry={loadFeed} />}

          {!error && posts.length === 0 ? (
            <div className="rounded-lg border border-gray-200 bg-white shadow-sm">
              <EmptyState
                title="No posts yet"
                action={
                  <Link to="/explore" className="inline-flex rounded-md bg-primary px-4 py-2 text-sm font-semibold text-white hover:bg-blue-700">
                    Explore
                  </Link>
                }
              />
            </div>
          ) : (
            <div className="space-y-5">
              {posts.map((post) => <PostCard key={post.id} post={post} onPostUpdate={loadFeed} />)}
            </div>
          )}
        </section>

        <aside className="hidden space-y-4 lg:block lg:sticky lg:top-8">
          <div className="rounded-lg border border-gray-200 bg-white p-4 shadow-sm">
            <h2 className="text-sm font-semibold text-gray-950">Quick actions</h2>
            <div className="mt-4 space-y-2">
              <Link
                to="/explore"
                className="flex items-center gap-3 rounded-md px-3 py-2 text-sm font-semibold text-gray-700 hover:bg-gray-50 hover:text-gray-950"
              >
                <FiCompass size={18} />
                Explore
              </Link>
              <Link
                to="/settings"
                className="flex items-center gap-3 rounded-md px-3 py-2 text-sm font-semibold text-gray-700 hover:bg-gray-50 hover:text-gray-950"
              >
                <FiSettings size={18} />
                Settings
              </Link>
            </div>
          </div>
        </aside>
      </div>
    </Layout>
  );
};

export default HomePage;
