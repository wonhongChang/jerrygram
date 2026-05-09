import api from './api';
import { SearchResult, PopularSearch } from '../types';
import { normalizePopularSearch, normalizeSearchResult } from '../utils/apiData';

export const searchService = {
  async search(query: string): Promise<SearchResult> {
    const response = await api.get('/search', {
      params: { query },
    });
    return normalizeSearchResult(response.data);
  },

  async autocomplete(query: string): Promise<SearchResult> {
    const response = await api.get('/search/autocomplete', {
      params: { query },
    });
    return normalizeSearchResult(response.data);
  },

  async getPopularSearches(limit: number = 10, hours: number = 24): Promise<PopularSearch[]> {
    const response = await api.get('/search/popular', {
      params: { limit, hours },
    });
    return Array.isArray(response.data) ? response.data.map(normalizePopularSearch) : [];
  },

  async getTrendingSearches(limit: number = 5): Promise<PopularSearch[]> {
    const response = await api.get('/search/popular/trending', {
      params: { limit },
    });
    return Array.isArray(response.data) ? response.data.map(normalizePopularSearch) : [];
  },
};
