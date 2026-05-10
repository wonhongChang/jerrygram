import type { Page, Route } from '@playwright/test';

const now = new Date('2026-05-10T00:00:00.000Z').toISOString();

const imageDataUrl = (from: string, to: string, label: string) =>
  `data:image/svg+xml,${encodeURIComponent(`
    <svg xmlns="http://www.w3.org/2000/svg" width="900" height="900" viewBox="0 0 900 900">
      <defs>
        <linearGradient id="g" x1="0" x2="1" y1="0" y2="1">
          <stop offset="0" stop-color="${from}" />
          <stop offset="1" stop-color="${to}" />
        </linearGradient>
      </defs>
      <rect width="900" height="900" fill="url(#g)" />
      <circle cx="680" cy="210" r="96" fill="rgba(255,255,255,0.28)" />
      <rect x="110" y="560" width="680" height="120" rx="18" fill="rgba(255,255,255,0.2)" />
      <text x="112" y="500" fill="#fff" font-family="Arial, sans-serif" font-size="72" font-weight="700">${label}</text>
    </svg>
  `)}`;

export const currentUser = {
  id: 'user-jerry',
  username: 'jerry',
  email: 'jerry@example.com',
  profileImageUrl: '',
  followerCount: 42,
  followingCount: 18,
};

export const demoPosts = [
  {
    id: 'post-kafka',
    caption: 'Kafka made this trend pop in real time. #kafka #realtime',
    imageUrl: imageDataUrl('#2563eb', '#22c55e', 'Kafka trend'),
    createdAt: now,
    likes: 18,
    liked: false,
    saved: false,
    user: {
      id: 'user-ada',
      username: 'ada',
      profileImageUrl: '',
    },
  },
  {
    id: 'post-coffee',
    caption: 'A quiet build log and a warm deploy. #devlife',
    imageUrl: imageDataUrl('#111827', '#f59e0b', 'Build log'),
    createdAt: now,
    likes: 9,
    liked: true,
    saved: true,
    user: {
      id: 'user-jerry',
      username: 'jerry',
      profileImageUrl: '',
    },
  },
];

const popularSearches = [
  {
    searchTerm: 'kafka',
    count: 64,
    rank: 1,
    lastSearched: now,
    category: 'rising',
  },
  {
    searchTerm: 'realtime',
    count: 37,
    rank: 2,
    lastSearched: now,
    category: 'rising',
  },
  {
    searchTerm: 'blob storage',
    count: 24,
    rank: 3,
    lastSearched: now,
    category: 'stable',
  },
];

export type ApiCallState = {
  likePosts: number;
  savePosts: number;
};

const fulfillJson = (route: Route, body: unknown, status = 200) =>
  route.fulfill({
    status,
    contentType: 'application/json',
    headers: {
      'access-control-allow-origin': '*',
      'access-control-allow-headers': 'authorization,content-type',
      'access-control-allow-methods': 'GET,POST,PUT,PATCH,DELETE,OPTIONS',
    },
    body: JSON.stringify(body),
  });

const pagedPosts = {
  items: demoPosts,
  totalCount: demoPosts.length,
  page: 1,
  pageSize: 10,
};

const searchResult = {
  posts: [demoPosts[0]],
  users: [
    currentUser,
    {
      id: 'user-ada',
      username: 'ada',
      email: '',
      profileImageUrl: '',
      followerCount: 108,
      followingCount: 12,
    },
  ],
  hashtags: ['kafka', 'realtime'],
};

export const signInForE2E = async (page: Page) => {
  await page.addInitScript(() => {
    window.localStorage.setItem('token', 'e2e-token');
  });
};

export const mockJerrygramApi = async (page: Page): Promise<ApiCallState> => {
  const calls: ApiCallState = {
    likePosts: 0,
    savePosts: 0,
  };

  await page.route('**/api/**', async (route) => {
    const request = route.request();
    const url = new URL(request.url());
    const method = request.method().toUpperCase();
    const path = url.pathname.replace(/^\/api/, '') || '/';

    if (method === 'OPTIONS') {
      await route.fulfill({
        status: 204,
        headers: {
          'access-control-allow-origin': '*',
          'access-control-allow-headers': 'authorization,content-type',
          'access-control-allow-methods': 'GET,POST,PUT,PATCH,DELETE,OPTIONS',
        },
      });
      return;
    }

    if (method === 'POST' && (path === '/auth/register' || path === '/auth/login')) {
      await fulfillJson(route, { token: 'e2e-token' });
      return;
    }

    if (method === 'GET' && path === '/users/me') {
      await fulfillJson(route, currentUser);
      return;
    }

    if (method === 'GET' && (path === '/posts/feed' || path === '/posts')) {
      await fulfillJson(route, pagedPosts);
      return;
    }

    if (method === 'POST' && /^\/posts\/[^/]+\/like$/.test(path)) {
      calls.likePosts += 1;
      await fulfillJson(route, {});
      return;
    }

    if (method === 'POST' && /^\/posts\/[^/]+\/save$/.test(path)) {
      calls.savePosts += 1;
      await fulfillJson(route, {});
      return;
    }

    if (method === 'GET' && path === '/search/popular/trending') {
      await fulfillJson(route, popularSearches.slice(0, 2));
      return;
    }

    if (method === 'GET' && path === '/search/popular') {
      await fulfillJson(route, popularSearches);
      return;
    }

    if (method === 'GET' && (path === '/search' || path === '/search/autocomplete')) {
      await fulfillJson(route, searchResult);
      return;
    }

    await fulfillJson(route, { message: `Unhandled E2E API route: ${method} ${path}` }, 404);
  });

  return calls;
};
