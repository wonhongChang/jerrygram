import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import Layout from '../components/layout/Layout';
import { User, Post } from '../types';
import { userService } from '../services/userService';
import { postService } from '../services/postService';
import { useAuth } from '../contexts/AuthContext';
import { FiSettings, FiGrid, FiHeart } from 'react-icons/fi';

const ProfilePage: React.FC = () => {
  const { username } = useParams<{ username: string }>();
  const { user: currentUser } = useAuth();
  const [profile, setProfile] = useState<User | null>(null);
  const [posts, setPosts] = useState<Post[]>([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<'posts' | 'saved'>('posts');

  const isOwnProfile = currentUser?.username === username;

  useEffect(() => {
    loadProfile();
  }, [username]);

  const loadProfile = async () => {
    if (!username) return;

    try {
      setLoading(true);
      const userData = await userService.getUserProfile(username);
      setProfile(userData);

      // Load user's posts
      const postsData = await postService.getPosts();
      // Filter posts by this user (in real app, there would be a dedicated endpoint)
      setPosts(postsData.items.filter(p => p.user.username === username));
    } catch (error) {
      console.error('Failed to load profile:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleFollowToggle = async () => {
    if (!profile) return;

    try {
      if (profile.isFollowing) {
        await userService.unfollowUser(profile.id);
        setProfile({ ...profile, isFollowing: false, followerCount: profile.followerCount - 1 });
      } else {
        await userService.followUser(profile.id);
        setProfile({ ...profile, isFollowing: true, followerCount: profile.followerCount + 1 });
      }
    } catch (error) {
      console.error('Failed to toggle follow:', error);
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

  if (!profile) {
    return (
      <Layout>
        <div className="text-center py-12">
          <h2 className="text-2xl font-semibold mb-2">User not found</h2>
          <p className="text-gray-500">The user you're looking for doesn't exist.</p>
        </div>
      </Layout>
    );
  }

  return (
    <Layout>
      <div className="max-w-4xl mx-auto">
        {/* Profile Header */}
        <div className="flex items-start gap-8 mb-12">
          {/* Profile Picture */}
          <div className="flex-shrink-0">
            {profile.profileImageUrl ? (
              <img
                src={profile.profileImageUrl}
                alt={profile.username}
                className="w-32 h-32 md:w-40 md:h-40 rounded-full object-cover border-2 border-gray-300"
              />
            ) : (
              <div className="w-32 h-32 md:w-40 md:h-40 rounded-full bg-gray-300 flex items-center justify-center text-gray-600 text-5xl">
                {profile.username[0].toUpperCase()}
              </div>
            )}
          </div>

          {/* Profile Info */}
          <div className="flex-1">
            <div className="flex items-center gap-4 mb-6">
              <h1 className="text-2xl font-light">{profile.username}</h1>
              {isOwnProfile ? (
                <Link
                  to="/settings"
                  className="px-4 py-1.5 border border-gray-300 rounded font-semibold text-sm hover:bg-gray-50"
                >
                  Edit Profile
                </Link>
              ) : (
                <button
                  onClick={handleFollowToggle}
                  className={`px-6 py-1.5 rounded font-semibold text-sm ${
                    profile.isFollowing
                      ? 'bg-gray-200 text-gray-800 hover:bg-gray-300'
                      : 'bg-primary text-white hover:bg-blue-600'
                  }`}
                >
                  {profile.isFollowing ? 'Following' : 'Follow'}
                </button>
              )}
              {isOwnProfile && (
                <Link to="/settings" className="text-gray-700 hover:text-gray-900">
                  <FiSettings size={24} />
                </Link>
              )}
            </div>

            {/* Stats */}
            <div className="flex gap-8 mb-6">
              <div>
                <span className="font-semibold">{posts.length}</span> posts
              </div>
              <div className="cursor-pointer hover:text-gray-600">
                <span className="font-semibold">{profile.followerCount}</span> followers
              </div>
              <div className="cursor-pointer hover:text-gray-600">
                <span className="font-semibold">{profile.followingCount}</span> following
              </div>
            </div>

            {/* Bio */}
            <div>
              <p className="font-semibold">{profile.username}</p>
            </div>
          </div>
        </div>

        {/* Tabs */}
        <div className="border-t border-gray-300">
          <div className="flex justify-center gap-16">
            <button
              onClick={() => setActiveTab('posts')}
              className={`flex items-center gap-2 py-4 border-t-2 ${
                activeTab === 'posts'
                  ? 'border-gray-900 text-gray-900'
                  : 'border-transparent text-gray-400'
              }`}
            >
              <FiGrid size={16} />
              <span className="text-xs font-semibold uppercase tracking-wider">Posts</span>
            </button>
            {isOwnProfile && (
              <button
                onClick={() => setActiveTab('saved')}
                className={`flex items-center gap-2 py-4 border-t-2 ${
                  activeTab === 'saved'
                    ? 'border-gray-900 text-gray-900'
                    : 'border-transparent text-gray-400'
                }`}
              >
                <FiHeart size={16} />
                <span className="text-xs font-semibold uppercase tracking-wider">Saved</span>
              </button>
            )}
          </div>
        </div>

        {/* Posts Grid */}
        {activeTab === 'posts' && (
          <div className="grid grid-cols-3 gap-1 md:gap-4 mt-4">
            {posts.length === 0 ? (
              <div className="col-span-3 text-center py-12">
                <p className="text-gray-500 text-lg">No posts yet</p>
              </div>
            ) : (
              posts.map((post) => (
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
                        <FiHeart size={20} fill="white" />
                        <span>{post.likes}</span>
                      </div>
                    </div>
                  </div>
                </Link>
              ))
            )}
          </div>
        )}

        {activeTab === 'saved' && (
          <div className="text-center py-12">
            <p className="text-gray-500">Saved posts feature coming soon</p>
          </div>
        )}
      </div>
    </Layout>
  );
};

export default ProfilePage;
