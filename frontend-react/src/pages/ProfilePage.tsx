import React, { useCallback, useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import Layout from '../components/layout/Layout';
import Avatar from '../components/ui/Avatar';
import EmptyState from '../components/ui/EmptyState';
import ErrorState from '../components/ui/ErrorState';
import ImageWithFallback from '../components/ui/ImageWithFallback';
import LoadingSpinner from '../components/ui/LoadingSpinner';
import { Post, User } from '../types';
import { userService } from '../services/userService';
import { postService } from '../services/postService';
import { useAuth } from '../contexts/AuthContext';
import { getApiErrorMessage } from '../utils/apiData';
import { FiGrid, FiHeart, FiSettings } from 'react-icons/fi';

const ProfilePage: React.FC = () => {
  const { username } = useParams<{ username: string }>();
  const { user: currentUser } = useAuth();
  const [profile, setProfile] = useState<User | null>(null);
  const [posts, setPosts] = useState<Post[]>([]);
  const [savedPosts, setSavedPosts] = useState<Post[]>([]);
  const [loading, setLoading] = useState(true);
  const [savedLoading, setSavedLoading] = useState(false);
  const [error, setError] = useState('');
  const [savedError, setSavedError] = useState('');
  const [activeTab, setActiveTab] = useState<'posts' | 'saved'>('posts');

  const isOwnProfile = currentUser?.username?.toLowerCase() === username?.toLowerCase();

  const loadProfile = useCallback(async () => {
    if (!username) return;

    try {
      setLoading(true);
      setError('');
      const userData = await userService.getUserProfile(username);
      setProfile(userData);

      const postsData = await postService.getPosts(1, 50);
      setPosts(postsData.items.filter((post) => post.user.username.toLowerCase() === username.toLowerCase()));
    } catch (error) {
      setError(getApiErrorMessage(error, 'Failed to load profile'));
      console.error('Failed to load profile:', error);
    } finally {
      setLoading(false);
    }
  }, [username]);

  useEffect(() => {
    loadProfile();
  }, [loadProfile]);

  useEffect(() => {
    if (!isOwnProfile || activeTab !== 'saved') return;

    const loadSavedPosts = async () => {
      try {
        setSavedLoading(true);
        setSavedError('');
        const data = await postService.getSavedPosts(1, 60);
        setSavedPosts(data.items);
      } catch (error) {
        setSavedError(getApiErrorMessage(error, 'Failed to load saved posts'));
        console.error('Failed to load saved posts:', error);
      } finally {
        setSavedLoading(false);
      }
    };

    loadSavedPosts();
  }, [activeTab, isOwnProfile]);

  const handleFollowToggle = async () => {
    if (!profile) return;

    try {
      if (profile.isFollowing) {
        await userService.unfollowUser(profile.id);
        setProfile({ ...profile, isFollowing: false, followerCount: Math.max(0, profile.followerCount - 1) });
      } else {
        await userService.followUser(profile.id);
        setProfile({ ...profile, isFollowing: true, followerCount: profile.followerCount + 1 });
      }
      setError('');
    } catch (error) {
      setError(getApiErrorMessage(error, 'Failed to update follow status'));
      console.error('Failed to toggle follow:', error);
    }
  };

  if (loading) {
    return (
      <Layout>
        <LoadingSpinner label="Loading profile" className="h-64" />
      </Layout>
    );
  }

  if (error && !profile) {
    return (
      <Layout>
        <ErrorState message={error} onRetry={loadProfile} />
      </Layout>
    );
  }

  if (!profile) {
    return (
      <Layout>
        <EmptyState title="User not found" description="The user you're looking for does not exist." />
      </Layout>
    );
  }

  return (
    <Layout>
      <div className="mx-auto max-w-5xl">
        {error && (
          <div className="mb-4">
            <ErrorState message={error} />
          </div>
        )}

        <section className="mb-8 rounded-lg border border-gray-200 bg-white p-5 shadow-sm sm:p-6">
          <div className="flex flex-col gap-6 sm:flex-row sm:items-start sm:gap-8">
            <div className="flex-shrink-0">
              <Avatar src={profile.profileImageUrl} username={profile.username} size="xl" />
            </div>

            <div className="min-w-0 flex-1">
              <div className="mb-6 flex flex-wrap items-center gap-3">
                <h1 className="break-all text-2xl font-bold text-gray-950">{profile.username}</h1>
              {isOwnProfile ? (
                <Link to="/settings" className="rounded-md border border-gray-300 px-4 py-2 text-sm font-semibold text-gray-800 hover:bg-gray-50">
                  Edit Profile
                </Link>
              ) : (
                <button
                  onClick={handleFollowToggle}
                  className={`rounded-md px-5 py-2 text-sm font-semibold ${
                    profile.isFollowing ? 'bg-gray-200 text-gray-800 hover:bg-gray-300' : 'bg-primary text-white hover:bg-blue-700'
                  }`}
                >
                  {profile.isFollowing ? 'Following' : 'Follow'}
                </button>
              )}
              {isOwnProfile && (
                <Link to="/settings" className="rounded-md p-2 text-gray-700 hover:bg-gray-100 hover:text-gray-900" aria-label="Settings">
                  <FiSettings size={24} />
                </Link>
              )}
              </div>

              <div className="mb-6 flex flex-wrap gap-x-8 gap-y-3 text-sm sm:text-base">
                <div>
                  <span className="font-semibold text-gray-950">{posts.length}</span> posts
                </div>
                <div>
                  <span className="font-semibold text-gray-950">{profile.followerCount}</span> followers
                </div>
                <div>
                  <span className="font-semibold text-gray-950">{profile.followingCount}</span> following
                </div>
              </div>

              <p className="font-semibold text-gray-950">{profile.username}</p>
            </div>
          </div>
        </section>

        <div className="border-t border-gray-200">
          <div className="flex justify-center gap-10 sm:gap-16">
            <button
              onClick={() => setActiveTab('posts')}
              className={`flex items-center gap-2 border-t-2 py-4 ${
                activeTab === 'posts' ? 'border-gray-900 text-gray-900' : 'border-transparent text-gray-400'
              }`}
            >
              <FiGrid size={16} />
              <span className="text-xs font-semibold">Posts</span>
            </button>
            {isOwnProfile && (
              <button
                onClick={() => setActiveTab('saved')}
                className={`flex items-center gap-2 border-t-2 py-4 ${
                  activeTab === 'saved' ? 'border-gray-900 text-gray-900' : 'border-transparent text-gray-400'
                }`}
              >
                <FiHeart size={16} />
                <span className="text-xs font-semibold">Saved</span>
              </button>
            )}
          </div>
        </div>

        {activeTab === 'posts' && (
          <div className="mt-4 grid grid-cols-3 gap-2 sm:gap-3">
            {posts.length === 0 ? (
              <div className="col-span-3">
                <div className="rounded-lg border border-gray-200 bg-white shadow-sm">
                  <EmptyState title="No posts yet" />
                </div>
              </div>
            ) : (
              posts.map((post) => (
                <Link key={post.id} to={`/p/${post.id}`} className="group relative aspect-square overflow-hidden rounded-md bg-gray-100">
                  <ImageWithFallback src={post.imageUrl} alt={post.caption || 'Post image'} className="w-full h-full object-cover" />
                  <div className="absolute inset-0 flex items-center justify-center bg-black bg-opacity-0 transition-all duration-200 group-hover:bg-opacity-35">
                    <div className="flex items-center space-x-4 text-white opacity-0 transition-opacity group-hover:opacity-100">
                      <div className="flex items-center space-x-1">
                        <FiHeart size={20} fill="white" />
                        <span className="text-sm font-semibold">{post.likes}</span>
                      </div>
                    </div>
                  </div>
                </Link>
              ))
            )}
          </div>
        )}

        {activeTab === 'saved' && (
          <div className="mt-4">
            {savedLoading ? (
              <LoadingSpinner label="Loading saved posts" className="h-40" />
            ) : savedError ? (
              <ErrorState message={savedError} />
            ) : savedPosts.length === 0 ? (
              <div className="rounded-lg border border-gray-200 bg-white shadow-sm">
                <EmptyState title="No saved posts yet" />
              </div>
            ) : (
              <div className="grid grid-cols-3 gap-2 sm:gap-3">
                {savedPosts.map((post) => (
                  <Link key={post.id} to={`/p/${post.id}`} className="group relative aspect-square overflow-hidden rounded-md bg-gray-100">
                    <ImageWithFallback src={post.imageUrl} alt={post.caption || 'Post image'} className="w-full h-full object-cover" />
                    <div className="absolute inset-0 flex items-center justify-center bg-black bg-opacity-0 transition-all duration-200 group-hover:bg-opacity-35">
                      <div className="flex items-center space-x-4 text-white opacity-0 transition-opacity group-hover:opacity-100">
                        <div className="flex items-center space-x-1">
                          <FiHeart size={20} fill="white" />
                          <span className="text-sm font-semibold">{post.likes}</span>
                        </div>
                      </div>
                    </div>
                  </Link>
                ))}
              </div>
            )}
          </div>
        )}
      </div>
    </Layout>
  );
};

export default ProfilePage;
