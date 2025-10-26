using Application.Commands;
using Application.Commands.Auth;
using Application.Commands.Comments;
using Application.Commands.Posts;
using Application.Commands.Users;
using Application.Common;
using Application.DTOs;
using Application.Interfaces;
using Application.Queries;
using Application.Queries.Auth;
using Application.Queries.Comments;
using Application.Queries.Notifications;
using Application.Queries.Posts;
using Application.Queries.Users;
using Confluent.Kafka;
using Domain.Entities;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Nest;
using Persistence.Data;
using Persistence.Repositories;
using StackExchange.Redis;
using System.Text;
using WebApi.Configurations;
using WebApi.Middleware;

namespace WebApi.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();

            // Add Redis
            services.Configure<RedisCacheSettings>(config.GetSection("RedisCache"));
            services.Configure<CacheSettings>(config.GetSection("Cache"));

            var redisConnectionString = config.GetConnectionString("Redis");
            var redisPassword = config["RedisCache:Password"];

            if (!string.IsNullOrEmpty(redisPassword))
            {
                redisConnectionString += $",password={redisPassword}";
            }

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var configuration = ConfigurationOptions.Parse(redisConnectionString);
                configuration.AbortOnConnectFail = false;
                configuration.ConnectRetry = 2;
                configuration.ConnectTimeout = 1000;  // 1 second
                configuration.SyncTimeout = 1000;     // 1 second
                configuration.AsyncTimeout = 1000;    // 1 second
                configuration.CommandMap = CommandMap.Create(
                [
                    "FLUSHALL", "FLUSHDB", "SHUTDOWN", "DEBUG"
                ], available: false);
                return ConnectionMultiplexer.Connect(configuration);
            });

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "Jerrygram";
            });

            // Add FluentValidation
            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<WebApi.Validators.RegisterDtoValidator>();
            
            // Add CORS
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins(config.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" })
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Jerrygram API", Version = "v1" });

                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter 'Bearer {token}'",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };

                c.AddSecurityDefinition("Bearer", securityScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { securityScheme, new[] { "Bearer" } }
                });
            });

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

            services.Configure<JwtSettings>(config.GetSection("JwtSettings"));
            var jwtSettings = config.GetSection("JwtSettings").Get<JwtSettings>();
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    var key = Encoding.UTF8.GetBytes(jwtSettings!.SecretKey);
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtSettings.Audience,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };
                });

            services.AddSingleton<IElasticClient>(sp =>
            {
                var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
                    .DefaultMappingFor<Application.Common.PostIndex>(m => m.IndexName("posts"))
                    .DefaultMappingFor<Application.Common.UserIndex>(m => m.IndexName("users"))
                    .EnableDebugMode();

                return new ElasticClient(settings);
            });

            services.AddHttpClient<IRecommendClient, RecommendClient>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:3001");
                client.Timeout = TimeSpan.FromSeconds(3);
            });

            services.Configure<KafkaSettings>(config.GetSection(KafkaSettings.SectionName));

            var kafkaSettings = config.GetSection(KafkaSettings.SectionName).Get<KafkaSettings>();
            if (kafkaSettings?.EnableEventPublishing == true)
            {
                // Register Kafka Producer
                services.AddSingleton<IProducer<string, string>>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<IProducer<string, string>>>();

                    var producerConfig = new ProducerConfig
                    {
                        BootstrapServers = kafkaSettings.BootstrapServers,
                        ClientId = kafkaSettings.ClientId,
                        EnableIdempotence = kafkaSettings.EnableIdempotence,
                        Acks = Acks.All,
                        MessageTimeoutMs = kafkaSettings.MessageTimeoutMs,
                        RequestTimeoutMs = kafkaSettings.RequestTimeoutMs,
                        SecurityProtocol = SecurityProtocol.Plaintext,
                        // Resilience settings
                        SocketTimeoutMs = 60000,
                        ReconnectBackoffMs = 100,
                        ReconnectBackoffMaxMs = 10000
                    };

                    var producer = new ProducerBuilder<string, string>(producerConfig)
                        .SetValueSerializer(Serializers.Utf8)
                        .SetKeySerializer(Serializers.Utf8)
                        .SetErrorHandler((_, e) => logger.LogError("Kafka producer error: {Error}", e.Reason))
                        .SetLogHandler((_, message) => logger.LogDebug("Kafka: {Message}", message.Message))
                        .Build();

                    logger.LogInformation("Kafka producer initialized with servers: {Servers}", kafkaSettings.BootstrapServers);
                    return producer;
                });

                // Register Event Service
                services.AddScoped<IEventService, EventService>();
            }
            else
            {
                // Register no-op event service for disabled state
                services.AddScoped<IEventService, NoOpEventService>();
            }

            // Register Infrastructure Services
            // services.AddScoped<IAuthService, AuthService>();
            services.AddSingleton<IBlobService, BlobService>();
            services.AddScoped<IElasticService, ElasticService>();
            services.AddScoped<ISearchService, SearchService>();
            services.AddScoped<IJwtService, JwtService>();

            // Cache Services
            services.AddMemoryCache();
            services.AddScoped<CacheService>();
            services.AddScoped<RedisCacheService>();
            services.AddScoped<ICacheService, HybridCacheService>();

            // Register Command Handlers
            services.AddScoped<ICommandHandler<RegisterUserCommand, (string token, User user)>, RegisterUserCommandHandler>();
            services.AddScoped<ICommandHandler<CreatePostCommand, PostListItemDto>, CreatePostCommandHandler>();
            services.AddScoped<ICommandHandler<LikePostCommand>, LikePostCommandHandler>();
            services.AddScoped<ICommandHandler<DeletePostCommand>, DeletePostCommandHandler>();
            services.AddScoped<ICommandHandler<UpdatePostCommand, Post>, UpdatePostCommandHandler>();
            services.AddScoped<ICommandHandler<UnlikePostCommand>, UnlikePostCommandHandler>();
            services.AddScoped<ICommandHandler<FollowUserCommand>, FollowUserCommandHandler>();
            services.AddScoped<ICommandHandler<UnfollowUserCommand>, UnfollowUserCommandHandler>();
            services.AddScoped<ICommandHandler<UploadAvatarCommand, object>, UploadAvatarCommandHandler>();
            services.AddScoped<ICommandHandler<CreateCommentCommand, CommentResponseDto>, CreateCommentCommandHandler>();
            services.AddScoped<ICommandHandler<DeleteCommentCommand>, DeleteCommentCommandHandler>();

            // Register Query Handlers
            services.AddScoped<IQueryHandler<LoginQuery, string>, LoginQueryHandler>();
            services.AddScoped<IQueryHandler<GetPostByIdQuery, PostListItemDto>, GetPostByIdQueryHandler>();
            services.AddScoped<IQueryHandler<GetPublicPostsQuery, PagedResult<PostListItemDto>>, GetPublicPostsQueryHandler>();
            services.AddScoped<IQueryHandler<GetUserFeedQuery, PagedResult<PostListItemDto>>, GetUserFeedQueryHandler>();
            services.AddScoped<IQueryHandler<GetPostLikesQuery, PagedResult<SimpleUserDto>>, GetPostLikesQueryHandler>();
            services.AddScoped<IQueryHandler<GetExplorePostsQuery, List<PostListItemDto>>, GetExplorePostsQueryHandler>();
            services.AddScoped<IQueryHandler<GetCurrentUserQuery, object>, GetCurrentUserQueryHandler>();
            services.AddScoped<IQueryHandler<GetUserProfileQuery, object>, GetUserProfileQueryHandler>();
            services.AddScoped<IQueryHandler<GetFollowersQuery, object>, GetFollowersQueryHandler>();
            services.AddScoped<IQueryHandler<GetFollowingsQuery, object>, GetFollowingsQueryHandler>();
            services.AddScoped<IQueryHandler<GetCommentsQuery, object>, GetCommentsQueryHandler>();
            services.AddScoped<IQueryHandler<GetNotificationsQuery, PagedResult<NotificationResponseDto>>, GetNotificationsQueryHandler>();

            // Register Generic Repository
            services.AddScoped(typeof(Application.Interfaces.IRepository<>), typeof(Repository<>));
            
            // Register Specific Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<IPostLikeRepository, PostLikeRepository>();
            services.AddScoped<IPostTagRepository, PostTagRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IUserFollowRepository, UserFollowRepository>();
        }

        public static async Task ConfigureMiddleware(this WebApplication app)
        {
            // Security headers (should be first)
            app.UseMiddleware<SecurityHeadersMiddleware>();

            // Request logging
            app.UseMiddleware<RequestLoggingMiddleware>();
            
            // Global exception handling
            app.UseMiddleware<GlobalExceptionMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.Migrate();


            if (!context.Users.Any(u => u.Email == "jerry@gram.com"))
            {
                var user = new User
                {
                    Email = "jerry@gram.com",
                    Username = "jerry",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pa$$w0rd!")
                };
                context.Users.Add(user);
                context.SaveChanges();
            }
        }
    }
}