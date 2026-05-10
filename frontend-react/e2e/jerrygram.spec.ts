import { expect, test } from '@playwright/test';
import { demoPosts, mockJerrygramApi, signInForE2E } from './fixtures/jerrygramApi';

test('registers a user and lands on the personalized feed', async ({ page }) => {
  await mockJerrygramApi(page);
  await page.goto('/register');

  await expect(page.getByRole('heading', { name: 'Create account' })).toBeVisible();
  await page.getByLabel('Email').fill('jerry@example.com');
  await page.getByLabel('Username').fill('jerry');
  await page.getByLabel('Password').fill('CorrectHorseBatteryStaple1!');
  await page.getByRole('button', { name: 'Sign up' }).click();

  await expect(page).toHaveURL('/');
  await expect(page.getByRole('heading', { name: 'Home' })).toBeVisible();
  await expect(page.getByText(demoPosts[0].caption)).toBeVisible();
});

test('loads the feed and sends post interaction events', async ({ page }) => {
  const calls = await mockJerrygramApi(page);
  await signInForE2E(page);
  await page.goto('/');

  const kafkaPost = page.getByRole('article').filter({ hasText: demoPosts[0].caption });
  await expect(kafkaPost).toBeVisible();
  await kafkaPost.getByRole('button', { name: 'Like post' }).click();

  await expect.poll(() => calls.likePosts).toBe(1);
});

test('renders Kafka-backed search trends and results', async ({ page }) => {
  await mockJerrygramApi(page);
  await signInForE2E(page);
  await page.goto('/search');

  await expect(page.getByRole('button', { name: 'kafka', exact: true })).toBeVisible();
  await expect(page.getByRole('button', { name: 'realtime', exact: true })).toBeVisible();

  const searchInput = page.getByPlaceholder('Users, posts, or hashtags');
  await searchInput.fill('kafka');
  await expect(page.getByText('#kafka')).toBeVisible();

  await searchInput.press('Enter');
  await expect(page.getByRole('heading', { name: 'Posts' })).toBeVisible();
  await expect(page.getByAltText(demoPosts[0].caption)).toBeVisible();
});
