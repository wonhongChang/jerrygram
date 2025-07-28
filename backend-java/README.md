# Jerrygram Java Backend

A social media platform backend ported from .NET to Java Spring Boot, implementing Clean Architecture with CQRS pattern.

## Architecture Overview

This project follows Clean Architecture principles with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────────┐
│                        Presentation Layer                       │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐ │
│  │   Controllers   │  │   DTOs/Models   │  │  Exception      │ │
│  │   (REST API)    │  │   (Request/     │  │  Handlers       │ │
│  │                 │  │    Response)    │  │                 │ │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                       Application Layer                         │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐ │
│  │    Commands     │  │     Queries     │  │   Interfaces    │ │
│  │   (CQRS Write)  │  │   (CQRS Read)   │  │   (Abstractions)│ │
│  │                 │  │                 │  │                 │ │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘ │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐ │
│  │  Command        │  │    Query        │  │   Services      │ │
│  │  Handlers       │  │   Handlers      │  │  (Business      │ │
│  │                 │  │                 │  │   Logic)        │ │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                         Domain Layer                            │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐ │
│  │    Entities     │  │  Value Objects  │  │      Enums      │ │
│  │  (Core Models)  │  │   (Immutable    │  │   (Constants)   │ │
│  │                 │  │    Values)      │  │                 │ │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                     Infrastructure Layer                        │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐ │
│  │  Repositories   │  │   External      │  │  Configuration  │ │
│  │  (Data Access)  │  │   Services      │  │   (Security,    │ │
│  │                 │  │  (Blob, Cache,  │  │   Database,     │ │
│  │                 │  │   Search, etc.) │  │   etc.)         │ │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

## Technology Stack

### Core Framework
- **Spring Boot 3.2.1** - Main application framework
- **Java 21** - Programming language
- **Maven/Gradle** - Build tool and dependency management

### Database & Persistence
- **PostgreSQL** - Primary database
- **Spring Data JPA** - ORM framework
- **Hibernate** - JPA implementation
- **HikariCP** - Connection pooling

### Caching & Search
- **Redis** - Distributed caching
- **Elasticsearch** - Full-text search and indexing
- **Spring Cache** - Caching abstraction

### Security & Authentication
- **Spring Security** - Security framework
- **JWT (JSON Web Tokens)** - Authentication mechanism
- **BCrypt** - Password hashing

### Cloud & Storage
- **Azure Blob Storage** - File storage for images
- **Docker** - Containerization

### API & Documentation
- **Spring Web MVC** - REST API framework
- **Swagger/OpenAPI** - API documentation
- **Jackson** - JSON serialization/deserialization

## Porting Strategy from .NET to Java

### 1. Architecture Alignment
- **Clean Architecture**: Maintained the same layered architecture structure
- **CQRS Pattern**: Implemented Command Query Responsibility Segregation
- **Dependency Injection**: Used Spring's IoC container instead of .NET's built-in DI

### 2. Entity Framework → Spring Data JPA
```csharp
// .NET Entity Framework
public class User
{
    public Guid Id { get; set; }
    [Column("Username")]
    public string Username { get; set; }
}
```

```java
// Java JPA with Hibernate
@Entity
@Table(name = "\"Users\"")
public class User {
    @Id
    @Column(name = "\"Id\"")
    private UUID id;
    
    @Column(name = "\"Username\"")
    private String username;
}
```

### 3. Command/Query Pattern Implementation
```csharp
// .NET Command Handler
public class CreatePostCommandHandler : ICommandHandler<CreatePostCommand, PostDto>
{
    public async Task<PostDto> HandleAsync(CreatePostCommand command) { }
}
```

```java
// Java Command Handler
@Service
public class CreatePostCommandHandler implements ICommandHandler<CreatePostCommand, PostDto> {
    public PostDto handle(CreatePostCommand command) { }
}
```

### 4. Database Schema Compatibility
- **Case-sensitive naming**: PostgreSQL requires quoted identifiers for mixed-case columns
- **Enum handling**: Used ordinal values (0, 1, 2) instead of string values for database compatibility
- **UUID type mapping**: Proper UUID handling across both platforms

### 5. API Endpoint Alignment
- **Authentication**: Mapped `[Authorize]` → `@PreAuthorize` / SecurityConfig
- **Anonymous access**: `[AllowAnonymous]` → `permitAll()` in security configuration
- **Request handling**: `[FromForm]` → `@RequestParam` for multipart data
- **Response formatting**: Consistent DTO structures and JSON serialization

### 6. Service Layer Translation

#### Caching Strategy
```csharp
// .NET
_cacheService.RemoveByPattern("public_posts*");
await _cacheService.SetAsync(key, value, TimeSpan.FromMinutes(15));
```

```java
// Java
cacheService.deleteByPattern("public_posts*");
cacheService.set(key, value, Duration.ofMinutes(15));
```

#### Blob Storage
```csharp
// .NET
await _blobService.UploadAsync(stream, filename, BlobContainers.Post);
```

