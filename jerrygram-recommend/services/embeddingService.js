import OpenAI from "openai";

const openai = new OpenAI({ apiKey: process.env.OPENAI_API_KEY });

/**
 * Convert a single caption into an embedding vector.
 */
export async function getEmbedding(text) {
  const res = await openai.embeddings.create({
    input: text,
    model: 'text-embedding-3-small'
  });

  return res.data[0].embedding; // Float[]
}