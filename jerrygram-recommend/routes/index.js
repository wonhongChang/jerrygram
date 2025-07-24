import express from 'express';
import { getRecommendations, healthCheck } from '../controllers/index.js';

const router = express.Router();

router.get('/recommend', getRecommendations);
router.get('/health', healthCheck);

export default router;