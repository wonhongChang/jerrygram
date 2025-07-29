# Jerrygram 📸

**Jerrygram** is a full-stack Instagram clone built as a portfolio project, demonstrating a modern tech stack and enterprise-grade architecture. It was originally built with an ASP.NET Core backend and has recently been **migrated to a Java Spring Boot** backend as the primary API. The app features a modern UI/UX design, cloud integration, native mobile apps (planned), and AI-driven features powered by OpenAI (chatbot, personalized recommendations).

## 🧰 Tech Stack

### Frontend

* React + TypeScript + Tailwind CSS (Web)

### Backend

* **ASP.NET Core (C#)** — Initial backend implementation using Clean Architecture and CQRS, demonstrating .NET enterprise patterns.
* **Spring Boot (Java)** — Java-based backend built with a similar architecture to showcase language-agnostic design principles.
* **Node.js (Express)** — AI-powered recommendation microservice (OpenAI GPT integration).
* **Redis** — Distributed caching with a hybrid in-memory fallback strategy.

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
* Input validation for all API requests (server-side enforcement)
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
* **Clean Architecture** with CQRS pattern for maintainability
* **Hybrid Caching Strategy** — Redis primary with memory fallback
* **Performance Optimization** — 10-30x faster response times with caching
* Comprehensive health monitoring and structured error handling

## 📁 Project Structure

```
jerrygram/
├── backend-dotnet/            # 🏢 ASP.NET Core API (original implementation)
│   ├── Domain/                      # Entities, enums, domain logic
│   ├── Application/                 # CQRS commands, queries, handlers, DTOs
│   ├── Infrastructure/              # External services (Redis, Elasticsearch, JWT)
│   ├── Persistence/                 # EF Core, repositories, migrations
│   └── WebApi/                      # Controllers, middleware, configurations
├── backend-java/              # ☕ Spring Boot API (Clean Architecture)
│   ├── application/           # DTOs, service interfaces, business logic
│   ├── domain/                # Entities, repository interfaces
│   ├── infrastructure/        # JPA repositories, Redis & Elasticsearch implementations
│   └── ...                    # Controllers, configurations, etc.
├── jerrygram-recommend/       # 🤖 AI Recommendation Service (Node.js)
│   ├── config/                     # Centralized configuration
│   ├── middleware/                 # Security, logging, monitoring
│   ├── controllers/                # Request handlers (Express endpoints)
│   ├── services/                   # OpenAI embeddings & recommendation logic
│   ├── models/                     # Data models
│   ├── cache/                      # Embedding caching system
│   └── validation/                 # Input validation
├── docker-compose.yml         # 🐳 Multi-service orchestration
├── frontend-react/            # React web frontend (planned)
├── mobile-android/            # Android app (planned)
└── mobile-ios/                # iOS app (planned)
```

### 🧱 Backend Architecture
Both backend implementations (Spring Boot and the original .NET Core) follow a **Clean Architecture** pattern with layered separation of concerns:
- **Domain Layer:** Core business entities and repository interfaces (encapsulating enterprise logic).
- **Application Layer:** Data Transfer Objects (DTOs), service interfaces, and business logic (implementing use cases; e.g. command and query handlers in .NET).
- **Infrastructure Layer:** Implementations for data access and external services (PostgreSQL via JPA/EF Core, Redis cache providers, Azure Blob Storage, Elasticsearch, etc.), configured via dependency injection.
- **Presentation Layer:** API endpoints (controllers) exposing application services via HTTP (Spring REST controllers or ASP.NET Web API controllers with Swagger). Controllers are kept thin, delegating to the application layer.
- **Separation of Concerns:** Domain models are decoupled from DTOs (with mappings between them). Dependencies point inward (inversion of control), resulting in a highly testable and maintainable codebase.

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
- **Redis** on port `6379` (caching layer)
- **Elasticsearch** on port `9200` (search engine)
- **AI Recommendation Service** on port `3001`

4. **Run the Spring Boot API locally:**
```bash
cd backend-java
./mvnw spring-boot:run
```

### Service Endpoints

- **Main API**: `http://localhost:8080`
- **API Documentation**: `http://localhost:8080/swagger-ui/`
- **Health Check**: `http://localhost:8080/api/health`
- **Recommendations**: `http://localhost:3001/recommend`
- **Recommendation Health**: `http://localhost:3001/health`

### Performance Characteristics

With Redis caching enabled:
- **Autocomplete Search**: 11-40ms (Redis) vs 370ms (cold start)
- **User Profiles**: 8-15ms (Redis) vs 150ms (database)
- **Public Posts**: 12-25ms (Redis) vs 200ms (database)

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

Project planning is managed using **Jira** (private).

Technical documentation and architecture guides are maintained in **Confluence** (internal).

## 🐳 Docker & DevOps

### Current Docker Setup ✅
- **Spring Boot API (Java 21)**: Primary backend service (REST API container)
- **PostgreSQL 17**: Database with persistent volume storage
- **Redis 7.2**: In-memory cache with disk persistence (hybrid caching strategy)
- **Elasticsearch 8.18**: Search and indexing engine
- **AI Recommendation Service**: Node.js microservice (Express, OpenAI integration)
- **Multi-service Orchestration**: Docker Compose for easy dev environment setup

### Production Deployment (Planned)
- **Spring Boot API**: Deployable via Azure Container Instances or AWS EKS (containerized backend)
- **Frontend**: Static web hosting (Azure App Service, Vercel, etc.)
- **Database**: Azure Database for PostgreSQL
- **Container Registry**: Azure Container Registry
- **Monitoring**: Azure Application Insights integration

### CI/CD Automation (GitHub Actions)
- **Continuous Integration**: GitHub Actions workflow builds & tests on every push (backend and frontend).
- **Docker Build & Push**: Automated Docker image build for services, pushed to registry (Docker Hub or Azure ACR).
- **Continuous Deployment**: Optionally deploys updated containers to cloud (e.g. Azure Web App or Kubernetes cluster) on main branch merges.

### Development Features
- **Hot Reload**: File watching for development
- **Health Monitoring**: Built-in health checks for all services
- **Logging**: Centralized logging with structured output
- **Environment Management**: Configurable settings per environment

## 📄 License

This project is licensed under the MIT License.
