import api from './api';
import { LoginDto, RegisterDto, AuthResponse } from '../types';

export const authService = {
  async login(credentials: LoginDto): Promise<AuthResponse> {
    const response = await api.post<AuthResponse & { accessToken?: string }>('/auth/login', credentials);
    const token = response.data.token || response.data.accessToken;
    if (token) {
      localStorage.setItem('token', token);
    }
    return { token: token || '' };
  },

  async register(data: RegisterDto): Promise<AuthResponse> {
    const response = await api.post<AuthResponse & { accessToken?: string }>('/auth/register', data);
    const token = response.data.token || response.data.accessToken;
    if (token) {
      localStorage.setItem('token', token);
    }
    return { token: token || '' };
  },

  logout() {
    localStorage.removeItem('token');
  },

  isAuthenticated(): boolean {
    return !!localStorage.getItem('token');
  },

  getToken(): string | null {
    return localStorage.getItem('token');
  },
};
