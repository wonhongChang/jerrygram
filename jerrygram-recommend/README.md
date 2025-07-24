# Jerrygram Recommend API

AI-powered post recommendation service for Jerrygram social platform.

## 🏗️ Project Structure

```
jerrygram-recommend/
├── cache/              # Caching system
│   ├── embeddingCache.js   # Embedding result caching
│   └── index.js
├── config/             # Configuration management
│   ├── app.js             # Application settings
│   ├── database.js        # Database connection
│   └── openai.js          # OpenAI API configuration
├── controllers/        # Request handlers
│   ├── recommendController.js
│   └── index.js
├── middleware/         # Express middleware
│   ├── cors.js            # CORS configuration
│   ├── errorHandler.js    # Error handling
│   ├── logger.js          # Logging system
│   ├── monitoring.js      # Performance monitoring
│   ├── security.js        # Security headers & rate limiting
│   └── index.js
├── models/             # Data models
│   ├── Post.js            # Post model
│   ├── User.js            # User model
│   ├── RecommendationRequest.js
│   ├── ValidationError.js
│   └── index.js
├── routes/             # API routes
│   └── index.js
├── services/           # Business logic
│   ├── embeddingService.js    # OpenAI embedding generation
│   ├── postRepository.js     # Database queries
│   └── recommendService.js   # Recommendation algorithm
├── utils/              # Utilities
│   └── cosine.js          # Cosine similarity calculation
├── validation/         # Input validation
│   ├── validators.js
│   └── index.js
├── index.js            # Application entry point
└── .env.example        # Environment variables template
```

## 🚀 Features

- **AI-powered recommendations** using OpenAI embeddings
- **Caching system** for performance optimization
- **Rate limiting** and security headers
- **Performance monitoring** with memory and timing metrics
- **Comprehensive error handling** and logging
- **Input validation** and sanitization
- **Health check endpoint** with system metrics

## 📋 Environment Variables

Copy `.env.example` to `.env` and configure:

```bash
# Database
DATABASE_URL=postgresql://username:password@localhost:5432/database_name

# OpenAI
OPENAI_API_KEY=your_openai_api_key_here

# Server Configuration
PORT=3001
NODE_ENV=development
CORS_ORIGINS=http://localhost:3000,http://localhost:3001

# Recommendation Settings
MAX_CANDIDATE_POSTS=100
MAX_RECOMMENDATIONS=10
MAX_USER_CAPTIONS=10

# Cache Settings
ENABLE_CACHE=true
CACHE_EXPIRY=3600
```

## 🔌 API Endpoints

### GET /recommend
Get personalized post recommendations for a user.

**Query Parameters:**
- `userId` (required): User ID to generate recommendations for
- `limit` (optional): Number of recommendations to return (default: 10, max: 50)

**Example:**
```bash
curl "http://localhost:3001/recommend?userId=123&limit=5"
```

### GET /health
Health check endpoint with system metrics.

**Example Response:**
```json
{
  "status": "healthy",
  "uptime": "15 minutes",
  "memory": {
    "rss": "45.23MB",
    "heapUsed": "32.18MB",
    "heapTotal": "38.45MB",
    "external": "2.15MB"
  },
  "nodeVersion": "v18.17.0",
  "version": "1.0.0",
  "service": "jerrygram-recommend"
}
```

## 🛠️ Installation & Setup

1. Install dependencies:
```bash
npm install
```

2. Set up environment variables:
```bash
cp .env.example .env
# Edit .env with your configuration
```

3. Start the server:
```bash
npm start
```

## 🏭 Production Considerations

- Set `NODE_ENV=production`
- Enable caching with `ENABLE_CACHE=true`
- Configure proper CORS origins
- Set up proper logging and monitoring
- Use a process manager like PM2
- Implement proper database connection pooling
- Consider using Redis for caching in production

## 🔧 Monitoring

The application includes built-in monitoring features:

- **Performance monitoring**: Tracks slow requests and memory usage
- **Rate limiting**: Prevents API abuse
- **Health metrics**: Available at `/health` endpoint
- **Comprehensive logging**: Request/response logging with performance metrics

## 🚦 Error Handling

The application implements comprehensive error handling:

- **Validation errors**: 400 Bad Request
- **Rate limit exceeded**: 429 Too Many Requests
- **Database connection issues**: 503 Service Unavailable
- **AI service issues**: 502 Bad Gateway
- **Generic server errors**: 500 Internal Server Error

## 📈 Performance Optimizations

- **Embedding caching**: Reduces OpenAI API calls
- **Batch processing**: Processes embeddings in batches
- **Database connection pooling**: Efficient database connections
- **Memory monitoring**: Alerts for memory spikes
- **Request rate limiting**: Prevents system overload