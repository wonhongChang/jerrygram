import pg from 'pg';
import dotenv from 'dotenv';

dotenv.config();

const { Pool } = pg;

const pool = new Pool({
  connectionString: process.env.DATABASE_URL
});

/**
 * Get recent liked post captions for a user.
 * @param {string} userId
 * @returns {Promise<string[]>}
 */
export async function getLikedCaptionsByUserId(userId) {
  const client = await pool.connect();
  try {
    const result = await client.query(
      `SELECT p."Caption"
       FROM "PostLikes" pl
       JOIN "Posts" p ON pl."PostId" = p."Id"
       WHERE pl."UserId" = $1
       ORDER BY pl."CreatedAt" DESC
       LIMIT 10`,
      [userId]
    );
    return result.rows.map(row => row.Caption).filter(Boolean);
  } finally {
    client.release();
  }
}

export async function getCandidatePostsFromDb() {
  const client = await pool.connect();
  try {
    const result = await client.query(
      `SELECT p."Id", p."Caption", p."ImageUrl", p."CreatedAt",
          COUNT(pl."Id") AS "Likes",
          u."Id" AS "UserId", u."Username", u."ProfileImageUrl"
        FROM "Posts" p
        JOIN "Users" u ON p."UserId" = u."Id"
        LEFT JOIN "PostLikes" pl ON pl."PostId" = p."Id"
        WHERE p."Visibility" = 0
        GROUP BY p."Id", p."Caption", p."ImageUrl", p."CreatedAt",
                    u."Id", u."Username", u."ProfileImageUrl"
        ORDER BY p."CreatedAt" DESC
        LIMIT 100`
    );

    return result.rows.map(row => ({
      id: row.Id,
      caption: row.Caption,
      imageUrl: row.ImageUrl,
      createdAt: row.CreatedAt,
      likes: parseInt(row.Likes, 10),
      liked: false,
      user: {
        id: row.UserId,
        username: row.Username,
        profileImageUrl: row.ProfileImageUrl
      }
    }));
  } finally {
    client.release();
  }
}