import { pool } from '../config/database.js';
import { Post } from '../models/Post.js';
import { APP_CONFIG } from '../config/app.js';
import { logger } from '../middleware/logger.js';
import { validateUserId } from '../validation/validators.js';

/**
 * Get recent liked post captions for a user.
 * @param {string} userId
 * @returns {Promise<string[]>}
 */
export async function getLikedCaptionsByUserId(userId) {
  validateUserId(userId);
logger.info(`DATABASE_URL: ${process.env.DATABASE_URL}`);
  const client = await pool.connect();
  try {
    logger.info(`Fetching liked captions for user: ${userId}`);
    
    const result = await client.query(
      `SELECT p."Caption"
       FROM "PostLikes" pl
       JOIN "Posts" p ON pl."PostId" = p."Id"
       WHERE pl."UserId" = $1
       ORDER BY pl."CreatedAt" DESC
       LIMIT $2`,
      [userId, APP_CONFIG.maxUserCaptions]
    );
    
    const captions = result.rows.map(row => row.Caption).filter(Boolean);
    logger.info(`Found ${captions.length} liked captions for user: ${userId}`);
    
    return captions;
  } catch (error) {
    logger.error(`Failed to fetch liked captions for user: ${userId}`, error);
    throw error;
  } finally {
    client.release();
  }
}

export async function getCandidatePostsFromDb() {
  const client = await pool.connect();
  try {
    logger.info('Fetching candidate posts from database');
    
    const result = await client.query(
      `SELECT p."Id", p."Caption", p."ImageUrl", p."CreatedAt",
          COUNT(pl."Id") AS "Likes",
          u."Id" AS "UserId", u."Username", u."ProfileImageUrl"
        FROM "Posts" p
        JOIN "Users" u ON p."UserId" = u."Id"
        LEFT JOIN "PostLikes" pl ON pl."PostId" = p."Id"
        WHERE p."Visibility" = 0 AND p."Caption" IS NOT NULL AND p."Caption" != ''
        GROUP BY p."Id", p."Caption", p."ImageUrl", p."CreatedAt",
                    u."Id", u."Username", u."ProfileImageUrl"
        ORDER BY p."CreatedAt" DESC
        LIMIT $1`,
      [APP_CONFIG.maxCandidatePosts]
    );

    const posts = result.rows.map(row => Post.fromDbRow(row));
    logger.info(`Found ${posts.length} candidate posts`);
    
    return posts;
  } catch (error) {
    logger.error('Failed to fetch candidate posts', error);
    throw error;
  } finally {
    client.release();
  }
}