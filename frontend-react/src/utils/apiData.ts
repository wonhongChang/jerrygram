import { API_BASE_URL } from '../services/api';
import {
  Comment,
  Notification,
  NotificationType,
  PagedResult,
  PopularSearch,
  Post,
  SearchResult,
  SimpleUser,
  User,
} from '../types';

type AnyRecord = Record<string, any>;

const isRecord = (value: unknown): value is AnyRecord =>
  typeof value === 'object' && value !== null;

const toArray = <T = unknown>(value: unknown): T[] => (Array.isArray(value) ? value : []);

const firstDefined = (...values: unknown[]) => values.find((value) => value !== undefined && value !== null);

const toStringValue = (value: unknown, fallback = ''): string => {
  if (value === undefined || value === null) return fallback;
  return String(value);
};

const toErrorMessages = (value: unknown): string[] => {
  if (Array.isArray(value)) {
    return value.flatMap(toErrorMessages);
  }

  if (isRecord(value)) {
    return Object.values(value).flatMap(toErrorMessages);
  }

  const message = toStringValue(value).trim();
  return message ? [message] : [];
};

const toNumberValue = (value: unknown, fallback = 0): number => {
  const numberValue = Number(value);
  return Number.isFinite(numberValue) ? numberValue : fallback;
};

const toBooleanValue = (value: unknown, fallback = false): boolean => {
  if (typeof value === 'boolean') return value;
  if (typeof value === 'string') return value.toLowerCase() === 'true';
  if (typeof value === 'number') return value !== 0;
  return fallback;
};

export const normalizeIsoDate = (value: unknown): string => {
  const date = value ? new Date(String(value)) : new Date();
  return Number.isNaN(date.getTime()) ? new Date().toISOString() : date.toISOString();
};

export const resolveAssetUrl = (value?: string | null): string | undefined => {
  const raw = value?.trim();
  if (!raw) return undefined;
  if (/^(https?:|data:|blob:)/i.test(raw)) return raw;
  if (raw.startsWith('//')) return `https:${raw}`;

  try {
    const apiUrl = new URL(API_BASE_URL);
    const base = `${apiUrl.protocol}//${apiUrl.host}`;
    return new URL(raw.startsWith('/') ? raw : `/${raw}`, base).toString();
  } catch {
    return raw;
  }
};

export const getApiErrorMessage = (error: any, fallback = 'Something went wrong. Please try again.'): string => {
  const data = error?.response?.data;
  if (typeof data === 'string') return data;
  if (isRecord(data)) {
    const validationMessages = toErrorMessages(data.errors);
    if (validationMessages.length > 0) return validationMessages.join(' ');

    const message = firstDefined(data.message, data.error, data.title);
    if (message) return toStringValue(message, fallback);
  }
  return error?.message || fallback;
};

export const getApiValidationErrors = (error: any): Record<string, string> => {
  const data = error?.response?.data;
  if (!isRecord(data) || !isRecord(data.errors)) return {};

  return Object.entries(data.errors).reduce<Record<string, string>>((errors, [field, value]) => {
    const message = toErrorMessages(value).join(' ').trim();
    if (message) errors[field] = message;
    return errors;
  }, {});
};

export const normalizeSimpleUser = (value: unknown): SimpleUser => {
  if (typeof value === 'string') {
    return {
      id: value,
      username: value,
    };
  }

  const record = isRecord(value) ? value : {};
  const username = toStringValue(firstDefined(record.username, record.userName, record.name), 'unknown');

  return {
    id: toStringValue(firstDefined(record.id, record.userId, username), username),
    username,
    profileImageUrl: resolveAssetUrl(toStringValue(firstDefined(record.profileImageUrl, record.avatarUrl), '')),
  };
};

export const normalizeUser = (value: unknown): User => {
  const record = isRecord(value) ? value : {};
  const simpleUser = normalizeSimpleUser(record);

  return {
    ...simpleUser,
    email: toStringValue(record.email),
    followerCount: toNumberValue(firstDefined(record.followerCount, record.followers, record.followersCount)),
    followingCount: toNumberValue(firstDefined(record.followingCount, record.followings, record.followingsCount)),
    isFollowing: toBooleanValue(record.isFollowing),
  };
};

