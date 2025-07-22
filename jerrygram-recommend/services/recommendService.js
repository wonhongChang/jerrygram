import { getEmbedding } from './embeddingService.js';
import { cosineSimilarity } from '../utils/cosine.js';

/**
 * Recommend posts from candidates based on user caption embedding.
 * @param {string[]} userCaptions - recent liked captions
 * @param {Object[]} postCandidates - candidate posts with caption
 */
export async function recommendPosts(userCaptions, postCandidates) {
    // 🔧 TODO: Replace fallback captions with user-liked caption lookup from DB
  const fallbackCaptions = [
    "Watching the sunset at the beach",
    "Exploring the mountain trail"
  ];
  const captionsToUse = userCaptions?.length > 0 ? userCaptions : fallbackCaptions;
  const input = captionsToUse.join('. ');

  //const input = userCaptions.join('. ');
  const userVector = await getEmbedding(input);

  // calculate similarity
  const scored = await Promise.all(
    postCandidates.map(async post => {
      const postVector = await getEmbedding(post.caption);
      const score = cosineSimilarity(userVector, postVector);
      return { ...post, score };
    })
  );

  return scored
    .sort((a, b) => b.score - a.score)
    .slice(0, 10); // return top 10
}