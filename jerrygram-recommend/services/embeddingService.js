import { openai, EMBEDDING_CONFIG } from '../config/openai.js';
import { hybridEmbeddingCache } from '../cache/hybridEmbeddingCache.js';
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
    const cached = await hybridEmbeddingCache.get(normalizedText);
    if (cached) {
      return cached;
    }

    logger.info('Generating embedding for text (cache miss)');
    
    const response = await openai.embeddings.create({
      model: "text-embedding-ada-002",
      input: normalizedText,
    });

    const embedding = response.data[0].embedding;
    
    // Cache the result
    await hybridEmbeddingCache.set(normalizedText, embedding);
    
    return embedding;
  } catch (error) {
    logger.error('Failed to generate embedding', error);
    throw error;
  }
}