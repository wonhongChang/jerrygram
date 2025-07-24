import express from 'express';
import { APP_CONFIG } from './config/app.js';
import { corsMiddleware } from './middleware/cors.js';
import { errorHandler, notFoundHandler } from './middleware/errorHandler.js';
import { requestLogger, logger } from './middleware/logger.js';
import { rateLimit, securityHeaders } from './middleware/security.js';
import { performanceMonitor } from './middleware/monitoring.js';
import routes from './routes/index.js';

const app = express();

// Security middleware
app.use(securityHeaders);
app.use(rateLimit(60000, 100)); // 100 requests per minute

// Monitoring middleware
app.use(performanceMonitor);

// Basic middleware
app.use(requestLogger);
app.use(corsMiddleware);
app.use(express.json({ limit: '1mb' }));
app.use(express.urlencoded({ extended: true, limit: '1mb' }));

// Routes
app.use('/', routes);

// Error handling
app.use(notFoundHandler);
app.use(errorHandler);

// Graceful shutdown
process.on('SIGTERM', () => {
  logger.info('SIGTERM received, shutting down gracefully');
  process.exit(0);
});

process.on('SIGINT', () => {
  logger.info('SIGINT received, shutting down gracefully');
  process.exit(0);
});

app.listen(APP_CONFIG.port, () => {
  logger.success(`🧠 Jerrygram Recommend API running on port ${APP_CONFIG.port}`);
  logger.info(`Environment: ${APP_CONFIG.nodeEnv}`);
  logger.info(`Cache enabled: ${APP_CONFIG.enableCache}`);
});