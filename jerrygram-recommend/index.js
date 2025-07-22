import express from 'express';
import cors from 'cors';
import dotenv from 'dotenv';
import { recommendPosts } from './services/recommendService.js';
import { getCandidatePostsFromDb, getLikedCaptionsByUserId } from './services/postRepository.js';

// Load environment variables from .env
dotenv.config();

const app = express();
const PORT = process.env.PORT || 3001;

app.use(cors());
app.use(express.json());

// Mock recommend endpoint
app.get('/recommend', async (req, res) => {
  const userId = req.query.userId;

  if (!userId) {
    return res.status(400).json({ error: 'Missing userId query parameter' });
  }

  try {
    // TODO: Replace with DB lookup
    const likedCaptions = await getLikedCaptionsByUserId(userId);
    const candidates = await getCandidatePostsFromDb();

    if (!candidates.length) {
      console.warn('⚠️ No candidate posts found');
      return res.json([]);
    }


    const recommended = await recommendPosts(likedCaptions, candidates);
    res.json(recommended);
  } catch (err) {
    console.error('❌ AI recommendation failed:', err);
    res.status(500).json({ error: 'AI recommendation failed' });
  }
});

app.listen(PORT, () => {
  console.log(`🧠 Jerrygram Recommend API running on port ${PORT}`);
});