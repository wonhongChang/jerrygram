import dotenv from 'dotenv';

// Load environment variables, but don't fail if .env doesn't exist
dotenv.config({ path: '.env' });

export const APP_CONFIG = {
  port: parseInt(process.env.PORT) || 3001,
  nodeEnv: process.env.NODE_ENV || 'development',
  corsOrigins: process.env.CORS_ORIGINS?.split(',').filter(Boolean) || ['*'],
  
  // Recommendation settings
  maxCandidatePosts: Math.max(1, parseInt(process.env.MAX_CANDIDATE_POSTS) || 100),
  maxRecommendations: Math.max(1, parseInt(process.env.MAX_RECOMMENDATIONS) || 10),
  maxUserCaptions: Math.max(1, parseInt(process.env.MAX_USER_CAPTIONS) || 10),
  
  // Cache settings
  enableCache: process.env.ENABLE_CACHE === 'true' || process.env.NODE_ENV === 'production',
  cacheExpiry: Math.max(60, parseInt(process.env.CACHE_EXPIRY) || 3600), // Min 1 minute
};