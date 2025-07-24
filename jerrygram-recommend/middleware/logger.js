export const requestLogger = (req, res, next) => {
  const start = Date.now();
  
  const originalSend = res.send;
  res.send = function(data) {
    const duration = Date.now() - start;
    console.log(`${req.method} ${req.path} - ${res.statusCode} - ${duration}ms`);
    return originalSend.call(this, data);
  };
  
  next();
};

export const logger = {
  info: (message, meta = {}) => {
    console.log(`ℹ️  ${message}`, meta);
  },
  
  warn: (message, meta = {}) => {
    console.warn(`⚠️  ${message}`, meta);
  },
  
  error: (message, error = null) => {
    console.error(`❌ ${message}`, error);
  },
  
  success: (message, meta = {}) => {
    console.log(`✅ ${message}`, meta);
  }
};