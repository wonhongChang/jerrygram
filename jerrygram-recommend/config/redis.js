import { createClient } from 'redis';
import { logger } from '../utils/logger.js';

import { createClient } from 'redis';
import { logger } from '../utils/logger.js';

class RedisClient {
  constructor() {
    this.client = null;
    this.isConnected = false;
    this.retryCount = 0;
    this.maxRetries = 3;
  }

  async connect() {
    try {
      const redisUrl = process.env.REDIS_URL || 'redis://localhost:6379';
      const redisPassword = process.env.REDIS_PASSWORD;
      
      let connectionOptions = {
        url: redisUrl,
        retry_strategy: (times) => {
          if (times > this.maxRetries) {
            logger.error('Redis max retries exceeded');
            return null;
          }
          return Math.min(times * 50, 2000);
        }
      };

      if (redisPassword) {
        connectionOptions.password = redisPassword;
      }

      this.client = createClient(connectionOptions);

      this.client.on('connect', () => {
        logger.info('Redis client connected');
        this.isConnected = true;
        this.retryCount = 0;
      });

      this.client.on('error', (err) => {
        logger.error('Redis client error:', err);
        this.isConnected = false;
      });

      this.client.on('reconnecting', () => {
        this.retryCount++;
        logger.info(`Redis client reconnecting... (attempt ${this.retryCount})`);
      });

      this.client.on('end', () => {
        logger.info('Redis client disconnected');
        this.isConnected = false;
      });

      await this.client.connect();
      logger.info('Redis connected successfully');
    } catch (error) {
      logger.error('Failed to connect to Redis:', error);
      this.isConnected = false;
    }
  }

  async disconnect() {
    if (this.client) {
      await this.client.quit();
      this.isConnected = false;
      logger.info('Redis client disconnected');
    }
  }

  getClient() {
    return this.client;
  }

  isReady() {
    return this.isConnected && this.client?.isReady;
  }
}

export const redisClient = new RedisClient();