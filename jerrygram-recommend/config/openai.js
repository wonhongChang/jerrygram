import OpenAI from "openai";
import dotenv from 'dotenv';

dotenv.config();

if (!process.env.OPENAI_API_KEY) {
  throw new Error('OPENAI_API_KEY is required');
}

export const openai = new OpenAI({ 
  apiKey: process.env.OPENAI_API_KEY 
});

export const EMBEDDING_CONFIG = {
  model: 'text-embedding-3-small',
  maxTokens: 8191,
  dimensions: 1536
};