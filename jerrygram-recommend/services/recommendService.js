import { getEmbedding } from './embeddingService.js';
import { cosineSimilarity } from '../utils/cosine.js';
import { logger } from '../middleware/logger.js';
import { APP_CONFIG } from '../config/app.js';

/**
 * Recommend posts from candidates based on user caption embedding.
 * @param {string[]} userCaptions - recent liked captions
 * @param {Object[]} postCandidates - candidate posts with caption
 * @param {number} limit - maximum number of recommendations to return
 */
export async function recommendPosts(userCaptions, postCandidates, limit = APP_CONFIG.maxRecommendations) {
  try {
    if (!userCaptions?.length) {
      throw new Error('User captions are required');
    }

    if (!postCandidates?.length) {
      throw new Error('Post candidates are required');
    }

    const input = userCaptions.join('. ');
    logger.info(`Generating user embedding from ${userCaptions.length} captions`);
    
    const userVector = await getEmbedding(input);

    logger.info(`Calculating similarity scores for ${postCandidates.length} posts`);
    
    // Calculate similarity in batches to avoid overwhelming the API
    const batchSize = 10;
    const scored = [];
    
    for (let i = 0; i < postCandidates.length; i += batchSize) {
      const batch = postCandidates.slice(i, i + batchSize);
      
      const batchScored = await Promise.all(
        batch.map(async post => {
          try {
            const postVector = await getEmbedding(post.caption || '');
            const score = cosineSimilarity(userVector, postVector);
            
            // Create new post with score
            const scoredPost = { ...post, score };
            return scoredPost;
          } catch (error) {
            logger.warn(`Failed to score post ${post.id}:`, error.message);
            return { ...post, score: 0 };
          }
        })
      );
      
      scored.push(...batchScored);
    }

    const recommendations = scored
      .sort((a, b) => b.score - a.score)
      .slice(0, limit)
      .map(post => ({ ...post.toJSON?.() || post, score: post.score }));

    logger.success(`Generated ${recommendations.length} recommendations`);
    return recommendations;
    
  } catch (error) {
    logger.error('Failed to generate recommendations', error);
    throw error;
  }
}