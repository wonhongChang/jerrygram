import { logger } from './logger.js';

// Simple performance monitoring
export const performanceMonitor = (req, res, next) => {
  const start = process.hrtime.bigint();
  const startMemory = process.memoryUsage();
  
  const originalSend = res.send;
  res.send = function(data) {
    const end = process.hrtime.bigint();
    const endMemory = process.memoryUsage();
    
    const duration = Number(end - start) / 1000000; // Convert to milliseconds
    const memoryDiff = endMemory.heapUsed - startMemory.heapUsed;
    
    // Log slow requests
    if (duration > 1000) { // More than 1 second
      logger.warn(`Slow request detected: ${req.method} ${req.path}`, {
        duration: `${duration.toFixed(2)}ms`,
        memoryDiff: `${(memoryDiff / 1024 / 1024).toFixed(2)}MB`,
        statusCode: res.statusCode
      });
    }
    
    // Log memory spikes
    if (Math.abs(memoryDiff) > 50 * 1024 * 1024) { // More than 50MB
      logger.warn(`Memory spike detected: ${req.method} ${req.path}`, {
        memoryDiff: `${(memoryDiff / 1024 / 1024).toFixed(2)}MB`,
        currentHeap: `${(endMemory.heapUsed / 1024 / 1024).toFixed(2)}MB`
      });
    }
    
    return originalSend.call(this, data);
  };
  
  next();
};

// Health metrics endpoint
export const getHealthMetrics = () => {
  const memUsage = process.memoryUsage();
  const uptime = process.uptime();
  
  return {
    status: 'healthy',
    uptime: `${Math.floor(uptime / 60)} minutes`,
    memory: {
      rss: `${(memUsage.rss / 1024 / 1024).toFixed(2)}MB`,
      heapUsed: `${(memUsage.heapUsed / 1024 / 1024).toFixed(2)}MB`,
      heapTotal: `${(memUsage.heapTotal / 1024 / 1024).toFixed(2)}MB`,
      external: `${(memUsage.external / 1024 / 1024).toFixed(2)}MB`
    },
    loadAverage: process.platform !== 'win32' ? process.loadavg() : 'N/A (Windows)',
    nodeVersion: process.version,
    timestamp: new Date().toISOString()
  };
};