export const normalizePost = (value: unknown): Post => {
  const record = isRecord(value) ? value : {};
  const fallbackUsername = firstDefined(record.username, record.userName);
  const user = normalizeSimpleUser(firstDefined(record.user, fallbackUsername));

  return {
    id: toStringValue(record.id),
    caption: toStringValue(record.caption),
    imageUrl: resolveAssetUrl(toStringValue(firstDefined(record.imageUrl, record.image, record.photoUrl), '')) || '',
    createdAt: normalizeIsoDate(record.createdAt),
    likes: toNumberValue(firstDefined(record.likes, record.likeCount)),
    liked: toBooleanValue(firstDefined(record.liked, record.isLiked)),
    saved: toBooleanValue(firstDefined(record.saved, record.isSaved)),
    user,
    score: record.score === undefined || record.score === null ? undefined : toNumberValue(record.score),
  };
};

export const normalizeComment = (value: unknown): Comment => {
  const record = isRecord(value) ? value : {};

  return {
    id: toStringValue(record.id),
    content: toStringValue(record.content),
    createdAt: normalizeIsoDate(record.createdAt),
    user: normalizeSimpleUser(record.user),
  };
};

export const normalizeNotificationType = (value: unknown): NotificationType => {
  const normalized = toStringValue(value).toUpperCase();

  if (normalized === '1' || normalized === 'LIKE') return NotificationType.LIKE;
  if (normalized === '0' || normalized === 'COMMENT') return NotificationType.COMMENT;
  if (normalized === '2' || normalized === 'FOLLOW') return NotificationType.FOLLOW;

  return NotificationType.COMMENT;
};

export const normalizeNotification = (value: unknown): Notification => {
  const record = isRecord(value) ? value : {};

  return {
    id: toStringValue(record.id),
    message: toStringValue(record.message),
    type: normalizeNotificationType(record.type),
    createdAt: normalizeIsoDate(record.createdAt),
    isRead: toBooleanValue(record.isRead),
    fromUser: normalizeSimpleUser(record.fromUser),
    postId: record.postId ? toStringValue(record.postId) : undefined,
  };
};

export const normalizePagedResult = <T>(value: unknown, itemNormalizer: (item: unknown) => T): PagedResult<T> => {
  const record = isRecord(value) ? value : {};
  const rawItems = firstDefined(record.items, record.Items, record.content, Array.isArray(value) ? value : undefined);

  return {
    items: toArray(rawItems).map(itemNormalizer),
    totalCount: toNumberValue(firstDefined(record.totalCount, record.TotalCount, record.totalElements, toArray(rawItems).length)),
    page: toNumberValue(firstDefined(record.page, record.Page, record.number), 1),
    pageSize: toNumberValue(firstDefined(record.pageSize, record.PageSize, record.size, toArray(rawItems).length)),
  };
};

export const normalizeSearchResult = (value: unknown): SearchResult => {
  const record = isRecord(value) ? value : {};
  const tags = firstDefined(record.hashtags, record.tags);

  return {
    posts: toArray(record.posts).map(normalizePost).filter((post) => post.id),
    users: toArray(record.users).map(normalizeUser).filter((user) => user.username !== 'unknown'),
    hashtags: toArray(tags)
      .map((tag) => toStringValue(tag).replace(/^#/, ''))
      .filter(Boolean),
  };
};

export const normalizePopularSearch = (value: unknown): PopularSearch => {
  const record = isRecord(value) ? value : {};

  return {
    searchTerm: toStringValue(record.searchTerm),
    count: toNumberValue(record.count),
    rank: toNumberValue(record.rank),
    lastSearched: normalizeIsoDate(record.lastSearched),
    category: toStringValue(record.category, 'stable'),
  };
};

export const formatRelativeTime = (value: unknown): string => {
  const date = value ? new Date(String(value)) : new Date();
  if (Number.isNaN(date.getTime())) return 'just now';

  const seconds = Math.max(0, Math.floor((Date.now() - date.getTime()) / 1000));
  if (seconds < 60) return 'just now';
  const minutes = Math.floor(seconds / 60);
  if (minutes < 60) return `${minutes}m ago`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  if (days < 30) return `${days}d ago`;
  const months = Math.floor(days / 30);
  if (months < 12) return `${months}mo ago`;
  return `${Math.floor(months / 12)}y ago`;
};
