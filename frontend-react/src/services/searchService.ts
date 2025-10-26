import api from './api';
import { SearchResult, PopularSearch } from '../types';

export const searchService = {
  async search(query: string): Promise<SearchResult> {
    const response = await api.get<SearchResult>('/search', {
      params: { query },
    });
    return response.data;
  },

  async autocomplete(query: string): Promise<SearchResult> {
    const response = await api.get<SearchResult>('/search/autocomplete', {
      params: { query },
    });
    return response.data;
  },

  async getPopularSearches(limit: number = 10, hours: number = 24): Promise<PopularSearch[]> {
    const response = await api.get<PopularSearch[]>('/search/popular', {
      params: { limit, hours },
    });
    return response.data;
  },

  async getTrendingSearches(limit: number = 5): Promise<PopularSearch[]> {
    const response = await api.get<PopularSearch[]>('/search/popular/trending', {
      params: { limit },
    });
    return response.data;
  },
};
