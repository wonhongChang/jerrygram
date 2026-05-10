import { chromium } from '@playwright/test';
import { spawn } from 'node:child_process';
import { mkdir } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const frontendRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const repoRoot = path.resolve(frontendRoot, '..');
const screenshotDir = path.join(repoRoot, 'docs', 'assets', 'screenshots');
const port = Number(process.env.SCREENSHOT_PORT || process.env.E2E_PORT || 13002);
const baseURL = process.env.E2E_BASE_URL || `http://127.0.0.1:${port}`;
const apiURL = process.env.REACT_APP_API_URL || 'http://127.0.0.1:5018/api';

const now = new Date('2026-05-10T00:00:00.000Z').toISOString();
const imageDataUrl = (from, to, label) =>
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

const currentUser = {
  id: 'user-jerry',
  username: 'jerry',
  email: 'jerry@example.com',
  profileImageUrl: '',
  followerCount: 42,
  followingCount: 18,
};

const posts = [
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
  { searchTerm: 'kafka', count: 64, rank: 1, lastSearched: now, category: 'rising' },
  { searchTerm: 'realtime', count: 37, rank: 2, lastSearched: now, category: 'rising' },
  { searchTerm: 'blob storage', count: 24, rank: 3, lastSearched: now, category: 'stable' },
];

const delay = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

const canReach = async (url) => {
  try {
    const response = await fetch(url);
    return response.status < 500;
  } catch {
    return false;
  }
};

const waitForServer = async (url) => {
  const deadline = Date.now() + 120_000;
  while (Date.now() < deadline) {
    if (await canReach(url)) return;
    await delay(1_000);
  }

  throw new Error(`Timed out waiting for ${url}`);
};

const startServerIfNeeded = async () => {
  if (await canReach(baseURL)) {
    return undefined;
  }

  const server = spawn(process.execPath, ['./node_modules/react-scripts/bin/react-scripts.js', 'start'], {
    cwd: frontendRoot,
    env: {
      ...process.env,
      BROWSER: 'none',
      HOST: '127.0.0.1',
      PORT: String(port),
      REACT_APP_API_URL: apiURL,
    },
    stdio: process.env.SCREENSHOT_DEBUG ? 'inherit' : 'ignore',
  });

  await waitForServer(baseURL);
  return server;
};

const registerApiMocks = async (page) => {
  await page.route('**/api/**', async (route) => {
    const request = route.request();
    const url = new URL(request.url());
    const method = request.method().toUpperCase();
    const routePath = url.pathname.replace(/^\/api/, '') || '/';
    const json = async (body, status = 200) =>
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

    if (method === 'OPTIONS') {
      await route.fulfill({ status: 204 });
      return;
    }

    if (method === 'POST' && (routePath === '/auth/register' || routePath === '/auth/login')) {
      await json({ token: 'screenshot-token' });
      return;
    }

    if (method === 'GET' && routePath === '/users/me') {
      await json(currentUser);
      return;
    }

    if (method === 'GET' && (routePath === '/posts/feed' || routePath === '/posts')) {
      await json({ items: posts, totalCount: posts.length, page: 1, pageSize: 10 });
      return;
    }

    if (method === 'GET' && routePath === '/search/popular/trending') {
      await json(popularSearches.slice(0, 2));
      return;
    }

    if (method === 'GET' && routePath === '/search/popular') {
      await json(popularSearches);
      return;
    }

    if (method === 'GET' && (routePath === '/search' || routePath === '/search/autocomplete')) {
      await json({
        posts: [posts[0]],
        users: [currentUser],
        hashtags: ['kafka', 'realtime'],
      });
      return;
    }

    await json({ message: `Unhandled screenshot route: ${method} ${routePath}` }, 404);
  });
};

let server;
let browser;

try {
  await mkdir(screenshotDir, { recursive: true });
  server = await startServerIfNeeded();
  browser = await chromium.launch();

  const publicContext = await browser.newContext({
    baseURL,
    viewport: { width: 1440, height: 1000 },
    deviceScaleFactor: 1,
  });
  const registerPage = await publicContext.newPage();
  await registerApiMocks(registerPage);
  await registerPage.goto('/register');
  await registerPage.screenshot({
    path: path.join(screenshotDir, 'jerrygram-register.png'),
    fullPage: true,
  });
  await publicContext.close();

  const signedInContext = await browser.newContext({
    baseURL,
    viewport: { width: 1440, height: 1100 },
    deviceScaleFactor: 1,
  });
  await signedInContext.addInitScript(() => {
    window.localStorage.setItem('token', 'screenshot-token');
  });

  const appPage = await signedInContext.newPage();
  await registerApiMocks(appPage);
  await appPage.goto('/');
  await appPage.getByRole('heading', { name: 'Home' }).waitFor();
  await appPage.screenshot({
    path: path.join(screenshotDir, 'jerrygram-feed.png'),
    fullPage: true,
  });

  await appPage.goto('/search');
  await appPage.getByRole('button', { name: 'kafka', exact: true }).waitFor();
  await appPage.screenshot({
    path: path.join(screenshotDir, 'jerrygram-search.png'),
    fullPage: true,
  });

  await signedInContext.close();
  console.log(`Screenshots saved to ${screenshotDir}`);
} finally {
  if (browser) {
    await browser.close();
  }
  if (server) {
    server.kill();
  }
}
