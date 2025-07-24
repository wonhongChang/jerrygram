import { ValidationError } from '../models/ValidationError.js';

export const validateUserId = (userId) => {
  if (!userId) {
    throw new ValidationError('userId is required');
  }

  if (typeof userId !== 'string') {
    throw new ValidationError('userId must be a string');
  }

  if (userId.trim().length === 0) {
    throw new ValidationError('userId cannot be empty');
  }

  return true;
};

export const validateLimit = (limit) => {
  if (limit !== undefined) {
    const numLimit = Number(limit);
    
    if (isNaN(numLimit)) {
      throw new ValidationError('limit must be a number');
    }

    if (numLimit < 1) {
      throw new ValidationError('limit must be at least 1');
    }

    if (numLimit > 50) {
      throw new ValidationError('limit cannot exceed 50');
    }
  }

  return true;
};

export const validateText = (text, fieldName, maxLength = 1000) => {
  if (text && typeof text !== 'string') {
    throw new ValidationError(`${fieldName} must be a string`);
  }

  if (text && text.length > maxLength) {
    throw new ValidationError(`${fieldName} cannot exceed ${maxLength} characters`);
  }

  return true;
};

export const sanitizeInput = (input) => {
  if (typeof input !== 'string') return input;
  return input.trim().replace(/[<>]/g, '');
};