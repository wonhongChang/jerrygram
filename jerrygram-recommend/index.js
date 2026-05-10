import express from 'express';
import { APP_CONFIG } from './config/app.js';
import { corsMiddleware } from './middleware/cors.js';
import { errorHandler, notFoundHandler } from './middleware/errorHandler.js';
import { requestLogger, logger } from './middleware/logger.js';
import { rateLimit, securityHeaders } from './middleware/security.js';
import { performanceMonitor } from './middleware/monitoring.js';
import { redisClient } from './config/redis.js';
import routes from './routes/index.js';

const app = express();

app.use(securityHeaders);
app.use(rateLimit(60000, 100));
app.use(performanceMonitor);
app.use(requestLogger);
app.use(corsMiddleware);
app.use(express.json({ limit: '1mb' }));
app.use(express.urlencoded({ extended: true, limit: '1mb' }));

app.use('/', routes);

app.use(notFoundHandler);
app.use(errorHandler);

async function startServer() {
  try {
    if (APP_CONFIG.redis.enabled) {
      try {
        await redisClient.connect();
        logger.info('Redis cache enabled and connected');
      } catch (error) {
        logger.warn('Redis connection failed, using memory cache only:', error);
      }
    } else {
      logger.info('Redis cache disabled, using memory cache only');
    }

    app.listen(APP_CONFIG.port, () => {
      logger.info(`Recommendation service running on port ${APP_CONFIG.port}`);
      logger.info(`Environment: ${APP_CONFIG.nodeEnv}`);
      logger.info(`Cache enabled: ${APP_CONFIG.enableCache}`);
      logger.info(`Redis enabled: ${APP_CONFIG.redis.enabled}`);
    });
  } catch (error) {
    logger.error('Failed to start server:', error);
    process.exit(1);
  }
}

async function shutdown(signal) {
  logger.info(`${signal} received, shutting down gracefully`);
  await redisClient.disconnect();
  process.exit(0);
}

process.on('SIGTERM', () => shutdown('SIGTERM'));
process.on('SIGINT', () => shutdown('SIGINT'));

startServer();
