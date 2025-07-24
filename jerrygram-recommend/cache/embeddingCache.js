import { APP_CONFIG } from '../config/app.js';
import { logger } from '../middleware/logger.js';

class EmbeddingCache {
  constructor() {
    this.cache = new Map();
    this.enabled = APP_CONFIG.enableCache;
    this.expiry = APP_CONFIG.cacheExpiry * 1000; // Convert to milliseconds
  }

  _generateKey(text) {
    return Buffer.from(text).toString('base64');
  }

  get(text) {
    if (!this.enabled) return null;

    const key = this._generateKey(text);
    const cached = this.cache.get(key);

    if (!cached) return null;

    if (Date.now() - cached.timestamp > this.expiry) {
      this.cache.delete(key);
      return null;
    }

    logger.info('Cache hit for embedding');
    return cached.embedding;
  }

  set(text, embedding) {
    if (!this.enabled) return;

    const key = this._generateKey(text);
    this.cache.set(key, {
      embedding,
      timestamp: Date.now()
    });

    logger.info('Embedding cached');
  }

  clear() {
    this.cache.clear();
    logger.info('Embedding cache cleared');
  }

  size() {
    return this.cache.size;
  }

  // Clean up expired entries
  cleanup() {
    const now = Date.now();
    let cleaned = 0;

    for (const [key, value] of this.cache.entries()) {
      if (now - value.timestamp > this.expiry) {
        this.cache.delete(key);
        cleaned++;
      }
    }

    if (cleaned > 0) {
      logger.info(`Cleaned up ${cleaned} expired cache entries`);
    }
  }
}

export const embeddingCache = new EmbeddingCache();

// Cleanup expired entries every 30 minutes
if (APP_CONFIG.enableCache) {
  setInterval(() => {
    embeddingCache.cleanup();
  }, 30 * 60 * 1000);
}