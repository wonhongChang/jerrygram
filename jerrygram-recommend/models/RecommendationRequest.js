import { ValidationError } from './ValidationError.js';

export class RecommendationRequest {
  constructor({ userId, limit = 10 }) {
    this.userId = userId;
    this.limit = Math.min(limit, 50); // Cap at 50
  }

  static fromQuery(query) {
    return new RecommendationRequest({
      userId: query.userId,
      limit: parseInt(query.limit) || 10
    });
  }

  validate() {
    if (!this.userId) {
      throw new ValidationError('userId is required');
    }

    if (typeof this.userId !== 'string') {
      throw new ValidationError('userId must be a string');
    }

    if (this.limit < 1) {
      throw new ValidationError('limit must be at least 1');
    }

    return true;
  }
}