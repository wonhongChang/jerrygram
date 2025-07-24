# Jerrygram 📸

**Jerrygram** is a full-stack Instagram clone built as a portfolio project.
It features modern UI/UX design, cloud integration, native mobile apps, and an AI chatbot powered by Azure OpenAI.

## 🧰 Tech Stack

### Frontend

* React + TypeScript + Tailwind CSS (Web)

### Backend

* **ASP.NET Core (C#)** — Production-ready backend with enterprise features
* **Node.js** — AI-powered recommendation service with OpenAI integration  
* **Spring Boot (Java)** — secondary implementation (planned), hosted on AWS

### Mobile

* Android (Kotlin) — planned
* iOS (Swift) — planned

### Database

* PostgreSQL (managed via pgAdmin4)

### Cloud & Services

* Azure: Web API hosting, Blob Storage for image upload, Azure OpenAI for chatbot
* AWS: Bedrock, S3, EKS/Kubernetes

### AI & Machine Learning

* **OpenAI Integration** — GPT-powered post recommendations using embeddings
* **Azure OpenAI** — GPT-powered chatbot for user support (planned)
* **Elasticsearch** — Advanced search and content indexing

## 🧩 Features

### 🔐 **Authentication & Security**
* JWT-based user authentication with refresh tokens
* Input validation with FluentValidation
* Security headers and CORS protection
* Global exception handling

### 📱 **Social Media Core**
* Photo posting with Azure Blob Storage
* Like, comment, and follow system
* Real-time notifications
* Hashtag and mention support

### 🤖 **AI-Powered Features**
* **Smart Post Recommendations** using OpenAI embeddings
* Content-based similarity matching
* Personalized user feeds

### 🔍 **Search & Discovery**
* Elasticsearch-powered full-text search
* Advanced content indexing
* Explore page with trending content

### 📊 **Enterprise Features**
* Comprehensive health monitoring
* Performance metrics and caching
* Request logging and tracing
* Structured error handling

## 📁 Project Structure

```
jerrygram/
├── backend-dotnet/              # 🏢 ASP.NET Core API (Enterprise-Ready)
│   ├── Domain/                      # Entity models, value objects, enums
│   ├── Application/                 # CQRS (commands, queries, DTOs, handlers)
│   ├── Infrastructure/              # External services (e.g., Redis, OpenAI)
│   ├── Persistence/                 # DbContext, repositories, migrations
│   └── WebApi/                      # controllers, middleware
├── jerrygram-recommend/         # 🤖 AI Recommendation Service (Node.js)
│   ├── config/                      # Centralized configuration ✨
│   ├── middleware/                  # Security, logging, monitoring ✨
│   ├── controllers/                 # Request handlers ✨
│   ├── services/                    # OpenAI embeddings & recommendations
│   ├── models/                      # Data models ✨
│   ├── cache/                       # Embedding caching system ✨
│   ├── validation/                  # Input validation ✨
│   └── README.md                    # Service documentation ✨
├── docker-compose.yml           # 🐳 Multi-service orchestration
├── frontend-react/              # React web frontend (planned)
├── mobile-android/              # Android app (planned)
├── mobile-ios/                  # iOS app (planned)
└── docs/                        # Documentation & specs
```

## 🚀 Getting Started

### Quick Start with Docker

1. **Clone the repository:**
```bash
git clone https://github.com/wonhonChang/jerrygram.git
cd jerrygram
```

2. **Set up environment variables:**
```bash
# Create .env file for OpenAI API key
echo "OPENAI_API_KEY=your-openai-api-key" > .env
```

3. **Start all services:**
```bash
docker-compose up -d
```

This will start:
- **PostgreSQL** on port `15432`
- **Elasticsearch** on port `9200` 
- **AI Recommendation Service** on port `3001`

4. **Run .NET API locally:**
```bash
cd backend-dotnet/Jerrygram.Api
dotnet run
```

### Service Endpoints

- **Main API**: `https://localhost:5001`
- **API Documentation**: `https://localhost:5001/swagger`
- **Health Check**: `https://localhost:5001/api/health`
- **Recommendations**: `http://localhost:3001/recommend`
- **Recommendation Health**: `http://localhost:3001/health`

### Individual Service Setup

Each service has detailed setup instructions in its README:
- [`backend-dotnet/Jerrygram.Api/README.md`](backend-dotnet/Jerrygram.Api/README.md)
- [`jerrygram-recommend/README.md`](jerrygram-recommend/README.md)

## 🤖 AI-Powered Features

### Smart Recommendation Engine
- **Content-Based Filtering**: Uses OpenAI text embeddings to analyze post captions
- **Personalized Feeds**: Recommends posts based on user's like history
- **Real-time Processing**: Efficient caching and batch processing
- **Cosine Similarity**: Advanced similarity matching algorithms

### Architecture
- **Microservice Design**: Separate Node.js service for AI operations
- **Performance Optimized**: Embedding caching and rate limiting
- **Scalable**: Containerized with Docker for easy deployment
- **Monitoring**: Built-in health checks and performance metrics

### Future AI Features (Planned)
- **Azure OpenAI Chatbot**: User support and interaction
- **Image Recognition**: Auto-tagging and content moderation
- **Trend Analysis**: Hashtag and content trend predictions

## 📋 Project Management

Project planning is managed internally using **Jira** (private).

Technical documentation is maintained in **Confluence**:

🔗 [📘 Jerrygram Backend Documentation (Confluence)](https://jerryhub.atlassian.net/wiki/spaces/~5af0094aae7a832d555b8eae/folder/425989)  

## 🐳 Docker & DevOps

### Current Docker Setup ✅
- **PostgreSQL 17**: Database with persistent volumes
- **Elasticsearch 8.18**: Search and indexing service  
- **AI Recommendation Service**: Node.js microservice with OpenAI integration
- **Multi-service Orchestration**: Docker Compose for easy development

### Production Deployment (Planned)
- **ASP.NET Core API**: Azure Container Instances
- **Frontend**: Static web hosting (Azure/Vercel)
- **Database**: Azure Database for PostgreSQL
- **Container Registry**: Azure Container Registry
- **Monitoring**: Application Insights integration

### Development Features
- **Hot Reload**: File watching for development
- **Health Monitoring**: Built-in health checks for all services
- **Logging**: Centralized logging with structured output
- **Environment Management**: Configurable settings per environment

## 📄 License

This project is licensed under the MIT License.
