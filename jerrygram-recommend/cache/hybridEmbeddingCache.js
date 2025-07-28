import { redisEmbeddingCache } from './redisEmbeddingCache.js';
import { embeddingCache as memoryCache } from './embeddingCache.js';
import { logger } from '../middleware/logger.js';
import { APP_CONFIG } from '../config/app.js';

class HybridEmbeddingCache {
  constructor() {
    this.useRedis = process.env.USE_REDIS_CACHE === 'true' || APP_CONFIG.nodeEnv === 'production';
    this.primaryCache = this.useRedis ? redisEmbeddingCache : memoryCache;
    this.fallbackCache = this.useRedis ? memoryCache : null;
  }

  async get(text) {
    try {
      // Primary cache
      const result = await this.primaryCache.get(text);
      if (result) {
        return result;
      }
    } catch (error) {
      logger.warn('Primary cache failed for get:', error);
    }

    // Fallback cache
    if (this.fallbackCache) {
      try {
        return this.fallbackCache.get(text);
      } catch (error) {
        logger.warn('Fallback cache failed for get:', error);
      }
    }

    return null;
  }

  async set(text, embedding) {
    const promises = [];

    promises.push(
      this.primaryCache.set(text, embedding).catch(err => 
        logger.warn('Failed to set in primary cache:', err)
      )
    );

    if (this.fallbackCache) {
      promises.push(
        Promise.resolve(this.fallbackCache.set(text, embedding)).catch(err =>
          logger.warn('Failed to set in fallback cache:', err)
        )
      );
    }

    await Promise.allSettled(promises);
  }

  async clear() {
    const promises = [];

    promises.push(
      this.primaryCache.clear().catch(err =>
        logger.warn('Failed to clear primary cache:', err)
      )
    );

    if (this.fallbackCache) {
      promises.push(
        Promise.resolve(this.fallbackCache.clear()).catch(err =>
          logger.warn('Failed to clear fallback cache:', err)
        )
      );
    }

    await Promise.allSettled(promises);
  }

  async getStats() {
    try {
      return await this.primaryCache.getStats();
    } catch (error) {
      logger.warn('Failed to get primary cache stats:', error);
      if (this.fallbackCache) {
        try {
          return this.fallbackCache.getStats();
        } catch (fallbackError) {
          logger.warn('Failed to get fallback cache stats:', fallbackError);
        }
      }
      return { size: 0, memory: 0 };
    }
  }
}

export const hybridEmbeddingCache = new HybridEmbeddingCache();