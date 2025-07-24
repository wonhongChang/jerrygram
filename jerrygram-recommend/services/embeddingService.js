import { openai, EMBEDDING_CONFIG } from '../config/openai.js';
import { embeddingCache } from '../cache/embeddingCache.js';
import { logger } from '../middleware/logger.js';
import { validateText } from '../validation/validators.js';

/**
 * Convert a single caption into an embedding vector.
 * @param {string} text - The text to embed
 * @returns {Promise<number[]>} The embedding vector
 */
export async function getEmbedding(text) {
  try {
    validateText(text, 'text', EMBEDDING_CONFIG.maxTokens);
    
    if (!text || text.trim().length === 0) {
      throw new Error('Text cannot be empty');
    }

    // Check cache first
    const cached = embeddingCache.get(text);
    if (cached) {
      return cached;
    }

    logger.info('Generating embedding for text');
    
    const res = await openai.embeddings.create({
      input: text.trim(),
      model: EMBEDDING_CONFIG.model
    });

    const embedding = res.data[0].embedding;
    
    // Cache the result
    embeddingCache.set(text, embedding);
    
    return embedding;
  } catch (error) {
    logger.error('Failed to generate embedding', error);
    throw error;
  }
}