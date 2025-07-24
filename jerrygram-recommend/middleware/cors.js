import cors from 'cors';
import { APP_CONFIG } from '../config/app.js';

export const corsOptions = {
  origin: (origin, callback) => {
    if (APP_CONFIG.corsOrigins.includes('*')) {
      return callback(null, true);
    }
    
    if (!origin || APP_CONFIG.corsOrigins.includes(origin)) {
      return callback(null, true);
    }
    
    callback(new Error('Not allowed by CORS'));
  },
  credentials: true,
  methods: ['GET', 'POST', 'PUT', 'DELETE', 'OPTIONS'],
  allowedHeaders: ['Content-Type', 'Authorization']
};

export const corsMiddleware = cors(corsOptions);