```java
// Java
blobService.upload(multipartFile, BlobContainers.POSTS);
```

### 7. Configuration Management
- **Environment variables**: Mapped from .NET's `appsettings.json` to Spring's `application.yml`
- **Profile-based config**: Development, staging, production profiles
- **Security**: Externalized secrets and credentials

## Key Design Decisions

### 1. CQRS Implementation
- **Commands**: Handle write operations (Create, Update, Delete)
- **Queries**: Handle read operations with caching
- **Handlers**: Separate handlers for each command/query
- **DTOs**: Dedicated data transfer objects for API contracts

### 2. Database Strategy
- **Single Database**: PostgreSQL for both read/write operations
- **Connection Pooling**: HikariCP for optimal performance
- **Transaction Management**: Spring's declarative transactions
- **Migration**: Hibernate schema validation/update

### 3. Caching Strategy
- **Multi-level caching**: Redis for distributed cache, local cache for frequently accessed data
- **Cache invalidation**: Pattern-based cache clearing on data mutations
- **TTL management**: Different expiration times based on data volatility

### 4. Security Implementation
- **JWT Authentication**: Stateless authentication with refresh tokens
- **Method-level security**: Fine-grained access control
- **CORS configuration**: Cross-origin resource sharing setup
- **Password encryption**: BCrypt hashing algorithm

### 5. Error Handling
- **Global exception handler**: Centralized error processing
- **Consistent error responses**: Uniform error format across all endpoints
- **Logging strategy**: Structured logging with correlation IDs

## API Compatibility

All endpoints maintain compatibility with the original .NET implementation:

### Authentication
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User authentication

### Posts Management
- `GET /api/posts` - Get public posts (paginated)
- `POST /api/posts` - Create new post (multipart/form-data)
- `GET /api/posts/{id}` - Get specific post
- `PUT /api/posts/{id}` - Update post
- `DELETE /api/posts/{id}` - Delete post

### User Management
- `GET /api/users/me` - Get current user profile
- `GET /api/users/profile/{username}` - Get user profile by username
- `POST /api/users/avatar` - Upload user avatar

### Social Features
- `POST /api/posts/{id}/like` - Toggle post like
- `GET /api/posts/{id}/likes` - Get post likes
- `GET /api/explore` - Get explore feed with recommendations

### Search
- `GET /api/search` - Search posts and users
- `GET /api/search/autocomplete` - Get search suggestions

## Performance Optimizations

1. **Connection Pooling**: HikariCP with optimized pool settings
2. **Query Optimization**: Efficient JPQL queries with proper indexing
3. **Caching Strategy**: Multi-tier caching with Redis and local cache
4. **Lazy Loading**: Proper handling of JPA lazy initialization
5. **Async Processing**: Non-blocking operations where appropriate

## Deployment

### Docker Support
```dockerfile
FROM openjdk:21-jdk-slim
COPY target/jerrygram-*.jar app.jar
ENTRYPOINT ["java", "-jar", "/app.jar"]
```

### Environment Configuration
```yaml
# application-prod.yml
spring:
  datasource:
    url: ${DATABASE_URL}
    username: ${DB_USERNAME}
    password: ${DB_PASSWORD}
  data:
    redis:
      host: ${REDIS_HOST}
      password: ${REDIS_PASSWORD}
```

## Testing Strategy

- **Unit Tests**: Service layer and utility classes
- **Integration Tests**: Repository layer with test containers
- **API Tests**: Controller layer with MockMvc
- **End-to-end Tests**: Full application flow testing

## Monitoring & Observability

- **Health Checks**: Spring Boot Actuator endpoints
- **Metrics**: Application performance monitoring
- **Logging**: Structured logging with correlation tracing
- **Error Tracking**: Exception monitoring and alerting

## Migration Notes

### Data Migration
- Database schema is compatible between .NET and Java versions
- Existing data requires no transformation
- Enum values stored as ordinal integers for cross-platform compatibility

### Feature Parity
- ✅ All .NET API endpoints implemented
- ✅ Authentication and authorization preserved
- ✅ Caching behavior maintained
- ✅ File upload functionality working
- ✅ Search and recommendations integrated

### Known Differences
- **Async/Await**: Java uses blocking I/O model instead of async/await
- **LINQ**: Replaced with Stream API and JPQL queries
- **Dependency Injection**: Spring's annotation-based DI vs .NET's constructor injection
- **Configuration**: YAML-based instead of JSON appsettings

## Getting Started

1. **Prerequisites**
   - Java 21+
   - PostgreSQL 17+
   - Redis 7.2+
   - Elasticsearch 8.18+

2. **Build & Run**
   ```bash
   ./gradlew build
   ./gradlew bootRun --args="--spring.profiles.active=dev"
   ```

3. **Docker Compose**
   ```bash
   docker-compose up -d
   ```

This Java implementation maintains full API compatibility with the original .NET version while leveraging Java ecosystem strengths for scalability and maintainability.