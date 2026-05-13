import React, { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import Layout from '../components/layout/Layout';
import Avatar from '../components/ui/Avatar';
import EmptyState from '../components/ui/EmptyState';
import ErrorState from '../components/ui/ErrorState';
import ImageWithFallback from '../components/ui/ImageWithFallback';
import LoadingSpinner from '../components/ui/LoadingSpinner';
import { PopularSearch, SearchResult } from '../types';
import { searchService } from '../services/searchService';
import { getApiErrorMessage } from '../utils/apiData';
import { FiHash, FiImage, FiRefreshCw, FiSearch, FiTrendingUp, FiUser } from 'react-icons/fi';

const SEARCH_TREND_REFRESH_MS = 10000;

const formatTrendRefreshTime = (value: Date | null): string => {
  if (!value) return '';

  const pad = (part: number) => part.toString().padStart(2, '0');
  return `${pad(value.getHours())}:${pad(value.getMinutes())}:${pad(value.getSeconds())}`;
};

const SearchPage: React.FC = () => {
  const [query, setQuery] = useState('');
  const [searchResults, setSearchResults] = useState<SearchResult | null>(null);
  const [popularSearches, setPopularSearches] = useState<PopularSearch[]>([]);
  const [trendingSearches, setTrendingSearches] = useState<PopularSearch[]>([]);
  const [refreshingTrends, setRefreshingTrends] = useState(false);
  const [lastTrendRefresh, setLastTrendRefresh] = useState<Date | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [showAutocomplete, setShowAutocomplete] = useState(false);

  const loadTrendingAndPopular = useCallback(async (showRefreshing = false) => {
    if (showRefreshing) {
      setRefreshingTrends(true);
    }

    try {
      const [trending, popular] = await Promise.all([
        searchService.getTrendingSearches(10),
        searchService.getPopularSearches(20),
      ]);
      setTrendingSearches(trending);
      setPopularSearches(popular);
      setLastTrendRefresh(new Date());
    } catch (error) {
      console.error('Failed to load searches:', error);
    } finally {
      if (showRefreshing) {
        setRefreshingTrends(false);
      }
    }
  }, []);

  const handleAutocomplete = useCallback(async (value: string) => {
    if (!value.trim()) return;

    try {
      const results = await searchService.autocomplete(value);
      setSearchResults(results);
      setShowAutocomplete(true);
    } catch (error) {
      console.error('Failed to autocomplete:', error);
    }
  }, []);

  const handleSearch = useCallback(async (searchQuery: string) => {
    if (!searchQuery.trim()) return;

    setLoading(true);
    setError('');
    try {
      const results = await searchService.search(searchQuery);
      setSearchResults(results);
      setShowAutocomplete(false);
      window.setTimeout(() => {
        void loadTrendingAndPopular();
      }, 1200);
    } catch (error) {
      setError(getApiErrorMessage(error, 'Failed to search'));
      console.error('Failed to search:', error);
    } finally {
      setLoading(false);
    }
  }, [loadTrendingAndPopular]);

  useEffect(() => {
    void loadTrendingAndPopular(true);

    const intervalId = window.setInterval(() => {
      if (document.visibilityState === 'visible') {
        void loadTrendingAndPopular();
      }
    }, SEARCH_TREND_REFRESH_MS);

    return () => window.clearInterval(intervalId);
  }, [loadTrendingAndPopular]);

  useEffect(() => {
    const timer = setTimeout(() => {
      if (query.trim()) {
        handleAutocomplete(query);
      } else {
        setSearchResults(null);
        setShowAutocomplete(false);
        setError('');
      }
    }, 300);

    return () => clearTimeout(timer);
  }, [query, handleAutocomplete]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    handleSearch(query);
  };

  const runSearch = (searchTerm: string) => {
    setQuery(searchTerm);
    handleSearch(searchTerm);
  };

  const hasSearchResults =
    !!searchResults &&
    (searchResults.posts.length > 0 || searchResults.users.length > 0 || searchResults.hashtags.length > 0);
  const hasTrendData = trendingSearches.length > 0 || popularSearches.length > 0;
  const trendRefreshLabel = formatTrendRefreshTime(lastTrendRefresh);

  return (
    <Layout>
      <div className="mx-auto max-w-4xl">
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-gray-950">Search</h1>
          <form onSubmit={handleSubmit} className="relative mt-4">
            <FiSearch className="pointer-events-none absolute left-4 top-1/2 -translate-y-1/2 text-gray-400" size={20} />
            <input
              type="text"
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              onFocus={() => query && hasSearchResults && setShowAutocomplete(true)}
              placeholder="Users, posts, or hashtags"
              className="h-12 w-full rounded-lg border border-gray-200 bg-white pl-12 pr-4 text-sm shadow-sm outline-none transition focus:border-primary focus:ring-2 focus:ring-blue-100"
            />

            {showAutocomplete && searchResults && hasSearchResults && (
              <div className="absolute z-20 mt-2 max-h-96 w-full overflow-y-auto rounded-lg border border-gray-200 bg-white p-2 shadow-lg">
                {searchResults.users.length > 0 && (
                  <div>
                    <div className="flex items-center gap-2 px-3 py-2 text-xs font-semibold text-gray-500">
                      <FiUser size={14} />
                      Users
                    </div>
                    {searchResults.users.map((user) => (
                      <Link
                        key={user.id}
                        to={`/${user.username}`}
                        className="flex items-center gap-3 rounded-md px-3 py-2 hover:bg-gray-50"
                        onClick={() => setShowAutocomplete(false)}
                      >
                        <Avatar src={user.profileImageUrl} username={user.username} size="md" />
                        <div className="min-w-0">
                          <div className="truncate text-sm font-semibold text-gray-950">{user.username}</div>
                          <div className="text-xs text-gray-500">{user.followerCount} followers</div>
                        </div>
                      </Link>
                    ))}
                  </div>
                )}

                {searchResults.hashtags.length > 0 && (
                  <div className="mt-2 border-t border-gray-100 pt-2">
                    <div className="flex items-center gap-2 px-3 py-2 text-xs font-semibold text-gray-500">
                      <FiHash size={14} />
                      Hashtags
                    </div>
                    {searchResults.hashtags.map((tag) => (
                      <button
                        key={tag}
                        onClick={() => {
                          runSearch(`#${tag}`);
                          setShowAutocomplete(false);
                        }}
                        className="flex w-full items-center gap-3 rounded-md px-3 py-2 text-left hover:bg-gray-50"
                      >
                        <span className="flex h-9 w-9 items-center justify-center rounded-md bg-gray-100 text-gray-600">
                          <FiHash size={18} />
                        </span>
                        <span className="text-sm font-semibold text-gray-950">#{tag}</span>
                      </button>
                    ))}
                  </div>
                )}
              </div>
            )}
          </form>
        </div>

        <section className="mb-6 rounded-lg border border-gray-200 bg-white p-4 shadow-sm">
          <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
            <div className="min-w-0">
              <h2 className="flex items-center gap-2 text-sm font-semibold text-gray-950">
                <FiTrendingUp className="text-red-500" />
                Live trends
              </h2>
              {trendRefreshLabel && (
                <p className="mt-1 text-xs text-gray-500">Updated {trendRefreshLabel}</p>
              )}
            </div>
            <button
              type="button"
              onClick={() => loadTrendingAndPopular(true)}
              disabled={refreshingTrends}
              aria-label="Refresh live trends"
              className="inline-flex h-9 w-9 items-center justify-center rounded-md border border-gray-200 text-gray-600 transition hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-60"
            >
              <FiRefreshCw className={refreshingTrends ? 'animate-spin' : ''} size={16} />
            </button>
          </div>

          {hasTrendData ? (
            <div className="grid gap-4 lg:grid-cols-2">
              {trendingSearches.length > 0 && (
                <div>
                  <h3 className="mb-2 text-xs font-semibold uppercase text-gray-500">Trending</h3>
                  <div className="space-y-1">
                    {trendingSearches.map((search) => (
                      <button
                        key={`${search.searchTerm}-${search.rank}-live`}
                        onClick={() => runSearch(search.searchTerm)}
                        className="w-full rounded-md px-3 py-2 text-left transition-colors hover:bg-gray-50"
                      >
                        <div className="flex items-center justify-between gap-4">
                          <div className="min-w-0">
                            <div className="truncate text-sm font-semibold text-gray-950">{search.searchTerm}</div>
                            <div className="text-xs text-gray-500">{search.count} searches</div>
                          </div>
                          <div className="rounded-md bg-red-50 px-2 py-1 text-xs font-semibold text-red-600">
                            #{search.rank}
                          </div>
                        </div>
                      </button>
                    ))}
                  </div>
                </div>
              )}

              {popularSearches.length > 0 && (
                <div>
                  <h3 className="mb-2 text-xs font-semibold uppercase text-gray-500">Popular</h3>
                  <div className="flex flex-wrap gap-2">
                    {popularSearches.map((search) => (
                      <button
                        key={`${search.searchTerm}-${search.rank}-popular-live`}
                        onClick={() => runSearch(search.searchTerm)}
                        className="rounded-md border border-gray-200 bg-gray-50 px-3 py-2 text-sm font-semibold text-gray-700 transition-colors hover:bg-gray-100"
                      >
                        {search.searchTerm}
                      </button>
                    ))}
                  </div>
                </div>
              )}
            </div>
          ) : (
            <EmptyState title={refreshingTrends ? 'Loading live trends' : 'No search trends yet'} />
          )}
        </section>

        {error && (
          <div className="mb-6">
            <ErrorState message={error} onRetry={() => handleSearch(query)} />
          </div>
        )}

        {!showAutocomplete && searchResults && (
          <div className="space-y-8">
            {loading && <LoadingSpinner label="Searching" className="py-4" />}

            {searchResults.posts.length > 0 && (
              <section>
                <div className="mb-4 flex items-center gap-2">
                  <FiImage className="text-gray-500" size={18} />
                  <h2 className="text-lg font-semibold text-gray-950">Posts</h2>
                </div>
                <div className="grid grid-cols-3 gap-2 sm:gap-3">
                  {searchResults.posts.map((post) => (
                    <Link key={post.id} to={`/p/${post.id}`} className="group relative aspect-square overflow-hidden rounded-md bg-gray-100">
                      <ImageWithFallback src={post.imageUrl} alt={post.caption || 'Post image'} className="h-full w-full object-cover" />
                      <div className="absolute inset-0 bg-black bg-opacity-0 transition-all duration-200 group-hover:bg-opacity-35" />
                    </Link>
                  ))}
                </div>
              </section>
            )}

            {searchResults.users.length > 0 && (
              <section>
                <div className="mb-4 flex items-center gap-2">
                  <FiUser className="text-gray-500" size={18} />
                  <h2 className="text-lg font-semibold text-gray-950">Users</h2>
                </div>
                <div className="divide-y divide-gray-100 rounded-lg border border-gray-200 bg-white shadow-sm">
                  {searchResults.users.map((user) => (
                    <Link key={user.id} to={`/${user.username}`} className="flex items-center gap-3 p-4 hover:bg-gray-50">
                      <Avatar src={user.profileImageUrl} username={user.username} size="lg" />
                      <div className="min-w-0">
                        <div className="truncate font-semibold text-gray-950">{user.username}</div>
                        <div className="text-sm text-gray-500">{user.followerCount} followers</div>
                      </div>
                    </Link>
                  ))}
                </div>
              </section>
            )}

            {searchResults.hashtags.length > 0 && (
              <section>
                <div className="mb-4 flex items-center gap-2">
                  <FiHash className="text-gray-500" size={18} />
                  <h2 className="text-lg font-semibold text-gray-950">Hashtags</h2>
                </div>
                <div className="flex flex-wrap gap-2">
                  {searchResults.hashtags.map((tag) => (
                    <button
                      key={tag}
                      onClick={() => runSearch(`#${tag}`)}
                      className="rounded-md border border-gray-200 bg-white px-3 py-2 text-sm font-semibold text-gray-700 shadow-sm hover:bg-gray-50"
                    >
                      #{tag}
                    </button>
                  ))}
                </div>
              </section>
            )}

            {!loading && !hasSearchResults && (
              <div className="rounded-lg border border-gray-200 bg-white shadow-sm">
                <EmptyState title="No results found" />
              </div>
            )}
          </div>
        )}
      </div>
    </Layout>
  );
};

export default SearchPage;
