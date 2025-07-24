import { RecommendationRequest } from '../models/RecommendationRequest.js';
import { recommendPosts } from '../services/recommendService.js';
import { getCandidatePostsFromDb, getLikedCaptionsByUserId } from '../services/postRepository.js';
import { logger } from '../middleware/logger.js';
import { getHealthMetrics } from '../middleware/monitoring.js';

export const getRecommendations = async (req, res, next) => {
  try {
    const request = RecommendationRequest.fromQuery(req.query);
    request.validate();

    logger.info(`Processing recommendation request for user: ${request.userId}`);

    const [likedCaptions, candidates] = await Promise.all([
      getLikedCaptionsByUserId(request.userId),
      getCandidatePostsFromDb()
    ]);

    if (!candidates.length) {
      logger.warn('No candidate posts found');
      return res.json([]);
    }

    if (!likedCaptions.length) {
      logger.warn(`No liked captions found for user: ${request.userId}`);
      return res.json(candidates.slice(0, request.limit));
    }

    const recommended = await recommendPosts(likedCaptions, candidates, request.limit);
    
    logger.success(`Generated ${recommended.length} recommendations for user: ${request.userId}`);
    res.json(recommended);

  } catch (error) {
    next(error);
  }
};

export const healthCheck = async (req, res) => {
  const metrics = getHealthMetrics();
  res.json({
    ...metrics,
    version: '1.0.0',
    service: 'jerrygram-recommend'
  });
};