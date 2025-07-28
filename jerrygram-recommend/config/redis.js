import { createClient } from 'redis';
import { logger } from '../middleware/logger.js';

class RedisClient {
  constructor() {
    this.client = null;
    this.isConnected = false;
    this.retryCount = 0;
    this.maxRetries = 3;
  }

  async connect() {
    const redisUrl = process.env.REDIS_URL || 'redis://localhost:6379';

    logger.info(`🔗 Connecting to Redis: ${redisUrl}`);

    try {
      this.client = createClient({
        url: redisUrl,
        socket: {
          reconnectStrategy: (retries) => {
            this.retryCount++;
            logger.warn(`♻️ Redis reconnect attempt ${this.retryCount}`);
            if (retries > this.maxRetries) {
              logger.error('❌ Redis max retries exceeded');
              return new Error('Max retries exceeded');
            }
            return Math.min(retries * 100, 2000);
          }
        }
      });

      this.client.on('connect', () => {
        logger.info('✅ Redis client connected');
        this.isConnected = true;
        this.retryCount = 0;
      });

      this.client.on('error', (err) => {
        logger.error('❌ Redis client error:', err);
        this.isConnected = false;
      });

      this.client.on('end', () => {
        logger.warn('🚫 Redis client connection closed');
        this.isConnected = false;
      });

      await this.client.connect();
    } catch (error) {
      logger.error('🚨 Failed to connect to Redis:', error);
      this.isConnected = false;
    }
  }

  async disconnect() {
    if (this.client) {
      await this.client.quit();
      this.isConnected = false;
      logger.info('🔌 Redis client disconnected');
    }
  }

  getClient() {
    return this.client;
  }

  isReady() {
    return this.client?.isReady && this.isConnected;
  }
}

export const redisClient = new RedisClient();
