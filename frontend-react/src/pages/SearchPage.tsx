import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import Layout from '../components/layout/Layout';
import { SearchResult, PopularSearch } from '../types';
import { searchService } from '../services/searchService';
import { FiSearch, FiTrendingUp, FiHash, FiUser } from 'react-icons/fi';

const SearchPage: React.FC = () => {
  const [query, setQuery] = useState('');
  const [searchResults, setSearchResults] = useState<SearchResult | null>(null);
  const [popularSearches, setPopularSearches] = useState<PopularSearch[]>([]);
  const [trendingSearches, setTrendingSearches] = useState<PopularSearch[]>([]);
  const [loading, setLoading] = useState(false);
  const [showAutocomplete, setShowAutocomplete] = useState(false);

  useEffect(() => {
    loadTrendingAndPopular();
  }, []);

  useEffect(() => {
    const timer = setTimeout(() => {
      if (query.trim()) {
        handleAutocomplete();
      } else {
        setSearchResults(null);
        setShowAutocomplete(false);
      }
    }, 300);

    return () => clearTimeout(timer);
  }, [query]);

  const loadTrendingAndPopular = async () => {
    try {
      const [trending, popular] = await Promise.all([
        searchService.getTrendingSearches(10),
        searchService.getPopularSearches(20)
      ]);
      setTrendingSearches(trending);
      setPopularSearches(popular);
    } catch (error) {
      console.error('Failed to load searches:', error);
    }
  };

  const handleAutocomplete = async () => {
    if (!query.trim()) return;

    try {
      const results = await searchService.autocomplete(query);
      setSearchResults(results);
      setShowAutocomplete(true);
    } catch (error) {
      console.error('Failed to search:', error);
    }
  };

  const handleSearch = async (searchQuery: string) => {
    if (!searchQuery.trim()) return;

    setLoading(true);
    try {
      const results = await searchService.search(searchQuery);
      setSearchResults(results);
      setShowAutocomplete(false);
    } catch (error) {
      console.error('Failed to search:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    handleSearch(query);
  };

  return (
    <Layout>
      <div className="max-w-4xl mx-auto">
        {/* Search Bar */}
        <div className="bg-white border border-gray-300 rounded-lg p-6 mb-6">
          <form onSubmit={handleSubmit} className="relative">
            <div className="relative">
              <FiSearch className="absolute left-4 top-1/2 transform -translate-y-1/2 text-gray-400" size={20} />
              <input
                type="text"
                value={query}
                onChange={(e) => setQuery(e.target.value)}
                onFocus={() => query && setShowAutocomplete(true)}
                placeholder="Search for users, posts, or hashtags..."
                className="w-full pl-12 pr-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent"
              />
            </div>

            {/* Autocomplete Dropdown */}
            {showAutocomplete && searchResults && (
              <div className="absolute z-10 w-full mt-2 bg-white border border-gray-300 rounded-lg shadow-lg max-h-96 overflow-y-auto">
                {/* Users */}
                {searchResults.users && searchResults.users.length > 0 && (
                  <div className="p-2">
                    <div className="px-3 py-2 text-xs font-semibold text-gray-500 uppercase">Users</div>
                    {searchResults.users.map((user) => (
                      <Link
                        key={user.id}
                        to={`/${user.username}`}
                        className="flex items-center gap-3 px-3 py-2 hover:bg-gray-50 rounded-lg"
                        onClick={() => setShowAutocomplete(false)}
                      >
                        {user.profileImageUrl ? (
                          <img
                            src={user.profileImageUrl}
                            alt={user.username}
                            className="w-10 h-10 rounded-full object-cover"
                          />
                        ) : (
                          <div className="w-10 h-10 rounded-full bg-gray-300 flex items-center justify-center">
                            <FiUser size={20} />
                          </div>
                        )}
                        <div>
                          <div className="font-semibold text-sm">{user.username}</div>
                          <div className="text-xs text-gray-500">
                            {user.followerCount} followers
                          </div>
                        </div>
                      </Link>
                    ))}
                  </div>
                )}

                {/* Hashtags */}
                {searchResults.hashtags && searchResults.hashtags.length > 0 && (
                  <div className="p-2 border-t border-gray-200">
                    <div className="px-3 py-2 text-xs font-semibold text-gray-500 uppercase">Hashtags</div>
                    {searchResults.hashtags.map((tag, index) => (
                      <button
                        key={index}
                        onClick={() => {
                          setQuery(`#${tag}`);
                          handleSearch(`#${tag}`);
                          setShowAutocomplete(false);
                        }}
                        className="flex items-center gap-3 px-3 py-2 hover:bg-gray-50 rounded-lg w-full text-left"
                      >
                        <div className="w-10 h-10 rounded-full bg-gray-100 flex items-center justify-center">
                          <FiHash size={20} className="text-gray-600" />
                        </div>
                        <div className="font-semibold text-sm">#{tag}</div>
                      </button>
                    ))}
                  </div>
                )}
              </div>
            )}
          </form>
        </div>

        {/* Search Results */}
        {!showAutocomplete && searchResults && (
          <div>
            {/* Posts Results */}
            {searchResults.posts && searchResults.posts.length > 0 && (
              <div className="mb-8">
                <h2 className="text-xl font-semibold mb-4">Posts</h2>
                <div className="grid grid-cols-3 gap-1 md:gap-4">
                  {searchResults.posts.map((post) => (
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
                      <div className="absolute inset-0 bg-black bg-opacity-0 group-hover:bg-opacity-30 transition-all duration-200" />
                    </Link>
                  ))}
                </div>
              </div>
            )}

            {/* Users Results */}
            {searchResults.users && searchResults.users.length > 0 && (
              <div className="mb-8">
                <h2 className="text-xl font-semibold mb-4">Users</h2>
                <div className="bg-white border border-gray-300 rounded-lg divide-y">
                  {searchResults.users.map((user) => (
                    <Link
                      key={user.id}
                      to={`/${user.username}`}
                      className="flex items-center gap-3 p-4 hover:bg-gray-50"
                    >
                      {user.profileImageUrl ? (
                        <img
                          src={user.profileImageUrl}
                          alt={user.username}
                          className="w-12 h-12 rounded-full object-cover"
                        />
                      ) : (
                        <div className="w-12 h-12 rounded-full bg-gray-300" />
                      )}
                      <div>
                        <div className="font-semibold">{user.username}</div>
                        <div className="text-sm text-gray-500">
                          {user.followerCount} followers
                        </div>
                      </div>
                    </Link>
                  ))}
                </div>
              </div>
            )}

            {/* No Results */}
            {(!searchResults.posts || searchResults.posts.length === 0) &&
              (!searchResults.users || searchResults.users.length === 0) && (
                <div className="text-center py-12">
                  <p className="text-gray-500 text-lg">No results found</p>
                  <p className="text-gray-400 text-sm mt-2">Try searching for something else</p>
                </div>
              )}
          </div>
        )}

        {/* Trending & Popular (shown when no search query) */}
        {!query && !searchResults && (
          <div className="space-y-8">
            {/* Trending */}
            {trendingSearches.length > 0 && (
              <div className="bg-white border border-gray-300 rounded-lg p-6">
                <h2 className="text-xl font-semibold mb-4 flex items-center gap-2">
                  <FiTrendingUp className="text-red-500" />
                  Trending Now
                </h2>
                <div className="space-y-3">
                  {trendingSearches.map((search, index) => (
                    <button
                      key={index}
                      onClick={() => {
                        setQuery(search.searchTerm);
                        handleSearch(search.searchTerm);
                      }}
                      className="w-full text-left p-3 hover:bg-gray-50 rounded-lg transition-colors"
                    >
                      <div className="flex items-center justify-between">
                        <div>
                          <div className="font-semibold">{search.searchTerm}</div>
                          <div className="text-sm text-gray-500">{search.count} searches</div>
                        </div>
                        <div className="text-xs px-2 py-1 bg-red-100 text-red-600 rounded-full font-semibold">
                          #{search.rank}
                        </div>
                      </div>
                    </button>
                  ))}
                </div>
              </div>
            )}

            {/* Popular */}
            {popularSearches.length > 0 && (
              <div className="bg-white border border-gray-300 rounded-lg p-6">
                <h2 className="text-xl font-semibold mb-4">Popular Searches</h2>
                <div className="flex flex-wrap gap-2">
                  {popularSearches.map((search, index) => (
                    <button
                      key={index}
                      onClick={() => {
                        setQuery(search.searchTerm);
                        handleSearch(search.searchTerm);
                      }}
                      className="px-4 py-2 bg-gray-100 hover:bg-gray-200 rounded-full text-sm font-medium transition-colors"
                    >
                      {search.searchTerm}
                    </button>
                  ))}
                </div>
              </div>
            )}
          </div>
        )}
      </div>
    </Layout>
  );
};

export default SearchPage;
