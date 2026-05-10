import test from 'node:test';
import assert from 'node:assert/strict';

import { Post } from '../models/Post.js';
import { RecommendationRequest } from '../models/RecommendationRequest.js';
import { ValidationError } from '../models/ValidationError.js';
import { cosineSimilarity } from '../utils/cosine.js';

test('cosineSimilarity scores matching and unrelated vectors', () => {
  assert.equal(cosineSimilarity([1, 0, 1], [1, 0, 1]), 0.9999999999999998);
  assert.equal(cosineSimilarity([1, 0], [0, 1]), 0);
});

test('Post maps database rows to API JSON', () => {
  const post = Post.fromDbRow({
    Id: 'post-1',
    Caption: 'Kafka search trends',
    ImageUrl: 'https://example.test/post.png',
    UserId: 'user-1',
    Username: 'jerry',
    ProfileImageUrl: 'https://example.test/profile.png',
    Likes: '7',
    CreatedAt: '2026-05-10T00:00:00Z',
  });

  post.score = 0.75;

  assert.deepEqual(post.toJSON(), {
    id: 'post-1',
    caption: 'Kafka search trends',
    imageUrl: 'https://example.test/post.png',
    likes: 7,
    liked: false,
    user: {
      id: 'user-1',
      username: 'jerry',
      profileImageUrl: 'https://example.test/profile.png',
    },
    createdAt: '2026-05-10T00:00:00Z',
    score: 0.75,
  });
});

test('RecommendationRequest normalizes and validates query input', () => {
  const request = RecommendationRequest.fromQuery({ userId: 'user-1', limit: '500' });

  assert.equal(request.userId, 'user-1');
  assert.equal(request.limit, 50);
  assert.doesNotThrow(() => request.validate());

  assert.throws(
    () => RecommendationRequest.fromQuery({ limit: '10' }).validate(),
    ValidationError,
  );
});
