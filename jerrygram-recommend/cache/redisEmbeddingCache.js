import { redisClient } from '../config/redis.js';
import { logger } from '../utils/logger.js';
import { APP_CONFIG } from '../config/app.js';
import crypto from 'crypto';

class RedisEmbeddingCache {
  constructor() {
    this.enabled = APP_CONFIG.enableCache;
    this.expiry = APP_CONFIG.cacheExpiry;
    this.keyPrefix = 'jg:embedding:';
  }

  async get(text) {
    if (!this.enabled || !redisClient.isReady()) {
      return null;
    }

    try {
      const key = this.generateKey(text);
      const cached = await redisClient.getClient().get(key);
      
      if (!cached) {
        logger.debug('Redis cache miss for embedding');
        return null;
      }

      const parsed = JSON.parse(cached);
      logger.info('Redis cache hit for embedding');
      return parsed.embedding;
    } catch (error) {
      logger.error('Error getting embedding from Redis cache:', error);
      return null;
    }
  }

  async set(text, embedding) {
    if (!this.enabled || !redisClient.isReady()) {
      return;
    }

    try {
      const key = this.generateKey(text);
      const data = {
        embedding,
        timestamp: Date.now()
      };

      await redisClient.getClient().setEx(
        key, 
        this.expiry, 
        JSON.stringify(data)
      );
      
      logger.info('Embedding cached in Redis');
    } catch (error) {
      logger.error('Error setting embedding in Redis cache:', error);
    }
  }

  async clear() {
    if (!this.enabled || !redisClient.isReady()) {
      return;
    }

    try {
      const pattern = `${this.keyPrefix}*`;
      
      let cursor = 0;
      let deletedCount = 0;
      
      do {
        const reply = await redisClient.getClient().scan(cursor, {
          MATCH: pattern,
          COUNT: 100
        });
        
        cursor = reply.cursor;
        const keys = reply.keys;
        
        if (keys.length > 0) {
          await redisClient.getClient().del(keys);
          deletedCount += keys.length;
        }
      } while (cursor !== 0);
      
      logger.info(`Cleared ${deletedCount} embedding cache entries from Redis`);
    } catch (error) {
      logger.error('Error clearing Redis embedding cache:', error);
    }
  }

  async getStats() {
    if (!redisClient.isReady()) {
      return { size: 0, memory: 0 };
    }

    try {
      const pattern = `${this.keyPrefix}*`;
      let cursor = 0;
      let keyCount = 0;
      
      do {
        const reply = await redisClient.getClient().scan(cursor, {
          MATCH: pattern,
          COUNT: 100
        });
        
        cursor = reply.cursor;
        keyCount += reply.keys.length;
      } while (cursor !== 0);
      
      const info = await redisClient.getClient().memoryUsage('temp_key').catch(() => 0);
      
      return {
        size: keyCount,
        memory: info || 0
      };
    } catch (error) {
      logger.error('Error getting Redis cache stats:', error);
      return { size: 0, memory: 0 };
    }
  }

  generateKey(text) {
    const hash = crypto.createHash('sha256').update(text).digest('hex');
    return `${this.keyPrefix}${hash}`;
  }
}

export const redisEmbeddingCache = new RedisEmbeddingCache();